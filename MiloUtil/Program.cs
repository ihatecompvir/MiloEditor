using System;
using MiloLib;
using MiloLib.Assets;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "info":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: MiloUtil info <filePath>");
                    return;
                }
                InfoCommand(args[1]);
                break;

            case "add":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: MiloUtil add <filePath> <asset> [--directory <directory>]");
                    return;
                }
                string directory = GetOption(args, "--directory") ?? "root";
                AddCommand(args[1], args[2], directory);
                break;

            case "rename":
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: MiloUtil rename <filePath> <oldName> <newName> [--directory <directory>]");
                    return;
                }
                string renameDirectory = GetOption(args, "--directory");
                RenameCommand(args[1], args[2], args[3], renameDirectory);
                break;

            case "uncompress":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: MiloUtil uncompress <filePath>");
                    return;
                }
                UncompressCommand(args[1]);
                break;

            case "extract":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: MiloUtil extract <filePath> <asset> [<outputPath>] [--directory <directory>]");
                    return;
                }

                string extractFilePath = args[1];
                string assetName = args[2];
                string outputPath = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : null;
                string extractDirectory = GetOption(args, "--directory");

                ExtractCommand(extractFilePath, assetName, outputPath, extractDirectory);
                break;

            default:
                Console.WriteLine($"Unknown command: {command}");
                ShowHelp();
                break;
        }
    }

    /// <summary>
    /// Recursively prints information about directory entries.
    /// </summary>
    /// <param name="entry">The entry to process.</param>
    /// <param name="indentLevel">The level of indentation for formatting.</param>
    static void PrintEntry(DirectoryMeta.Entry entry, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        Console.WriteLine($"{indent}{entry.name} ({entry.type})");

        if (entry.dir != null)
        {
            Console.WriteLine($"{indent}Directory: {entry.dir.name}");
            foreach (var subEntry in entry.dir.entries)
            {
                PrintEntry(subEntry, indentLevel + 1);
            }

            ObjectDir subDir = entry.dir.directory as ObjectDir;
            if (subDir != null)
            {
                foreach (var inlineSubDir in subDir.inlineSubDirs)
                {
                    Console.WriteLine($"{indent}    Inline Subdirectory: {inlineSubDir.name}");
                    foreach (var inlineEntry in inlineSubDir.entries)
                    {
                        PrintEntry(inlineEntry, indentLevel + 2);
                    }
                }
            }
        }
    }

    static void InfoCommand(string filePath)
    {
        Console.WriteLine($"Fetching information for Milo file at: {filePath}");

        MiloFile file = new MiloFile(filePath);

        Console.WriteLine("Milo scene information:");
        if (file.dirMeta.type == "")
        {
            Console.WriteLine("    Root directory does not exist as this is a GH1-style scene");
        }
        else
        {
            Console.WriteLine($"    Name / Type: {file.dirMeta.name} ({file.dirMeta.type})");
        }
        Console.WriteLine($"    Number of entries: {file.dirMeta.entries.Count}");

        // Traverse the root directory
        foreach (var entry in file.dirMeta.entries)
        {
            PrintEntry(entry, 1);
        }

        if (file.dirMeta.type != "")
        {
            ObjectDir dir = (ObjectDir)file.dirMeta.directory;

            Console.WriteLine($"    Inlined Sub Directories: {dir.inlineSubDirs.Count}");
            foreach (var subDir in dir.inlineSubDirs)
            {
                Console.WriteLine($"        Subdirectory: {subDir.name}");
                foreach (var entry in subDir.entries)
                {
                    PrintEntry(entry, 2);
                }
            }
        }
    }

    static void AddCommand(string filePath, string asset, string directory)
    {
        Console.WriteLine($"Adding asset '{asset}' to Milo file at: {filePath} in directory: {directory}");
        // Logic for adding an asset
    }

    static void RenameCommand(string filePath, string oldName, string newName, string directory)
    {
        Console.WriteLine($"Renaming asset from '{oldName}' to '{newName}' in Milo file at: {filePath}");
        if (!string.IsNullOrEmpty(directory))
        {
            Console.WriteLine($"Located in directory: {directory}");
        }
        // Logic for renaming an asset
    }

    static void UncompressCommand(string filePath)
    {
        MiloFile compressedFile = new MiloFile(filePath);

        // try to save, catch any errors
        try
        {
            compressedFile.Save(filePath, MiloFile.Type.Uncompressed);
            Console.WriteLine("Milo at " + filePath + " is now uncompressed!");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error saving uncompressed Milo file: " + e.Message);
        }
    }

    static void ExtractCommand(string filePath, string asset, string outputPath = null, string directory = null)
    {

        // Default the outputPath to the asset name if not specified
        outputPath ??= $"{asset}";

        Console.WriteLine($"Output path: {outputPath}");

        MiloFile miloFile = new MiloFile(filePath);

        // look through the entries to find the asset
        foreach (var entry in miloFile.dirMeta.entries)
        {
            if (entry.name == asset && (string.IsNullOrEmpty(directory) || miloFile.dirMeta.name == directory))
            {
                // found the asset
                Console.WriteLine($"Found asset '{asset}' of type '{entry.type}' in directory '{miloFile.dirMeta.name}'");

                // extract the asset
                System.IO.File.WriteAllBytes(outputPath, entry.objBytes.ToArray());

                Console.WriteLine($"Extracted asset to: {outputPath}");
                return;
            }
        }

        // if we didn't find it in the entries, look through all inlined subdirectories
        foreach (var dir in ((ObjectDir)miloFile.dirMeta.directory).inlineSubDirs)
        {
            if (string.IsNullOrEmpty(directory) || dir.name == directory)
            {
                foreach (var entry in dir.entries)
                {
                    if (entry.name == asset)
                    {
                        // found the asset
                        Console.WriteLine($"Found asset '{asset}' of type '{entry.type}' in inlined subdirectory '{dir.name}'");

                        // extract the asset
                        System.IO.File.WriteAllBytes(outputPath, entry.objBytes.ToArray());

                        Console.WriteLine($"Extracted asset to: {outputPath}");
                        return;
                    }
                }
            }
        }

        // look through all entries as directories
        foreach (var entry in miloFile.dirMeta.entries)
        {
            if (entry.dir != null)
            {
                if (string.IsNullOrEmpty(directory) || entry.name == directory)
                {
                    foreach (var subEntry in entry.dir.entries)
                    {
                        if (subEntry.name == asset)
                        {
                            // found the asset
                            Console.WriteLine($"Found asset '{asset}' of type '{subEntry.type}' in directory '{entry.name}'");

                            // extract the asset
                            System.IO.File.WriteAllBytes(outputPath, subEntry.objBytes.ToArray());

                            Console.WriteLine($"Extracted asset to: {outputPath}");
                            return;
                        }
                    }

                    // look through inlined subdirs of this directory
                    foreach (var dir in ((ObjectDir)entry.dir.directory).inlineSubDirs)
                    {
                        if (string.IsNullOrEmpty(directory) || dir.name == directory)
                        {
                            foreach (var subEntry in dir.entries)
                            {
                                if (subEntry.name == asset)
                                {
                                    // found the asset
                                    Console.WriteLine($"Found asset '{asset}' of type '{subEntry.type}' in inlined subdirectory '{dir.name}' of directory '{entry.name}'");

                                    // extract the asset
                                    System.IO.File.WriteAllBytes(outputPath, subEntry.objBytes.ToArray());

                                    Console.WriteLine($"Extracted asset to: {outputPath}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine($"Asset '{asset}' not found in the Milo file.");
    }



    static string GetOption(string[] args, string optionName)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == optionName && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    static void ShowHelp()
    {
        Console.WriteLine("MiloUtil - A command-line utility for managing Milo scenes and their assets");
        Console.WriteLine("2024 ihatecompvir and contributors");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  info <filePath>                Lists information about the Milo scene and its assets");
        Console.WriteLine("  add <filePath> <asset> [--directory <directory>]  Adds a new asset into a Milo scene");
        Console.WriteLine("  rename <filePath> <oldName> <newName> [--directory <directory>]  Renames an asset in a Milo scene");
        Console.WriteLine("  extract <filePath> <asset> <outputPath> [--directory <directory>]  Extracts an asset from a Milo scene");
        Console.WriteLine("  uncompress <filePath>          Opens a Milo scene and resaves it as uncompressed.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --directory <directory>        Specify the directory inside the Milo scene (default: the root directory)");
    }
}