using System.Numerics;
using BigGustave;
using ImGuiNET;
using MiloLib.Assets;
using MiloIcons;

namespace ImMilo.ImGuiUtils;

public static class Util
{
    
    private static Dictionary<string, char> IconCodePoints = new();
    public static ImFontPtr iconFont;

    public static char GetIconCodePoint(string assetType)
    {
        if (!IconCodePoints.ContainsKey(assetType))
        {
            return IconCodePoints["default"];
        }
        return IconCodePoints[assetType];
    }
    
    public static unsafe bool InputUInt(string label, ref uint value)
    {
        fixed (uint* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U32, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputShort(string label, ref short value)
    {
        fixed (short* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.S16, (IntPtr)ptr);
        }
    }

    public static unsafe bool InputUShort(string label, ref ushort value)
    {
        fixed (ushort* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U16, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputLong(string label, ref long value)
    {
        fixed (long* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.S64, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputULong(string label, ref ulong value)
    {
        fixed (ulong* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U64, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputByte(string label, ref byte value)
    {
        fixed (byte* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U8, (IntPtr)ptr);
        }
    }

    public static void SceneTreeItem(DirectoryMeta.Entry entry)
    {
        var drawList = ImGui.GetWindowDrawList();
        var cornerPos = ImGui.GetCursorPos();
        //ImGui.InvisibleButton(entry.name, new Vector2(ImGui.GetContentRegionAvail().X, 24));
        //ImGui.SetCursorPos(cornerPos);
        //ImGui.Text(entry.name);
        ImGui.ArrowButton("", ImGuiDir.Down);
    }
    
    /// <summary>
    /// Creates icons for asset types in the Unicode Private Use Area, and populates <see cref="IconCodePoints"/> with
    /// the a map from icon name -> code point.
    /// </summary>
    /// <param name="font"></param>
    public static unsafe void CreateIconsInFont(ImFontPtr font)
    {
        var io = ImGui.GetIO();
        const int puaStart = 0xE000;
        var curPoint = puaStart;
        var rectIdMap = new Dictionary<string, int>();
        foreach (string typeName in Icons.typeNames)
        {
            var assetName = Icons.MapTypeName(typeName);
            if (rectIdMap.ContainsKey(assetName))
            {
                continue;
            }
            rectIdMap.Add(assetName, io.Fonts.AddCustomRectFontGlyph(font, (ushort)curPoint, 24, 24, 24 + 10));
            IconCodePoints.Add(assetName, (char)curPoint);
            curPoint++;
        }

        io.Fonts.Build();
        byte* texPixels;
        int texWidth, texHeight;
        io.Fonts.GetTexDataAsRGBA32(out texPixels, out texWidth, out texHeight);

        foreach (string assetName in rectIdMap.Keys)
        {
            var rect = io.Fonts.GetCustomRectByIndex(rectIdMap[assetName]);
            var png = Png.Open(Icons.GetMiloIconStream(assetName));
            

            for (int y = 0; y < rect.Height; y++)
            {
                uint* p = (uint*)texPixels + (rect.Y + y) * texWidth + rect.X;
                for (int x = 0; x < rect.Width; x++)
                {
                    var pixel = png.GetPixel((int)(x * 32.0/24.0), (int)(y * 32.0/24.0));
                    uint r = (uint)pixel.R;
                    uint g = (uint)pixel.G << 8;
                    uint b = (uint)pixel.B << 16;
                    uint a = (uint)pixel.A << 24;
                    *p++ = (r | g | b | a);
                    
                }
            }
        }
    }
}