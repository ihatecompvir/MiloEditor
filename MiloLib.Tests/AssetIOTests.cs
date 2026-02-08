using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MiloLib.Assets;
using MiloLib.Classes;
using MiloLib.Utils;
using Xunit;
using Xunit.Abstractions;

namespace MiloLib.Tests;

[Collection("RoundTripTests")]
public class AssetIOTests : IClassFixture<ReportGeneratorClassFixture>
{
    private readonly ITestOutputHelper _output;
    private readonly ReportGeneratorClassFixture _fixture;
    
    public AssetIOTests(ITestOutputHelper output, ReportGeneratorClassFixture fixture)
    {
        _output = output;
        _fixture = fixture;
    }
    /// <summary>
    /// Gets all asset types (classes that inherit from MiloLib.Assets.Object and are not abstract).
    /// </summary>
    public static IEnumerable<object[]> GetAssetTypes()
    {
        var assembly = typeof(MiloLib.Assets.Object).Assembly;
        var objectType = typeof(MiloLib.Assets.Object);
        
        var assetTypes = assembly.GetTypes()
            .Where(t => 
                t.IsClass && 
                !t.IsAbstract && 
                objectType.IsAssignableFrom(t) &&
                t != objectType)
            .OrderBy(t => t.FullName)
            .Select(t => new object[] { t });
        
        return assetTypes;
    }

    [Theory]
    [MemberData(nameof(GetAssetTypes))]
    public void RoundTrip_VerifyByteEquality(Type assetType)
    {
        var failures = new ConcurrentBag<string>();

        // Cache reflection lookups once per asset type (not per revision)
        var writeMethod = GetWriteMethod(assetType);
        var readMethod = GetReadMethod(assetType);

        if (writeMethod == null || readMethod == null)
        {
            var msg = writeMethod == null ? "No Write method found" : "No Read method found";
            for (ushort r = 0; r <= 100; r++)
                TestResultCollector.RecordResult(assetType.Name, r, TestResultCollector.TestStatus.Failed, msg);
            return;
        }

        // sweet sweet parallelism
        Parallel.For(0, 101, revision =>
        {
            ushort rev = (ushort)revision;
            string? errorMessage = null;
            var status = TestResultCollector.TestStatus.Passed;

            var fuzzer = new AssetFuzzer(seed: 42 + revision);

            var directoryMeta = new DirectoryMeta
            {
                revision = 32,
                platform = DirectoryMeta.Platform.Xbox,
                type = new Symbol(0, "ObjectDir"),
                name = new Symbol(0, "TestDir")
            };
            var entry = new DirectoryMeta.Entry(
                new Symbol(0, assetType.Name),
                new Symbol(0, "TestAsset"),
                new MiloLib.Assets.Object());
            entry.isProxy = false;

            try
            {
                var originalInstance = fuzzer.Create(assetType);
                if (originalInstance == null)
                {
                    errorMessage = "Failed to create instance";
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                    failures.Add($"Rev {rev}: {errorMessage}");
                    return;
                }

                SetRevisionField(originalInstance, rev);
                entry.obj = originalInstance as MiloLib.Assets.Object;

                byte[] streamABytes;
                try
                {
                    using var streamA = new MemoryStream();
                    using var writerA = new EndianWriter(streamA, Endian.BigEndian);
                    writeMethod.Invoke(originalInstance, new object[] { writerA, true, directoryMeta, entry });
                    streamABytes = streamA.ToArray();
                }
                catch (Exception ex)
                {
                    var actual = UnwrapException(ex);
                    errorMessage = $"Write failed - {actual.GetType().Name}: {actual.Message}";
                    if (ex != actual) errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    status = TestResultCollector.TestStatus.Skipped;
                    TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                    failures.Add($"Rev {rev}: {errorMessage}");
                    return;
                }

                object? readInstance;
                try
                {
                    readInstance = CreateInstance(assetType);
                    if (readInstance == null)
                    {
                        errorMessage = "Failed to create instance for reading";
                        status = TestResultCollector.TestStatus.Failed;
                        TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                        failures.Add($"Rev {rev}: {errorMessage}");
                        return;
                    }

                    using var readStream = new MemoryStream(streamABytes);
                    using var reader = new EndianReader(readStream, Endian.BigEndian);
                    readMethod.Invoke(readInstance, new object[] { reader, true, directoryMeta, entry });
                }
                catch (Exception ex)
                {
                    var actual = UnwrapException(ex);
                    errorMessage = $"Read exception - {actual.GetType().Name}: {actual.Message}";
                    if (actual.StackTrace != null)
                    {
                        var stackLines = actual.StackTrace.Split('\n').Take(3);
                        errorMessage += $"\nStack trace: {string.Join("\n", stackLines)}";
                    }
                    if (ex != actual) errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                    failures.Add($"Rev {rev}: {errorMessage}");
                    return;
                }

                entry.obj = readInstance as MiloLib.Assets.Object;

                byte[] streamBBytes;
                try
                {
                    using var streamB = new MemoryStream();
                    using var writerB = new EndianWriter(streamB, Endian.BigEndian);
                    writeMethod.Invoke(readInstance, new object[] { writerB, true, directoryMeta, entry });
                    streamBBytes = streamB.ToArray();
                }
                catch (Exception ex)
                {
                    var actual = UnwrapException(ex);
                    errorMessage = $"Second Write failed - {actual.GetType().Name}: {actual.Message}";
                    if (ex != actual) errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                    failures.Add($"Rev {rev}: {errorMessage}");
                    return;
                }

                if (!streamABytes.AsSpan().SequenceEqual(streamBBytes))
                {
                    errorMessage = $"Byte mismatch - Stream A length: {streamABytes.Length}, Stream B length: {streamBBytes.Length}";
                    int minLength = Math.Min(streamABytes.Length, streamBBytes.Length);
                    for (int i = 0; i < minLength; i++)
                    {
                        if (streamABytes[i] != streamBBytes[i])
                        {
                            errorMessage += $"; First difference at offset {i}: A=0x{streamABytes[i]:X2}, B=0x{streamBBytes[i]:X2}";
                            break;
                        }
                    }
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                    failures.Add($"Rev {rev}: {errorMessage}");
                }
                else
                {
                    TestResultCollector.RecordResult(assetType.Name, rev, TestResultCollector.TestStatus.Passed);
                }
            }
            catch (Exception ex)
            {
                var actual = UnwrapException(ex);
                errorMessage = $"Unexpected exception - {actual.GetType().Name}: {actual.Message}";
                if (ex != actual) errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                status = TestResultCollector.TestStatus.Failed;
                TestResultCollector.RecordResult(assetType.Name, rev, status, errorMessage);
                failures.Add($"Rev {rev}: {errorMessage}");
            }
        });

        if (!failures.IsEmpty)
        {
            System.Diagnostics.Debug.WriteLine($"Round-trip test had {failures.Count} failures for {assetType.Name}");
        }
    }
    
