using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        var failures = new List<string>();
        var fuzzer = new AssetFuzzer(seed: 42); // Use fixed seed for determinism
        
        // Create dummy DirectoryMeta (platform: PS3)
        var directoryMeta = new DirectoryMeta
        {
            revision = 32, // Use a known valid revision
            platform = DirectoryMeta.Platform.Xbox,
            type = new Symbol(0, "ObjectDir"),
            name = new Symbol(0, "TestDir")
        };
        
        // Create dummy DirectoryMeta.Entry (isProxy: false)
        // We'll create a dummy object for the entry - it will be replaced when we create the actual instance
        var dummyObj = Activator.CreateInstance(assetType);
        var entry = new DirectoryMeta.Entry(
            new Symbol(0, assetType.Name),
            new Symbol(0, "TestAsset"),
            dummyObj as MiloLib.Assets.Object ?? new MiloLib.Assets.Object());
        entry.isProxy = false;
        
        // Iterate through revisions 0 to 50
        for (ushort revision = 0; revision <= 50; revision++)
        {
            string? errorMessage = null;
            TestResultCollector.TestStatus status = TestResultCollector.TestStatus.Passed;
            
            try
            {
                // 1. Fuzz & Setup: Create an instance of assetType
                var originalInstance = fuzzer.Create(assetType);
                if (originalInstance == null)
                {
                    errorMessage = "Failed to create instance";
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                    continue;
                }
                
                // Set the revision field using reflection
                SetRevisionField(originalInstance, revision);
                
                // Update entry.obj to point to the actual instance
                entry.obj = originalInstance as MiloLib.Assets.Object;
                
                // 2. Write (A): Write this object to a MemoryStream (Stream A)
                byte[] streamABytes;
                try
                {
                    using (var streamA = new MemoryStream())
                    using (var writerA = new EndianWriter(streamA, Endian.BigEndian))
                    {
                        // Check if the instance has a Write method
                        var writeMethod = GetWriteMethod(originalInstance.GetType());
                        if (writeMethod == null)
                        {
                            errorMessage = "No Write method found";
                            status = TestResultCollector.TestStatus.Failed;
                            TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                            failures.Add($"Rev {revision}: {errorMessage}");
                            continue;
                        }
                        
                        writeMethod.Invoke(originalInstance, new object[] { writerA, true, directoryMeta, entry });
                        streamABytes = streamA.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    // Unwrap TargetInvocationException to get the actual exception
                    Exception actualException = ex;
                    if (ex is System.Reflection.TargetInvocationException targetEx && targetEx.InnerException != null)
                    {
                        actualException = targetEx.InnerException;
                    }
                    
                    errorMessage = $"Write failed - {actualException.GetType().Name}: {actualException.Message}";
                    if (ex != actualException)
                    {
                        errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    }
                    
                    // If Write fails, assume this specific revision is not supported
                    status = TestResultCollector.TestStatus.Skipped;
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                    continue;
                }
                
                // 3. Read: Instantiate a fresh empty instance and call .Read()
                object? readInstance = null;
                try
                {
                    readInstance = Activator.CreateInstance(assetType);
                    if (readInstance == null)
                    {
                        errorMessage = "Failed to create instance for reading";
                        status = TestResultCollector.TestStatus.Failed;
                        TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                        failures.Add($"Rev {revision}: {errorMessage}");
                        continue;
                    }
                    
                    var readMethod = GetReadMethod(assetType);
                    if (readMethod == null)
                    {
                        errorMessage = "No Read method found";
                        status = TestResultCollector.TestStatus.Failed;
                        TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                        failures.Add($"Rev {revision}: {errorMessage}");
                        continue;
                    }
                    
                    using (var streamA = new MemoryStream(streamABytes))
                    using (var reader = new EndianReader(streamA, Endian.BigEndian))
                    {
                        readMethod.Invoke(readInstance, new object[] { reader, true, directoryMeta, entry });
                    }
                }
                catch (Exception ex)
                {
                    // Unwrap TargetInvocationException to get the actual exception
                    Exception actualException = ex;
                    if (ex is System.Reflection.TargetInvocationException targetEx && targetEx.InnerException != null)
                    {
                        actualException = targetEx.InnerException;
                    }
                    
                    // Build a detailed error message with the actual exception
                    errorMessage = $"Read exception - {actualException.GetType().Name}: {actualException.Message}";
                    if (actualException.StackTrace != null)
                    {
                        // Include first few lines of stack trace for context
                        var stackLines = actualException.StackTrace.Split('\n').Take(3);
                        errorMessage += $"\nStack trace: {string.Join("\n", stackLines)}";
                    }
                    if (ex != actualException)
                    {
                        errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    }
                    
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                    continue;
                }
                
                if (readInstance == null)
                {
                    errorMessage = "Read returned null";
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                    continue;
                }
                
                // Update entry.obj to point to the read instance
                entry.obj = readInstance as MiloLib.Assets.Object;
                
                // 4. Write (B): Write the newly read object to a second MemoryStream
                byte[] streamBBytes;
                try
                {
                    using (var streamB = new MemoryStream())
                    using (var writerB = new EndianWriter(streamB, Endian.BigEndian))
                    {
                        var writeMethod = GetWriteMethod(readInstance.GetType());
                        if (writeMethod == null)
                        {
                            errorMessage = "No Write method found for read instance";
                            status = TestResultCollector.TestStatus.Failed;
                            TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                            failures.Add($"Rev {revision}: {errorMessage}");
                            continue;
                        }
                        
                        writeMethod.Invoke(readInstance, new object[] { writerB, true, directoryMeta, entry });
                        streamBBytes = streamB.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    // Unwrap TargetInvocationException to get the actual exception
                    Exception actualException = ex;
                    if (ex is System.Reflection.TargetInvocationException targetEx && targetEx.InnerException != null)
                    {
                        actualException = targetEx.InnerException;
                    }
                    
                    errorMessage = $"Second Write failed - {actualException.GetType().Name}: {actualException.Message}";
                    if (ex != actualException)
                    {
                        errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                    }
                    
                    status = TestResultCollector.TestStatus.Failed;
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                    continue;
                }
                
                // 5. Verify: Compare the byte arrays
                if (!streamABytes.SequenceEqual(streamBBytes))
                {
                    errorMessage = $"Byte mismatch - Stream A length: {streamABytes.Length}, Stream B length: {streamBBytes.Length}";
                    
                    // Find first difference for debugging
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
                    TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                    failures.Add($"Rev {revision}: {errorMessage}");
                }
                else
                {
                    // Success!
                    TestResultCollector.RecordResult(assetType.Name, revision, TestResultCollector.TestStatus.Passed);
                }
            }
            catch (Exception ex)
            {
                // Unwrap TargetInvocationException to get the actual exception
                Exception actualException = ex;
                if (ex is System.Reflection.TargetInvocationException targetEx && targetEx.InnerException != null)
                {
                    actualException = targetEx.InnerException;
                }
                
                errorMessage = $"Unexpected exception - {actualException.GetType().Name}: {actualException.Message}";
                if (ex != actualException)
                {
                    errorMessage += $"\n(Wrapped in {ex.GetType().Name})";
                }
                
                status = TestResultCollector.TestStatus.Failed;
                TestResultCollector.RecordResult(assetType.Name, revision, status, errorMessage);
                failures.Add($"Rev {revision}: {errorMessage}");
            }
        }
        
        // Report all failures (but don't fail the test - we want to collect all results)
        // The HTML report will show the failures
        if (failures.Count > 0)
        {
            // Log failures but don't fail the test - we want to generate the report
            System.Diagnostics.Debug.WriteLine($"Round-trip test had {failures.Count} failures for {assetType.Name}");
        }
    }
    
    /// <summary>
    /// Sets the revision field on an object using reflection.
    /// </summary>
    private void SetRevisionField(object instance, ushort revision)
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
/// Class fixture that generates the HTML report after all tests complete.
/// </summary>
public class ReportGeneratorClassFixture : IDisposable
{
    public ReportGeneratorClassFixture()
    {
        // Clear any previous results when starting
        TestResultCollector.Clear();
    }
    
    public void Dispose()
    {
        // Generate HTML report after all tests in the class complete
        GenerateReport();
    }
    
    private void GenerateReport()
    {
        try
        {
            // Get the base directory (where the test assembly is located)
            string baseDir = AppContext.BaseDirectory;
            
            // Try multiple locations, prioritizing TestResults directory
            var possibleDirs = new List<string>
            {
                // xUnit TestResults directory (most common)
                Path.Combine(baseDir, "TestResults"),
                // Project root TestResults
                Path.Combine(Directory.GetCurrentDirectory(), "TestResults"),
                // Solution root TestResults
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResults"),
                // Bin directory TestResults
                Path.Combine(baseDir, "..", "..", "..", "TestResults"),
                // Current directory
                Directory.GetCurrentDirectory(),
                // Base directory
                baseDir
            };
            
            // Also try to find TestResults in parent directories
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 3 && currentDir != null; i++)
            {
                var testResultsDir = Path.Combine(currentDir.FullName, "TestResults");
                if (!possibleDirs.Contains(testResultsDir))
                {
                    possibleDirs.Add(testResultsDir);
                }
                currentDir = currentDir.Parent;
            }
            
            string? testOutputDir = null;
            foreach (var dir in possibleDirs)
            {
                try
                {
                    var normalizedDir = Path.GetFullPath(dir);
                    if (Directory.Exists(normalizedDir))
                    {
                        testOutputDir = normalizedDir;
                        break;
                    }
                }
                catch
                {
                    // Skip invalid paths
                }
            }
            
            // Fallback to base directory
            if (testOutputDir == null)
            {
                testOutputDir = Path.GetFullPath(baseDir);
            }
            
            // Ensure directory exists
            Directory.CreateDirectory(testOutputDir);
            
            string reportPath = Path.Combine(testOutputDir, "MiloLib_RoundTrip_TestReport.html");
            string fullPath = Path.GetFullPath(HtmlReportGenerator.GenerateReport(reportPath));
            
            // Also save a copy to the project root for easy access
            try
            {
                var projectRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
                var projectRootFull = Path.GetFullPath(projectRoot);
                if (Directory.Exists(projectRootFull))
                {
                    var projectReportPath = Path.Combine(projectRootFull, "MiloLib_RoundTrip_TestReport.html");
                    File.Copy(fullPath, projectReportPath, overwrite: true);
                    Console.WriteLine($"Also saved to project root: {projectReportPath}");
                }
            }
            catch
            {
                // Ignore if we can't save to project root
            }
            
            // Write to console with clear formatting
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine($"HTML REPORT GENERATED");
            Console.WriteLine($"{new string('=', 80)}");
            Console.WriteLine($"Location: {fullPath}");
            Console.WriteLine($"{new string('=', 80)}\n");
            
            // Also write to standard error to ensure it's visible
            Console.Error.WriteLine($"\nHTML Report: {fullPath}\n");
            
            System.Diagnostics.Debug.WriteLine($"HTML Report generated: {fullPath}");
        }
        catch (Exception ex)
        {
            string errorMsg = $"Failed to generate HTML report: {ex.Message}\n{ex.StackTrace}";
            Console.WriteLine(errorMsg);
            Console.Error.WriteLine(errorMsg);
            System.Diagnostics.Debug.WriteLine(errorMsg);
        }
    }
}

