using System.Reflection;

namespace MiloIcons;

public class Icons
{
    public static Stream? GetMiloIconStream(string typeName)
    {
        Assembly assembly = typeof(Icons).Assembly;
        var outStream = assembly.GetManifestResourceStream(typeName);
        if (outStream == null)
        {
            outStream = assembly.GetManifestResourceStream("default");
        }
        return outStream;
    }
}