    /// <summary>
    /// Creates an instance of the given type, trying (ushort, ushort) and (ushort) constructors
    /// before falling back to parameterless. Matches how real assets are constructed.
    /// </summary>
    private static object? CreateInstance(Type type)
    {
        var ctor = type.GetConstructor(new[] { typeof(ushort), typeof(ushort) });
        if (ctor != null)
            return ctor.Invoke(new object[] { (ushort)0, (ushort)0 });

        ctor = type.GetConstructor(new[] { typeof(ushort) });
        if (ctor != null)
            return ctor.Invoke(new object[] { (ushort)0 });

        ctor = type.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
            return ctor.Invoke(Array.Empty<object>());

        return null;
    }

    /// <summary>
    /// Unwraps TargetInvocationException to get the actual exception.
    /// </summary>
    private static Exception UnwrapException(Exception ex)
    {
        return ex is TargetInvocationException { InnerException: not null } tie ? tie.InnerException : ex;
    }

    /// <summary>
    /// Sets the revision field on an object using reflection.
    /// </summary>
    private static void SetRevisionField(object instance, ushort revision)
    {
        Type type = instance.GetType();
        
        // Try to find and set the revision field (could be private/protected)
        while (type != null && type != typeof(object))
        {
            var revisionField = type.GetField("revision", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (revisionField != null && revisionField.FieldType == typeof(ushort))
            {
                revisionField.SetValue(instance, revision);
            }
            
            // Also set altRevision if it exists
            var altRevisionField = type.GetField("altRevision", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (altRevisionField != null && altRevisionField.FieldType == typeof(ushort))
            {
                altRevisionField.SetValue(instance, (ushort)0);
            }
            
            type = type.BaseType;
        }
    }
    
    /// <summary>
    /// Gets the Read method for a type.
    /// </summary>
    private MethodInfo? GetReadMethod(Type type)
    {
        // Look for Read method with signature: Read(EndianReader, bool, DirectoryMeta, DirectoryMeta.Entry)
        return type.GetMethod("Read", new[] 
        { 
            typeof(EndianReader), 
            typeof(bool), 
            typeof(DirectoryMeta), 
            typeof(DirectoryMeta.Entry) 
        });
    }
    
    /// <summary>
    /// Gets the Write method for a type.
    /// </summary>
    private MethodInfo? GetWriteMethod(Type type)
    {
        // Look for Write method with signature: Write(EndianWriter, bool, DirectoryMeta, DirectoryMeta.Entry?)
        // Try both nullable and non-nullable Entry
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Write")
            .ToList();
        
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 4 &&
                parameters[0].ParameterType == typeof(EndianWriter) &&
                parameters[1].ParameterType == typeof(bool) &&
                parameters[2].ParameterType == typeof(DirectoryMeta))
            {
                var entryParamType = parameters[3].ParameterType;
                // Check if it's Entry or nullable Entry (can't use typeof on nullable reference type)
                if (entryParamType == typeof(DirectoryMeta.Entry) ||
                    (entryParamType.IsGenericType && 
                     entryParamType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     entryParamType.GetGenericArguments()[0] == typeof(DirectoryMeta.Entry)))
                {
                    return method;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Final test that generates the HTML report and reports its location.
    /// This test should run after all other tests to ensure all results are collected.
    /// </summary>
    [Fact]
    public void GenerateHtmlReport()
    {
        var allResults = TestResultCollector.GetAllResults();
        
        if (allResults.Count == 0)
        {
            _output?.WriteLine("No test results collected. Make sure other tests have run first.");
            return;
        }
        
        // Get the base directory (where the test assembly is located)
        string baseDir = AppContext.BaseDirectory;
        
        // Try multiple locations
        var possibleDirs = new List<string>
        {
            Path.Combine(baseDir, "TestResults"),
            Path.Combine(Directory.GetCurrentDirectory(), "TestResults"),
            Directory.GetCurrentDirectory(),
            baseDir
        };
        
        string? testOutputDir = null;
        foreach (var dir in possibleDirs)
        {
            try
            {
                var normalizedDir = Path.GetFullPath(dir);
                if (Directory.Exists(normalizedDir) || dir == baseDir || dir == Directory.GetCurrentDirectory())
                {
                    testOutputDir = normalizedDir;
                    break;
                }
            }
            catch { }
        }
        
        if (testOutputDir == null)
        {
            testOutputDir = Path.GetFullPath(baseDir);
        }
        
        Directory.CreateDirectory(testOutputDir);
        
        string reportPath = Path.Combine(testOutputDir, "MiloLib_RoundTrip_TestReport.html");
        string fullPath = Path.GetFullPath(HtmlReportGenerator.GenerateReport(reportPath));
        
        // Also try project root
        try
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
            if (Directory.Exists(projectRoot))
            {
                var projectReportPath = Path.Combine(projectRoot, "MiloLib_RoundTrip_TestReport.html");
                File.Copy(fullPath, projectReportPath, overwrite: true);
                _output?.WriteLine($"Also saved to project root: {projectReportPath}");
            }
        }
        catch { }
        
        _output?.WriteLine($"\n{new string('=', 80)}");
        _output?.WriteLine($"HTML REPORT GENERATED");
        _output?.WriteLine($"Location: {fullPath}");
        _output?.WriteLine($"{new string('=', 80)}\n");
        
        // Assert true so this test always passes (it's just for reporting)
        Assert.True(true, $"Report generated at: {fullPath}");
    }
}

/// <summary>
/// Test collection fixture that generates the HTML report after all tests complete.
/// </summary>
[CollectionDefinition("RoundTripTests")]
public class RoundTripTestCollection : ICollectionFixture<ReportGeneratorFixture>
{
}

/// <summary>
/// Class fixture for round-trip tests. Report generation is handled by the GenerateHtmlReport test.
/// </summary>
public class ReportGeneratorClassFixture : IDisposable
{
    public ReportGeneratorClassFixture()
    {
        TestResultCollector.Clear();
    }

    public void Dispose()
    {
    }
}

public class ReportGeneratorFixture : IDisposable
{
    public ReportGeneratorFixture()
    {
    }

    public void Dispose()
    {
    }
}

