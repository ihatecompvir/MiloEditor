namespace MiloLib.Tests;
using System.Reflection;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using MiloLib.Assets.UI;
using MiloLib.Utils;

public class IOTests
{
    /// <summary>
    /// Creates an RB3-versioned ObjectDir, writes it to a MemoryStream, reads it back, and compares some fields to ensure they were written and read correctly.
    /// </summary>
    [Fact]
    public void TestRB3ObjectDirCreation()
    {
        ObjectDir objectDir = new ObjectDir(27);

        MemoryStream stream = new MemoryStream();
        EndianWriter writer = new EndianWriter(stream, Endian.BigEndian);

        objectDir.objFields.type = "Test_Directory";
        objectDir.proxyPath = "test_path.milo";

        objectDir.Write(writer, false);

        MemoryStream stream2 = new MemoryStream();

        stream.Position = 0;

        stream.CopyTo(stream2);

        stream2.Position = 0;

        EndianReader reader = new EndianReader(stream2, Endian.BigEndian);

        ObjectDir objectDir2 = new ObjectDir(27);
        objectDir2.Read(reader, false);

        // compare the two fields we set in the ObjectDirs
        Assert.Equal(objectDir.objFields.type.value, objectDir2.objFields.type.value);
        Assert.Equal(objectDir.proxyPath.value, objectDir2.proxyPath.value);

        // make sure the two MemoryStreams have the same size, meaning the same data was written and read
        Assert.Equal(stream.Length, stream2.Length);
    }
}
