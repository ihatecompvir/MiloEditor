using MiloBench;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

public class ReportGenerator
{
    public void Generate(List<MismatchResult> results, string outputFilePath)
    {
        var sb = new StringBuilder();

        // surely there is a better way than this to create html files with like a template engine or something but idc, this works and is simple enough for this purpose
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>MiloLib Round-Trip Verification Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f7f6; color: #333; margin: 1em; line-height: 1.6; }");
        sb.AppendLine("h1, h2, h3 { color: #2c3e50; }");
        sb.AppendLine("h1 { font-size: 2em; border-bottom: 2px solid #e0e0e0; padding-bottom: 10px; }");
        sb.AppendLine("h2 { font-size: 1.5em; margin-top: 30px; background-color: #ecf0f1; padding: 10px; border-radius: 5px; }");
        sb.AppendLine("h3 { font-size: 1.2em; margin-top: 25px; border-bottom: 1px solid #ccc; padding-bottom: 5px; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 15px; box-shadow: 0 2px 3px rgba(0,0,0,0.1); }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }");
        sb.AppendLine("th { background-color: #3498db; color: white; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine("code { font-family: Consolas, 'Courier New', monospace; background-color: #e8e8e8; padding: 2px 5px; border-radius: 3px; font-size: 0.9em; word-break: break-all; }");
        sb.AppendLine(".mismatch { color: #c0392b; font-weight: bold; }");
        sb.AppendLine(".summary, .error-block { padding: 15px; border-radius: 5px; box-shadow: 0 2px 3px rgba(0,0,0,0.1); margin-bottom: 20px; }");
        sb.AppendLine(".summary-ok { background-color: #e8f5e9; border-left: 5px solid #27ae60; }");
        sb.AppendLine(".summary-fail { background-color: #fbe9e7; border-left: 5px solid #c0392b; }");
        sb.AppendLine(".error-block { background-color: #fbe9e7; border-left: 5px solid #e74c3c; }");
        sb.AppendLine("pre { white-space: pre-wrap; word-wrap: break-word; background-color: #fff; padding: 10px; border: 1px solid #ddd; border-radius: 4px; }");
        sb.AppendLine(".table-container { overflow-x: auto; }");
        sb.AppendLine("@media (max-width: 768px) { body { font-size: 14px; margin: 0.5em; } h1 { font-size: 1.5em;} h2 { font-size: 1.2em;} }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<h1>MiloLib Round-Trip Report</h1>");
        sb.AppendLine($"<div class='summary'>Generated on: {DateTime.Now:F}</div>");

        var errors = results.Where(r => r.IsError).ToList();
        var mismatches = results.Where(r => !r.IsError && !r.IsUnsupported).ToList();
        var unsupported = results.Where(r => r.IsUnsupported).ToList();
        var mismatchedFiles = mismatches.GroupBy(r => r.FilePath).ToList();

        if (errors.Any())
        {
            sb.AppendLine("<h2>Processing Errors</h2>");
            sb.AppendLine($"<div class='error-block'>{errors.Count} file(s) failed to process due to a processing error.</div>");
            foreach (var error in errors)
            {
                sb.AppendLine($"<h3>File: <code>{HttpUtility.HtmlEncode(error.FilePath)}</code></h3>");
                sb.AppendLine("<pre><code>" + HttpUtility.HtmlEncode(error.ErrorMessage) + "</code></pre>");
            }
        }

        sb.AppendLine("<h2>Hash Mismatches</h2>");
        if (!mismatches.Any())
        {
            sb.AppendLine("<div class='summary summary-ok'><strong>Excellent!</strong> No hash mismatches were found, indicating a successful round trip.</div>");
        }
        else
        {
            sb.AppendLine($"<div class='summary summary-fail'>Found {mismatches.Count} object mismatch(es) across {mismatchedFiles.Count} file(s).</div>");
            foreach (var group in mismatchedFiles)
            {
                sb.AppendLine($"<h3>File: <code>{HttpUtility.HtmlEncode(group.Key)}</code></h3>");
                sb.AppendLine("<div class='table-container'>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>Object Name</th><th>Type</th><th>Before Hash</th><th>After Hash</th></tr>");

                foreach (var result in group)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td><code>{HttpUtility.HtmlEncode(result.ObjectName)}</code></td>");
                    sb.AppendLine($"<td><code>{HttpUtility.HtmlEncode(result.ObjectType)}</code></td>");
                    sb.AppendLine($"<td><code>{result.BeforeHash}</code></td>");
                    sb.AppendLine($"<td class='mismatch'><code>{result.AfterHash}</code></td>");
                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
            }
        }

        sb.AppendLine("<h2>Unsupported Types</h2>");
        if (!unsupported.Any())
        {
            sb.AppendLine("<div class='summary summary-ok'>All object and directory types in the scanned files are supported.</div>");
        }
        else
        {
            // group by type to show a summary of unique unsupported types with counts
            var byType = unsupported.GroupBy(r => r.ObjectType).OrderByDescending(g => g.Count()).ToList();
            sb.AppendLine($"<div class='summary summary-fail'>Found {unsupported.Count} unsupported object(s) across {byType.Count} type(s).</div>");

            sb.AppendLine("<h3>By Type</h3>");
            sb.AppendLine("<div class='table-container'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Type</th><th>Count</th></tr>");
            foreach (var group in byType)
            {
                sb.AppendLine($"<tr><td><code>{HttpUtility.HtmlEncode(group.Key)}</code></td><td>{group.Count()}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // detailed list grouped by file
            var byFile = unsupported.GroupBy(r => r.FilePath).ToList();
            sb.AppendLine("<h3>By File</h3>");
            foreach (var group in byFile)
            {
                sb.AppendLine($"<h4>File: <code>{HttpUtility.HtmlEncode(group.Key)}</code></h4>");
                sb.AppendLine("<div class='table-container'>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>Object Name</th><th>Type</th></tr>");
                foreach (var result in group)
                {
                    sb.AppendLine($"<tr><td><code>{HttpUtility.HtmlEncode(result.ObjectName)}</code></td><td><code>{HttpUtility.HtmlEncode(result.ObjectType)}</code></td></tr>");
                }
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
            }
        }

        sb.AppendLine("</body></html>");

        File.WriteAllText(outputFilePath, sb.ToString());
    }
}