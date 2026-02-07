using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiloLib.Tests;

/// <summary>
/// Generates an HTML report showing test results for each asset type and revision.
/// </summary>
public static class HtmlReportGenerator
{
    public static string GenerateReport(string outputPath)
    {
        var allResults = TestResultCollector.GetAllResults();
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>MiloLib Round-Trip Serialization Test Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f5f5f5; }");
        html.AppendLine("        h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }");
        html.AppendLine("        h2 { color: #555; margin-top: 30px; }");
        html.AppendLine("        .summary { background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 20px; }");
        html.AppendLine("        .summary-stats { display: flex; gap: 20px; flex-wrap: wrap; }");
        html.AppendLine("        .stat-box { background-color: #f8f9fa; padding: 15px; border-radius: 5px; min-width: 150px; }");
        html.AppendLine("        .stat-box h3 { margin: 0 0 10px 0; color: #666; font-size: 14px; }");
        html.AppendLine("        .stat-box .value { font-size: 24px; font-weight: bold; }");
        html.AppendLine("        .stat-box.passed .value { color: #4CAF50; }");
        html.AppendLine("        .stat-box.failed .value { color: #f44336; }");
        html.AppendLine("        .stat-box.skipped .value { color: #ff9800; }");
        html.AppendLine("        table { width: 100%; border-collapse: collapse; background-color: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 30px; }");
        html.AppendLine("        th { background-color: #4CAF50; color: white; padding: 12px; text-align: left; font-weight: 600; }");
        html.AppendLine("        td { padding: 10px; border-bottom: 1px solid #ddd; }");
        html.AppendLine("        tr:hover { background-color: #f5f5f5; }");
        html.AppendLine("        .status-passed { color: #4CAF50; font-weight: bold; }");
        html.AppendLine("        .status-failed { color: #f44336; font-weight: bold; }");
        html.AppendLine("        .status-skipped { color: #ff9800; font-weight: bold; }");
        html.AppendLine("        .error-message { font-size: 11px; color: #666; font-style: italic; max-width: 400px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }");
        html.AppendLine("        .error-message:hover { white-space: normal; overflow: visible; }");
        html.AppendLine("        .revision-cell { text-align: center; font-weight: 500; }");
        html.AppendLine("        .asset-name { font-weight: 600; color: #333; }");
        html.AppendLine("        .filter-controls { background-color: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 20px; }");
        html.AppendLine("        .filter-controls label { margin-right: 15px; }");
        html.AppendLine("        .filter-controls input[type=\"checkbox\"] { margin-right: 5px; }");
        html.AppendLine("        .no-results { text-align: center; padding: 40px; color: #999; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        html.AppendLine("    <h1>MiloLib Round-Trip Serialization Test Report</h1>");
        html.AppendLine($"    <p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        
        // Calculate summary statistics
        int totalTests = 0;
        int passedTests = 0;
        int failedTests = 0;
        int skippedTests = 0;
        int totalAssets = allResults.Count;
        
        foreach (var assetResults in allResults.Values)
        {
            foreach (var result in assetResults.Values)
            {
                totalTests++;
                switch (result.Status)
                {
                    case TestResultCollector.TestStatus.Passed:
                        passedTests++;
                        break;
                    case TestResultCollector.TestStatus.Failed:
                        failedTests++;
                        break;
                    case TestResultCollector.TestStatus.Skipped:
                        skippedTests++;
                        break;
                }
            }
        }
        
        // Summary section
        html.AppendLine("    <div class=\"summary\">");
        html.AppendLine("        <h2>Summary</h2>");
        html.AppendLine("        <div class=\"summary-stats\">");
        html.AppendLine($"            <div class=\"stat-box\"><h3>Total Asset Types</h3><div class=\"value\">{totalAssets}</div></div>");
        html.AppendLine($"            <div class=\"stat-box\"><h3>Total Tests</h3><div class=\"value\">{totalTests}</div></div>");
        html.AppendLine($"            <div class=\"stat-box passed\"><h3>Passed</h3><div class=\"value\">{passedTests}</div></div>");
        html.AppendLine($"            <div class=\"stat-box failed\"><h3>Failed</h3><div class=\"value\">{failedTests}</div></div>");
        html.AppendLine($"            <div class=\"stat-box skipped\"><h3>Skipped</h3><div class=\"value\">{skippedTests}</div></div>");
        if (totalTests > 0)
        {
            double passRate = (double)passedTests / totalTests * 100;
            html.AppendLine($"            <div class=\"stat-box\"><h3>Pass Rate</h3><div class=\"value\">{passRate:F1}%</div></div>");
        }
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        
        // Filter controls
        html.AppendLine("    <div class=\"filter-controls\">");
        html.AppendLine("        <label><input type=\"checkbox\" id=\"show-passed\" checked onchange=\"filterTable()\"> Show Passed</label>");
        html.AppendLine("        <label><input type=\"checkbox\" id=\"show-failed\" checked onchange=\"filterTable()\"> Show Failed</label>");
        html.AppendLine("        <label><input type=\"checkbox\" id=\"show-skipped\" checked onchange=\"filterTable()\"> Show Skipped</label>");
        html.AppendLine("    </div>");
        
        // Detailed results table
        html.AppendLine("    <h2>Detailed Results</h2>");
        html.AppendLine("    <table id=\"results-table\">");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>Asset Type</th>");
        html.AppendLine("                <th>Revision</th>");
        html.AppendLine("                <th>Status</th>");
        html.AppendLine("                <th>Error Message</th>");
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");
        
        // Sort assets by name for consistency
        var sortedAssets = allResults.OrderBy(kvp => kvp.Key).ToList();
        
        foreach (var (assetName, revisions) in sortedAssets)
        {
            var sortedRevisions = revisions.OrderBy(kvp => kvp.Key).ToList();
            
            foreach (var (revision, result) in sortedRevisions)
            {
                string statusClass = result.Status switch
                {
                    TestResultCollector.TestStatus.Passed => "status-passed",
                    TestResultCollector.TestStatus.Failed => "status-failed",
                    TestResultCollector.TestStatus.Skipped => "status-skipped",
                    _ => ""
                };
                
                string statusText = result.Status switch
                {
                    TestResultCollector.TestStatus.Passed => "✓ Passed",
                    TestResultCollector.TestStatus.Failed => "✗ Failed",
                    TestResultCollector.TestStatus.Skipped => "⊘ Skipped",
                    _ => "Unknown"
                };
                
                string dataStatus = result.Status.ToString().ToLower();
                
                html.AppendLine($"            <tr class=\"result-row\" data-status=\"{dataStatus}\">");
                html.AppendLine($"                <td class=\"asset-name\">{assetName}</td>");
                html.AppendLine($"                <td class=\"revision-cell\">{revision}</td>");
                html.AppendLine($"                <td class=\"{statusClass}\">{statusText}</td>");
                html.AppendLine($"                <td class=\"error-message\">{EscapeHtml(result.ErrorMessage ?? "")}</td>");
                html.AppendLine("            </tr>");
            }
        }
        
        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");
        
        // Add JavaScript for filtering
        html.AppendLine("    <script>");
        html.AppendLine("        function filterTable() {");
        html.AppendLine("            const showPassed = document.getElementById('show-passed').checked;");
        html.AppendLine("            const showFailed = document.getElementById('show-failed').checked;");
        html.AppendLine("            const showSkipped = document.getElementById('show-skipped').checked;");
        html.AppendLine("            const rows = document.querySelectorAll('.result-row');");
        html.AppendLine("            rows.forEach(row => {");
        html.AppendLine("                const status = row.getAttribute('data-status');");
        html.AppendLine("                if ((status === 'passed' && !showPassed) ||");
        html.AppendLine("                    (status === 'failed' && !showFailed) ||");
        html.AppendLine("                    (status === 'skipped' && !showSkipped)) {");
        html.AppendLine("                    row.style.display = 'none';");
        html.AppendLine("                } else {");
        html.AppendLine("                    row.style.display = '';");
        html.AppendLine("                }");
        html.AppendLine("            });");
        html.AppendLine("        }");
        html.AppendLine("    </script>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        string reportContent = html.ToString();
        
        // Write to file
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        File.WriteAllText(outputPath, reportContent);
        
        return outputPath;
    }
    
    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

