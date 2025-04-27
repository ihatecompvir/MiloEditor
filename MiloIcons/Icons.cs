using System.Reflection;

namespace MiloIcons;

/// <summary>
/// 
/// </summary>
public static class Icons
{

    private static Dictionary<string, string>? typeToAsset;

    private static void MapAssetPaths()
    {
        // regex to convert from MainForm.cs -> this dictionary:
        // imageList\.Images\.Add\((".*?"), Image.FromFile\((".+?")\)\);
        // typeToAsset.Add($1, $2);

        typeToAsset = new Dictionary<string, string>();
        typeToAsset.Add("default", "Images/default.png");
        typeToAsset.Add("TrackWidget", "Images/TrackWidget.png");
        typeToAsset.Add("ObjectDir", "Images/ObjectDir.png");
        typeToAsset.Add("RndDir", "Images/RndDir.png");
        typeToAsset.Add("Object", "Images/default.png");
        typeToAsset.Add("BandSongPref", "Images/BandSongPref.png");
        typeToAsset.Add("Tex", "Images/RndTex.png");
        typeToAsset.Add("TexRenderer", "Images/RndTex.png");
        typeToAsset.Add("Group", "Images/Group.png");
        typeToAsset.Add("Mat", "Images/RndMat.png");
        typeToAsset.Add("Mesh", "Images/RndMesh.png");
        typeToAsset.Add("MultiMesh", "Images/RndMultiMesh.png");
        typeToAsset.Add("Trans", "Images/RndTrans.png");
        typeToAsset.Add("TransAnim", "Images/TransAnim.png");
        typeToAsset.Add("Sfx", "Images/Sfx.png");
        typeToAsset.Add("SynthSample", "Images/Sfx.png");
        typeToAsset.Add("PanelDir", "Images/RndDir.png");
        typeToAsset.Add("Character", "Images/Character.png");
        typeToAsset.Add("Color", "Images/UIColor.png");
        typeToAsset.Add("BandCharDesc", "Images/BandCharDesc.png");
        typeToAsset.Add("ColorPalette", "Images/ColorPalette.png");
        typeToAsset.Add("Light", "Images/RndLight.png");
        typeToAsset.Add("WorldDir", "Images/WorldDir.png");
        typeToAsset.Add("ScreenMask", "Images/ScreenMask.png");
        typeToAsset.Add("TexMovie", "Images/TexMovie.png");
        typeToAsset.Add("BandCrowdMeterDir", "Images/BandCrowdMeterDir.png");
        typeToAsset.Add("Font", "Images/Font.png");
        typeToAsset.Add("Text", "Images/Text.png");
        typeToAsset.Add("BandList", "Images/Text.png");
        typeToAsset.Add("BandCamShot", "Images/Camera.png");
        typeToAsset.Add("UIColor", "Images/UIColor.png");
        typeToAsset.Add("UILabel", "Images/UILabel.png");
        typeToAsset.Add("BandLabel", "Images/UILabel.png");
        typeToAsset.Add("CharLipSync", "Images/Lipsync.png");
        typeToAsset.Add("UIButton", "Images/Button.png");
        typeToAsset.Add("BandButton", "Images/Button.png");
        typeToAsset.Add("BandStarDisplay", "Images/BandStarDisplay.png");
        typeToAsset.Add("Environ", "Images/Environ.png");
        typeToAsset.Add("PropAnim", "Images/PropAnim.png");
        typeToAsset.Add("Line", "Images/Line.png");
        typeToAsset.Add("AnimFilter", "Images/AnimFilter.png");
        typeToAsset.Add("UITrigger", "Images/Trigger.png");
        typeToAsset.Add("EventTrigger", "Images/Trigger.png");
        typeToAsset.Add("Cam", "Images/Camera.png");
        typeToAsset.Add("MeshAnim", "Images/MeshAnim.png");
        typeToAsset.Add("ParticleSys", "Images/ParticleSys.png");
        typeToAsset.Add("MatAnim", "Images/MatAnim.png");
        typeToAsset.Add("CharClip", "Images/CharClip.png");
        typeToAsset.Add("CharClipSet", "Images/CharClip.png");
        typeToAsset.Add("MidiInstrument", "Images/MidiInstrument.png");
        typeToAsset.Add("FileMerger", "Images/FileMerger.png");
        typeToAsset.Add("TransProxy", "Images/TransProxy.png");
        typeToAsset.Add("PostProc", "Images/PostProc.png");
        typeToAsset.Add("WorldInstance", "Images/WorldInstance.png");
        typeToAsset.Add("CharClipGroup", "Images/CharClipGroup.png");
        typeToAsset.Add("CheckboxDisplay", "Images/CheckboxDisplay.png");
        typeToAsset.Add("UIListDir", "Images/UIListDir.png");
        typeToAsset.Add("UIGuide", "Images/UIGuide.png");
        typeToAsset.Add("InlineHelp", "Images/InlineHelp.png");
        typeToAsset.Add("CharInterest", "Images/CharInterest.png");
        typeToAsset.Add("RandomGroupSeq", "Images/RandomGroupSeq.png");
        typeToAsset.Add("Set", "Images/Set.png");
        typeToAsset.Add("", "Images/NoDir.png");
    }

    /// <summary>
    /// Gets the icon asset path for a certain type name.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string GetIconAssetPath(string typeName)
    {
        if (typeToAsset == null)
        {
            MapAssetPaths();
        }
        return typeToAsset.ContainsKey(typeName) ? typeToAsset[typeName] : "Images/default.png";
    }

    /// <summary>
    /// Gets a stream for an icon. Doesn't take raw type names, check <see cref="GetIconAssetPath"/>
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public static Stream GetMiloIconStream(string assetPath)
    {
        Assembly assembly = typeof(Icons).Assembly;
        var outStream = assembly.GetManifestResourceStream(assetPath);
        if (outStream == null)
        {
            outStream = assembly.GetManifestResourceStream("Images/default.png");
            // if outStream is *still* null, something has gone very wrong!
            if (outStream == null)
            {
                throw new Exception("MiloIcons cannot find icon files");
            }
        }
        return outStream;
    }
}