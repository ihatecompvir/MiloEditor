using System.Reflection;

namespace MiloIcons;

/// <summary>
/// 
/// </summary>
public static class Icons
{
    
    public static string[] typeNames =
    [
        "default", "ObjectDir", "RndDir", "Object", "BandSongPref", "Tex", "TexRenderer", "Group", "Mat",
        "Mesh", "MultiMesh", "Trans", "TransAnim", "Sfx", "SynthSample", "PanelDir", "Character", "Color",
        "BandCharDesc", "ColorPalette", "Light", "WorldDir", "ScreenMask", "TexMovie", "BandCrowdMeterDir",
        "Font", "Text", "BandList", "BandCamShot", "UIColor", "UILabel", "BandLabel", "CharLipSync",
        "UIButton", "BandButton", "BandStarDisplay", "Environ", "PropAnim", "MeshAnim", "Line",
        "AnimFilter", "UITrigger", "EventTrigger", "Cam", "MeshAnim", "ParticleSys", "MatAnim", "CharClip",
        "CharClipSet", "MidiInstrument", "FileMerger", "TransProxy", "PostProc", "WorldInstance",
        "CharClipGroup", "CheckboxDisplay", ""
    ];

    /// <summary>
    /// Maps a type name from DirectoryMeta.Entry's "type" property to a valid icon name.
    /// Type names pulled from MiloEditor's LoadAssetClassImages()
    /// </summary>
    /// <param name="inType">Type name to map</param>
    /// <returns>A valid icon name, to be used in <see cref="GetMiloIconStream"/></returns>
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
    
    /// <summary>
    /// Gets a stream for an icon. Doesn't always take raw type names, check <see cref="MapTypeName"/>
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
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