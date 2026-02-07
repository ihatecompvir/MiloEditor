using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Char;
using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using TinyDialogsNet;

namespace ImMilo;

public static class CharAssetFixer
{
    
    private static List<byte> templateBytes = new();

    private static RndGroup AddTemplateTranslucentGroup(DirectoryMeta dir)
    {
        var assembly = typeof(CharAssetFixer).Assembly;


        if (templateBytes.Count == 0)
        {
            using (Stream s = assembly.GetManifestResourceStream("translucentGroupTemplate"))
            {
                byte[] bytes = new byte[s.Length];
            
                s.ReadExactly(bytes, 0, bytes.Length);
                templateBytes = bytes.ToList();
            }
        }
        
        var entry = DirectoryMeta.Entry.CreateDirtyAssetFromBytes("Group", "translucent.grp", templateBytes);
        entry.dirty = false; // this is probably stupid, whatever!
        dir.entries.Add(entry);

        var groupObj = new RndGroup().Read(new EndianReader(new MemoryStream(templateBytes.ToArray()), Endian.BigEndian), false, dir, entry);
        entry.obj = groupObj;
        return groupObj;
    }

    public static void FixCharAsset(MiloFile file, string newPath)
    {
        var objDir = (ObjectDir)file.dirMeta.directory;
        var dir = file.dirMeta;
        if (objDir.inlineSubDirs.Count > 0)
        {
            dir = objDir.inlineSubDirs[0];
        }
        if (dir.directory is Character character)
        {
            RndGroup? transGroup = null;
            if (character.translucentGroup.value.Length == 0)
            {
                transGroup = AddTemplateTranslucentGroup(dir);
                character.translucentGroup = "translucent.grp";
            }
            else
            {
                foreach (var entry in dir.entries)
                {
                    if (entry.name.value == character.translucentGroup.value)
                    {
                        transGroup = (RndGroup)entry.obj;
                    }
                }
            }

            if (transGroup == null)
            {
                throw new Exception($"Couldn't fix {file.filePath}! translucentGroup was defined but non-existent");
            }

            foreach (var entry in dir.entries)
            {
                if (entry.obj is RndMesh)
                {
                    var exists = transGroup.objects.Any(existMesh => existMesh.value == entry.name.value);

                    if (!exists)
                    {
                        Console.WriteLine($"Adding {entry.name.value} to translucent group");
                        transGroup.objects.Add(entry.name);
                    }
                }
            }
        }
        
        var compression = file.compressionType;
        if (file.dirMeta.platform == DirectoryMeta.Platform.PS3)
        {
            compression = MiloFile.Type.Uncompressed;
        }
        
        file.Save(newPath, compression, null, Endian.LittleEndian, file.endian);
    }

    public static void FixCharAssetFolder(string path)
    {
        var files = Directory.GetFiles(path);
        var fixedDir = Path.Join(path, "fixed");
        Directory.CreateDirectory(fixedDir);
        foreach (var filePath in files)
        {
            Console.WriteLine($"Fixing {filePath}");
            var filename = Path.GetFileName(filePath);
            var newPath = Path.Join(fixedDir, filename);
            FixCharAsset(new MiloFile(filePath), newPath);
        }
    }
    
    public static void PromptCharAssetFix()
    {
        var (canceled, path) = TinyDialogs.SelectFolderDialog("Select character asset folder");

        if (!canceled)
        {
            try
            {
                FixCharAssetFolder(path);
            }
            catch (Exception e)
            {
                Program.OpenErrorModal(e, "Failed to fix char asset folder");
            }
            
        }
    }
    
    public static void FixInstrument(MiloFile file, string newPath)
    {
        var objDir = (ObjectDir)file.dirMeta.directory;
        
        var uniq0 = file.dirMeta;
        if (objDir.inlineSubDirs.Count > 0)
        {
            uniq0 = objDir.inlineSubDirs[0];
        }

        RndGroup? translucentGrp = null;
        
        List<Symbol> meshes = new();
        
        foreach (var entry in uniq0.entries)
        {
            if (entry.name.value == "translucent.grp")
            {
                if (entry.obj is RndGroup grp)
                {
                    translucentGrp = grp;
                }
            }

            if (entry.obj is RndMesh mesh)
            {
                if (!entry.name.value.StartsWith("instrument_placement"))
                {
                    meshes.Add(entry.name);
                }
            }
        }

        if (translucentGrp != null)
        {
            foreach (var newMesh in meshes)
            {
                var exists = false;
                foreach (var existMesh in translucentGrp.objects)
                {
                    if (existMesh.value == newMesh.value)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists) continue;
                translucentGrp.objects.Add(newMesh);
            }

            for (int i = 0; i < translucentGrp.objects.Count; i++)
            {
                var obj = translucentGrp.objects[i];
                if (obj.value.Contains("_string"))
                {
                    translucentGrp.objects.Remove(obj);
                    translucentGrp.objects.Add(obj); // Reorders the strings to be last
                    break;
                }
            }
        }

        var compression = file.compressionType;
        if (file.dirMeta.platform == DirectoryMeta.Platform.PS3)
        {
            compression = MiloFile.Type.Uncompressed;
        }
        
        file.Save(newPath, compression, null, Endian.LittleEndian, file.endian);
    }

    public static void FixInstrumentFolder(string path)
    {
        var files = Directory.GetFiles(path);
        var fixedDir = Path.Join(path, "fixed");
        Directory.CreateDirectory(fixedDir);
        foreach (var filePath in files)
        {
            Console.WriteLine($"Fixing {filePath}");
            var filename = Path.GetFileName(filePath);
            var newPath = Path.Join(fixedDir, filename);
            FixInstrument(new MiloFile(filePath), newPath);
        }
    }

    public static void PromptInstrumentFix()
    {
        var (canceled, path) = TinyDialogs.SelectFolderDialog("Select instrument folder");

        if (!canceled)
        {
            try
            {
                FixInstrumentFolder(path);
            }
            catch (Exception e)
            {
                Program.OpenErrorModal(e, "Failed to fix instrument folder");
            }
            
        }
    }
}