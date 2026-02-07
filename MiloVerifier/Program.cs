using MiloBench;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    private static readonly object _consoleLock = new();

    static void Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            lock (_consoleLock)
            {
                Console.WriteLine("\n\nCtrl+C detected. Graceful shutdown initiated...");
            }
        };

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MiloVerifier.exe <path_to_folder>");
            Console.WriteLine("This will attempt to open and then re-save every milo file it discovers in a folder and output an HTML report listing which Objects did not save properly.");
            Console.WriteLine("If you are implementing new Objects, this is a good way to check, at scale, that your reading/writing logic is sound.");
            return;
        }

        // check if there is already a report, and ask the user if they want to overwrite it
        if (File.Exists("verification_report.html"))
        {
            Console.WriteLine("A previous report already exists. Do you want to overwrite it? (y/n)");
            var response = Console.ReadKey(true);
            if (response.Key != ConsoleKey.Y)
            {
                Console.WriteLine("Exiting without overwriting the report.");
                return;
            }
        }

        string folderPath = args[0];
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"Error: Directory not found at '{folderPath}'");
            return;
        }

        Console.WriteLine($"Scanning for 'milo_*', 'rnd_*', and 'gh' files in '{folderPath}'...");
        var filesToProcess = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var ext = Path.GetExtension(f);
                return ext.StartsWith(".milo_") || ext.StartsWith(".rnd_") || ext == ".milo" || ext == ".rnd" || ext == ".gh";
            })
            .ToArray();

        if (filesToProcess.Length == 0)
        {
            Console.WriteLine("No Milo files found.");
            return;
        }

        var allResults = new ConcurrentBag<MismatchResult>();
        int processedCount = 0;
        int okCount = 0;
        int mismatchCount = 0;
        int errorCount = 0;
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Found {filesToProcess.Length} files. Starting verification with {Environment.ProcessorCount} threads... (Press Ctrl+C to cancel gracefully)");

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cts.Token
        };

        try
        {
            Parallel.ForEach(filesToProcess, parallelOptions, file =>
            {
                var verifier = new MiloVerifier();
                List<MismatchResult> results;

                try
                {
                    results = verifier.ProcessFile(file);
                }
                catch (Exception ex)
                {
                    results = new List<MismatchResult>
                    {
                        new MismatchResult
                        {
                            FilePath = file,
                            ErrorMessage = "An unhandled exception occurred:\n" + ex.ToString()
                        }
                    };
                }

                foreach (var r in results)
                    allResults.Add(r);

                int current = Interlocked.Increment(ref processedCount);

                lock (_consoleLock)
                {
                    var errors = results.Where(r => r.IsError).ToList();
                    var actualMismatches = results.Where(r => !r.IsError && !r.IsUnsupported).ToList();
                    var unsupported = results.Where(r => r.IsUnsupported).ToList();

                    Console.Write($"  ({current}/{filesToProcess.Length}) {Path.GetFileName(file)}... ");
                    if (errors.Any())
                    {
                        Interlocked.Increment(ref errorCount);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("ERROR.");
                    }
                    else if (actualMismatches.Any())
                    {
                        Interlocked.Add(ref mismatchCount, actualMismatches.Count);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"Found {actualMismatches.Count} mismatch(es).");
                        if (unsupported.Any())
                            Console.Write($" ({unsupported.Count} unsupported)");
                        Console.WriteLine();
                    }
                    else if (unsupported.Any())
                    {
                        Interlocked.Increment(ref okCount);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"OK. ({unsupported.Count} unsupported)");
                    }
                    else
                    {
                        Interlocked.Increment(ref okCount);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("OK.");
                    }
                    Console.ResetColor();
                }
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n\nCancellation acknowledged. Stopping file processing.");
        }

        stopwatch.Stop();

        if (cts.IsCancellationRequested)
        {
            Console.WriteLine($"\nProcessing was cancelled after {processedCount}/{filesToProcess.Length} files. {okCount} OK, {mismatchCount} mismatches, {errorCount} errors.");
        }
        else
        {
            Console.WriteLine($"\nVerification complete in {stopwatch.Elapsed.TotalSeconds:F2} seconds. {okCount} OK, {mismatchCount} mismatches, {errorCount} errors.");
        }

        // print the output report
        var resultList = allResults.ToList();
        string reportPath = Path.Combine(AppContext.BaseDirectory, "verification_report.html");
        var reportGenerator = new ReportGenerator();
        reportGenerator.Generate(resultList, reportPath);

        Console.WriteLine($"A detailed HTML report has been saved to:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(reportPath);
        Console.ResetColor();
    }
}