namespace MiloLib.Tests;
using System.Reflection;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using MiloLib.Assets.UI;
using MiloLib.Utils;

public class IOTests
{
    [Fact]
    public void TestOpening()
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testFilePath = Path.Combine(projectDir, "TestData", "test.milo_ps3");

        // test creating a MiloFile from that path
        MiloFile milo = new MiloFile(testFilePath);

        // print the directory name and all entry names
        Console.WriteLine("Directory name: " + milo.dirMeta.name + " of type " + milo.dirMeta.type);

        foreach (DirectoryMeta.Entry entry in milo.dirMeta.entries)
        {
            Console.WriteLine("Entry: " + entry.name);
        }
    }

    [Fact]
    public void TestCompressed()
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testFilePath = Path.Combine(projectDir, "TestData", "test_compressed.milo_ps3");

        // test creating a MiloFile from that path
        MiloFile milo = new MiloFile(testFilePath);

        // print the directory name and all entry names
        Console.WriteLine("Directory name: " + milo.dirMeta.name + " of type " + milo.dirMeta.type);

        foreach (DirectoryMeta.Entry entry in milo.dirMeta.entries)
        {
            Console.WriteLine("Entry: " + entry.name);
        }
    }

    [Fact]
    public void TestDirectoryCreation()
    {
        DirectoryMeta meta = DirectoryMeta.New("ObjectDir", "testing_directory");

        // add some fake entries
        Object obj1 = new Object();
        DirectoryMeta.Entry entry1 = new DirectoryMeta.Entry("Object", "basic_object", obj1);
        Object obj2 = new Object();
        DirectoryMeta.Entry entry2 = new DirectoryMeta.Entry("Object", "testing_object", obj2);

        meta.entries.Add(entry1);
        meta.entries.Add(entry2);

        // create MemoryStream to write to
        MemoryStream mem = new MemoryStream();
        EndianWriter writer = new EndianWriter(mem, Endian.LittleEndian);

        meta.Write(writer);

        // read it back
        mem.Seek(0, SeekOrigin.Begin);
        EndianReader reader = new EndianReader(mem, Endian.LittleEndian);
        DirectoryMeta newMeta = new DirectoryMeta().Read(reader);

        // asserts to make sure it has the right name, 3 entries with the right name and type, etc.
        Assert.Equal("testing_directory", newMeta.name);
    }
}