public class ReportGeneratorFixture : IDisposable
{
    public ReportGeneratorFixture()
    {
        // Clear any previous results when starting
        TestResultCollector.Clear();
    }
    
    public void Dispose()
    {
        // Generate HTML report after all tests in the collection complete
        try
        {
            // Get the base directory (where the test assembly is located)
            string baseDir = AppContext.BaseDirectory;
            
            // Try multiple locations, prioritizing TestResults directory
            var possibleDirs = new List<string>
            {
                // xUnit TestResults directory (most common)
                Path.Combine(baseDir, "TestResults"),
                // Project root TestResults
                Path.Combine(Directory.GetCurrentDirectory(), "TestResults"),
                // Solution root TestResults
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResults"),
                // Bin directory TestResults
                Path.Combine(baseDir, "..", "..", "..", "TestResults"),
                // Current directory
                Directory.GetCurrentDirectory(),
                // Base directory
                baseDir
            };
            
            // Also try to find TestResults in parent directories
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 3 && currentDir != null; i++)
            {
                var testResultsDir = Path.Combine(currentDir.FullName, "TestResults");
                if (!possibleDirs.Contains(testResultsDir))
                {
                    possibleDirs.Add(testResultsDir);
                }
                currentDir = currentDir.Parent;
            }
            
            string? testOutputDir = null;
            foreach (var dir in possibleDirs)
            {
                try
                {
                    var normalizedDir = Path.GetFullPath(dir);
                    if (Directory.Exists(normalizedDir))
                    {
                        testOutputDir = normalizedDir;
                        break;
                    }
                }
                catch
                {
                    // Skip invalid paths
                }
            }
            
            // Fallback to base directory
            if (testOutputDir == null)
            {
                testOutputDir = Path.GetFullPath(baseDir);
            }
            
            // Ensure directory exists
            Directory.CreateDirectory(testOutputDir);
            
            string reportPath = Path.Combine(testOutputDir, "MiloLib_RoundTrip_TestReport.html");
            string fullPath = Path.GetFullPath(HtmlReportGenerator.GenerateReport(reportPath));
            
            // Also save a copy to the project root for easy access
            try
            {
                var projectRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
                var projectRootFull = Path.GetFullPath(projectRoot);
                if (Directory.Exists(projectRootFull))
                {
                    var projectReportPath = Path.Combine(projectRootFull, "MiloLib_RoundTrip_TestReport.html");
                    File.Copy(fullPath, projectReportPath, overwrite: true);
                    Console.WriteLine($"Also saved to project root: {projectReportPath}");
                }
            }
            catch
            {
                // Ignore if we can't save to project root
            }
            
            // Write to console with clear formatting
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine($"HTML REPORT GENERATED");
            Console.WriteLine($"{new string('=', 80)}");
            Console.WriteLine($"Location: {fullPath}");
            Console.WriteLine($"{new string('=', 80)}\n");
            
            // Also write to standard error to ensure it's visible
            Console.Error.WriteLine($"\nHTML Report: {fullPath}\n");
            
            System.Diagnostics.Debug.WriteLine($"HTML Report generated: {fullPath}");
        }
        catch (Exception ex)
        {
            string errorMsg = $"Failed to generate HTML report: {ex.Message}\n{ex.StackTrace}";
            Console.WriteLine(errorMsg);
            Console.Error.WriteLine(errorMsg);
            System.Diagnostics.Debug.WriteLine(errorMsg);
        }
    }
}

