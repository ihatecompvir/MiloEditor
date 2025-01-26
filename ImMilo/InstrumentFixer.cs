using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using TinyDialogsNet;

namespace ImMilo;

public static class InstrumentFixer
{
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
        
        file.Save(newPath, compression, 2064U, Endian.LittleEndian, file.endian);
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