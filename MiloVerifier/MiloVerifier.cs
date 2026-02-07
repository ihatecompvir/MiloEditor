using MiloBench;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class MiloVerifier
{
    public List<MismatchResult> ProcessFile(string filePath)
    {
        var mismatches = new List<MismatchResult>();
        string tempFilePath = Path.GetTempFileName() + Path.GetExtension(filePath);

        try
        {
            // read original milo
            var originalMilo = new MiloFile(filePath);
            var originalHashes = new Dictionary<string, string>();
            var unsupported = new HashSet<(string name, string type)>();
            PopulateHashesRecursively(originalMilo.dirMeta, originalHashes, unsupported);

            // save to temp file, preserving all original file properties
            originalMilo.Save(tempFilePath, originalMilo.compressionType, null, Endian.LittleEndian, originalMilo.endian);

            // read back the newlty saved milo
            var savedMilo = new MiloFile(tempFilePath);
            var savedHashes = new Dictionary<string, string>();
            PopulateHashesRecursively(savedMilo.dirMeta, savedHashes, null);

            // compare the hashes of each file and dir and all that
            var allKeys = originalHashes.Keys.Union(savedHashes.Keys);

            foreach (var key in allKeys)
            {
                originalHashes.TryGetValue(key, out var beforeHash);
                savedHashes.TryGetValue(key, out var afterHash);

                if (beforeHash != afterHash)
                {
                    var parts = key.Split('|');
                    mismatches.Add(new MismatchResult
                    {
                        FilePath = filePath,
                        ObjectName = parts[0],
                        ObjectType = (parts.Length > 2) ? $"{parts[1]} {parts[2]}" : parts[1],
                        BeforeHash = beforeHash ?? "Object not found in original file",
                        AfterHash = afterHash ?? "Object not found in saved file"
                    });
                }
            }

            // add unsupported entries to the results
            foreach (var (name, type) in unsupported)
            {
                mismatches.Add(new MismatchResult
                {
                    FilePath = filePath,
                    ObjectName = name,
                    ObjectType = type,
                    IsUnsupported = true
                });
            }
        }
        catch (Exception ex)
        {
            // if anything explodes just capture the error
            mismatches.Add(new MismatchResult
            {
                FilePath = filePath,
                ErrorMessage = ex.ToString()
            });
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        return mismatches;
    }

    private void PopulateHashesRecursively(DirectoryMeta dir, Dictionary<string, string> hashes, HashSet<(string name, string type)> unsupported)
    {
        if (dir == null) return;

        string dirKey = $"{dir.name}|{dir.type}|(Directory Data)";
        hashes[dirKey] = CalculateDirectoryHash(dir);

        foreach (var entry in dir.entries)
        {
            string key = $"{entry.name}|{entry.type}";
            hashes[key] = CalculateEntryHash(entry);

            if (!entry.typeRecognized)
            {
                unsupported?.Add((entry.name.value, entry.type.value));
            }

            if (entry.dir != null)
            {
                PopulateHashesRecursively(entry.dir, hashes, unsupported);
            }
        }

        // traverse the dirs
        if (dir.directory is ObjectDir objDir)
        {
            foreach (var inlineDir in objDir.inlineSubDirs)
            {
                PopulateHashesRecursively(inlineDir, hashes, unsupported);
            }
        }
    }

    private string CalculateDirectoryHash(DirectoryMeta dir)
    {
        byte[] bytes = dir.DirObjBytes;
        if (bytes == null || bytes.Length == 0) return "empty";

        // collect byte ranges of inline subdir string table count/size fields
        // these are within the parent's DirObjBytes and may differ between read/write
        var exclusions = new List<(int offset, int length)>();
        CollectStringTableExclusions(dir, dir.dirDataStartAbsolutePosition, exclusions);

        if (exclusions.Count == 0)
            return CalculateSha256(bytes);

        byte[] copy = (byte[])bytes.Clone();
        foreach (var (offset, length) in exclusions)
        {
            if (offset >= 0 && offset + length <= copy.Length)
                Array.Clear(copy, offset, length);
        }
        return CalculateSha256(copy);
    }

    private string CalculateEntryHash(DirectoryMeta.Entry entry)
    {
        byte[] bytes = entry.objBytes;
        if (bytes == null || bytes.Length == 0) return "empty";

        if (entry.dir == null)
            return CalculateSha256(bytes);

        // entry has a sub-directory whose DirectoryMeta header (including string table) is within objBytes
        var exclusions = new List<(int offset, int length)>();
        long basePos = entry.objBytesAbsolutePosition;
        if (basePos >= 0)
            CollectStringTableExclusions(entry.dir, basePos, exclusions);

        if (exclusions.Count == 0)
            return CalculateSha256(bytes);

        byte[] copy = (byte[])bytes.Clone();
        foreach (var (offset, length) in exclusions)
        {
            if (offset >= 0 && offset + length <= copy.Length)
                Array.Clear(copy, offset, length);
        }
        return CalculateSha256(copy);
    }

    private void CollectStringTableExclusions(DirectoryMeta dir, long byteRangeStart, List<(int offset, int length)> exclusions)
    {
        // exclude this directory's own string table if it falls within the byte range
        if (dir.stringTableAbsolutePosition >= 0 && byteRangeStart >= 0)
        {
            int offset = (int)(dir.stringTableAbsolutePosition - byteRangeStart);
            exclusions.Add((offset, 8)); // stringTableCount (4) + stringTableSize (4)
        }

        // walk inline subdirectories
        if (dir.directory is ObjectDir objDir)
        {
            foreach (var inlineDir in objDir.inlineSubDirs)
            {
                CollectStringTableExclusions(inlineDir, byteRangeStart, exclusions);
            }
        }

        // walk entry subdirectories (their data is read during the parent's read, so it's within the byte range)
        foreach (var entry in dir.entries)
        {
            if (entry.dir != null)
            {
                CollectStringTableExclusions(entry.dir, byteRangeStart, exclusions);
            }
        }
    }

    private string CalculateSha256(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "empty";
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
