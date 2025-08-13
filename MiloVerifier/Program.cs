using MiloBench;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    // flag to indicate cancellation request
    private static volatile bool _cancellationRequested = false;

    static void Main(string[] args)
    {
        // set up handler for ctrl C
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

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

        Console.WriteLine($"Scanning for 'milo_*' files in '{folderPath}'...");
        var filesToProcess = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
    .Where(f => Path.GetExtension(f).StartsWith(".milo_"))
    .ToArray();

        if (filesToProcess.Length == 0)
        {
            Console.WriteLine("No Milo files found.");
            return;
        }

        var verifier = new MiloVerifier();
        var allResults = new List<MismatchResult>();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Found {filesToProcess.Length} files. Starting verification... (Press Ctrl+C to cancel gracefully)");

        for (int i = 0; i < filesToProcess.Length; i++)
        {
            // alow graceful cancellation
            if (_cancellationRequested)
            {
                Console.WriteLine("\n\nCancellation acknowledged. Stopping file processing.");
                break;
            }

            string file = filesToProcess[i];
            Console.Write($"  ({i + 1}/{filesToProcess.Length}) Processing {Path.GetFileName(file)}... ");
            try
            {
                var results = verifier.ProcessFile(file);
                if (results.Any())
                {
                    bool wasError = results.First().IsError;
                    if (wasError)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("ERROR.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Found {results.Count} mismatch(es).");
                    }
                    Console.ResetColor();
                    allResults.AddRange(results);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"UNHANDLED ERROR: {ex.Message}");
                Console.ResetColor();
                allResults.Add(new MismatchResult
                {
                    FilePath = file,
                    ErrorMessage = "An unhandled exception occurred in the main loop:\n" + ex.ToString()
                });
            }
        }

        stopwatch.Stop();

        if (_cancellationRequested)
        {
            Console.WriteLine($"\nProcessing was cancelled. Generating report for the {allResults.Count(r => !r.IsError)} mismatches and {allResults.Count(r => r.IsError)} errors found so far.");
        }
        else
        {
            Console.WriteLine($"\nVerification complete in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }

        // print the output report
        string reportPath = Path.Combine(AppContext.BaseDirectory, "verification_report.html");
        var reportGenerator = new ReportGenerator();
        reportGenerator.Generate(allResults, reportPath);

        Console.WriteLine($"A detailed HTML report has been saved to:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(reportPath);
        Console.ResetColor();
    }

    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("\n\nCtrl+C detected. Graceful shutdown initiated...");
        _cancellationRequested = true;
        e.Cancel = true;
    }
}