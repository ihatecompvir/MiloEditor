using MiloBench;
using MiloLib;
using MiloLib.Assets;
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
            PopulateHashesRecursively(originalMilo.dirMeta, originalHashes);

            // save to temp file
            originalMilo.Save(tempFilePath, originalMilo.compressionType, bodyEndian: originalMilo.endian);

            // read back the newlty saved milo
            var savedMilo = new MiloFile(tempFilePath);
            var savedHashes = new Dictionary<string, string>();
            PopulateHashesRecursively(savedMilo.dirMeta, savedHashes);

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

    private void PopulateHashesRecursively(DirectoryMeta dir, Dictionary<string, string> hashes)
    {
        if (dir == null) return;

        string dirKey = $"{dir.name}|{dir.type}|(Directory Data)";
        hashes[dirKey] = CalculateSha256(dir.DirObjBytes);

        foreach (var entry in dir.entries)
        {
            string key = $"{entry.name}|{entry.type}";
            hashes[key] = CalculateSha256(entry.objBytes);

            if (entry.dir != null)
            {
                PopulateHashesRecursively(entry.dir, hashes);
            }
        }

        // traverse the dirs
        if (dir.directory is ObjectDir objDir)
        {
            foreach (var inlineDir in objDir.inlineSubDirs)
            {
                PopulateHashesRecursively(inlineDir, hashes);
            }
        }
    }

    private string CalculateSha256(List<byte> bytes)
    {
        if (bytes == null || bytes.Count == 0) return "empty";
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(bytes.ToArray());
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
