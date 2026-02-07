using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MiloLib.Tests;

/// <summary>
/// Thread-safe collector for test results that tracks pass/fail status per asset type and revision.
/// </summary>
public static class TestResultCollector
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, TestResult>> _results = new();
    private static readonly object _lockObject = new object();
    private static bool _reportGenerated = false;

    public enum TestStatus
    {
        Passed,
        Failed,
        Skipped // For revisions that aren't supported
    }

    public class TestResult
    {
        public string AssetType { get; set; } = string.Empty;
        public ushort Revision { get; set; }
        public TestStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Records a test result for a specific asset type and revision.
    /// </summary>
    public static void RecordResult(string assetType, ushort revision, TestStatus status, string? errorMessage = null)
    {
        var assetResults = _results.GetOrAdd(assetType, _ => new ConcurrentDictionary<ushort, TestResult>());
        
        assetResults.AddOrUpdate(revision, 
            new TestResult
            {
                AssetType = assetType,
                Revision = revision,
                Status = status,
                ErrorMessage = errorMessage
            },
            (key, existing) =>
            {
                existing.Status = status;
                existing.ErrorMessage = errorMessage;
                existing.Timestamp = DateTime.Now;
                return existing;
            });
    }

    /// <summary>
    /// Gets all results for a specific asset type.
    /// </summary>
    public static Dictionary<ushort, TestResult> GetResultsForAsset(string assetType)
    {
        if (_results.TryGetValue(assetType, out var assetResults))
        {
            return assetResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        return new Dictionary<ushort, TestResult>();
    }

    /// <summary>
    /// Gets all collected results.
    /// </summary>
    public static Dictionary<string, Dictionary<ushort, TestResult>> GetAllResults()
    {
        return _results.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)
        );
    }

    /// <summary>
    /// Clears all collected results.
    /// </summary>
    public static void Clear()
    {
        _results.Clear();
        lock (_lockObject)
        {
            _reportGenerated = false;
        }
    }

    /// <summary>
    /// Marks that a report has been generated to prevent duplicate generation.
    /// </summary>
    public static bool TryMarkReportGenerated()
    {
        lock (_lockObject)
        {
            if (_reportGenerated)
                return false;
            _reportGenerated = true;
            return true;
        }
    }
}

