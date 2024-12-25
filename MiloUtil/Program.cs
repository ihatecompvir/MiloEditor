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
            case "extract":
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: MiloUtil extract <filePath> <asset> <outputPath> [--directory <directory>]");
                    return;
                }
                string extractDirectory = GetOption(args, "--directory");
                ExtractCommand(args[1], args[2], args[3], extractDirectory);
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                ShowHelp();
                break;
        }
    }

    static void InfoCommand(string filePath)
    {
        Console.WriteLine($"Fetching information for Milo file at: {filePath}");

        MiloFile file = new MiloFile(filePath);

        Console.WriteLine("Milo scene information:");
        Console.WriteLine($"    Name / Type: {file.dirMeta.name} ({file.dirMeta.type})");
        Console.WriteLine($"    Number of entries: {file.dirMeta.entries.Count}");
        foreach (var entry in file.dirMeta.entries)
        {
            Console.WriteLine($"        {entry.name} ({entry.type})");

            if (entry.dir != null)
            {
                Console.WriteLine($"            Directory: {entry.dir.name}");

                foreach (var subEntry in entry.dir.entries)
                {
                    Console.WriteLine($"                {subEntry.name} ({subEntry.type})");
                }
            }

        }

        ObjectDir dir = (ObjectDir)file.dirMeta.dirObj;


        Console.WriteLine($"    Inlined Sub Directories: {dir.inlineSubDirs.Count}");
        foreach (var subDir in dir.inlineSubDirs)
        {
            Console.WriteLine($"        {subDir.name}");

            foreach (var entry in subDir.entries)
            {
                Console.WriteLine($"            {entry.name} ({entry.type})");
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

    static void ExtractCommand(string filePath, string asset, string outputPath, string directory)
    {
        Console.WriteLine($"Extracting asset '{asset}' from Milo file at: {filePath} to: {outputPath}");

        MiloFile miloFile = new MiloFile(filePath);

        // look through the entries to find the asset
        foreach (var entry in miloFile.dirMeta.entries)
        {
            if (entry.name == asset)
            {
                // found the asset
                Console.WriteLine($"Found asset '{asset}' of type '{entry.type}'");

                // extract the asset
                System.IO.File.WriteAllBytes(outputPath, entry.objBytes.ToArray());

                Console.WriteLine($"Extracted asset to: {outputPath}");
                return;
            }
        }

        // if we didn't find it in the entries, look through all inlined subdirectories
        foreach (var dir in ((ObjectDir)miloFile.dirMeta.dirObj).inlineSubDirs)
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

        // TODO: look through all entries that are directories
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
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --directory <directory>        Specify the directory inside the Milo scene (default: the root directory)");
    }
}