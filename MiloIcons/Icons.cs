using System.Reflection;

namespace MiloIcons;

public class Icons
{

    public static string MapTypeName(string inType)
    {
        return inType switch
        {
            "Object" => "default",
            "Tex" => "RndTex",
            "TexRenderer" => "RndTex",
            "Mat" => "RndMat",
            "Mesh" => "RndMesh",
            "MultiMesh" => "RndMultiMesh",
            "Trans" => "RndTrans",
            "SynthSample" => "Sfx",
            "PanelDir" => "RndDir",
            "Color" => "UIColor",
            "Light" => "RndLight",
            "BandList" => "Text",
            "BandCamShot" => "Camera",
            "BandLabel" => "UILabel",
            "CharLipSync" => "LipSync",
            "UIButton" => "Button",
            "BandButton" => "Button",
            "MeshAnim" => "PropAnim",
            "UITrigger" => "Trigger",
            "EventTrigger" => "Trigger",
            "Cam" => "Camera",
            "CharClipSet" => "CharClip",
            "" => "NoDir",
            _ => inType
        };
    }
    
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