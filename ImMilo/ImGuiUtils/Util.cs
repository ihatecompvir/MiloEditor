using System.Numerics;
using BigGustave;
using ImGuiNET;
using MiloLib.Assets;
using MiloIcons;
using Veldrid;

namespace ImMilo.ImGuiUtils;

public static class Util
{
    
    private static Dictionary<string, char> IconCodePoints = new();
    public static ImFontPtr iconFont;

    public static char GetIconCodePoint()
    {
        return (char)0xE000;
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
    
    private static Dictionary<string, nint> assetIcons = new();

    public static nint GetAssetIcon(string typeName)
    {
        if (!assetIcons.TryGetValue(typeName, out nint icon))
        {
            var iconStream = Icons.GetMiloIconStream(Icons.GetIconAssetPath(typeName));
            var png = Png.Open(iconStream);
            // TODO: find a library that can just spit out a RGBA32 stream instead of this silliness
            int[] data = new int[png.Width * png.Height];
            for (int y = 0; y < png.Height; y++)
            {
                for (int x = 0; x < png.Width; x++)
                {
                    var pixel = png.GetPixel(x, y);
                    byte[] pixelArray = [pixel.R, pixel.G, pixel.B, pixel.A];
                    data[x + y * png.Width] = BitConverter.ToInt32(pixelArray, 0);
                }
            }
            var texture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)png.Width, (uint)png.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            Program.gd.UpdateTexture(texture, data, 0, 0, 0, (uint)png.Width, (uint)png.Height, 1, 0, 0);
            icon = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, texture);
            assetIcons.Add(typeName, icon);
        }

        return icon;
    }

    private static (string, string) QueryEntryOrDir(object obj)
    {
        return obj switch
        {
            DirectoryMeta meta => (meta.name, meta.type),
            DirectoryMeta.Entry entry => (entry.name, entry.type),
            _ => throw new ArgumentException("Object is not a DirectoryMeta or an Entry")
        };
    }

    public static bool SceneTreeItem(DirectoryMeta dir, ImGuiTreeNodeFlags flags)
    {
        return SceneTreeItem((object)dir, flags);
    }
    
    public static bool SceneTreeItem(DirectoryMeta.Entry dir, ImGuiTreeNodeFlags flags)
    {
        return SceneTreeItem((object)dir, flags);
    }

    private static bool SceneTreeItem(object obj, ImGuiTreeNodeFlags flags)
    {
        var (name, type) = QueryEntryOrDir(obj);
        var homePos = ImGui.GetCursorScreenPos();
        var treeOpen = ImGui.TreeNodeEx(Util.GetIconCodePoint() + name, flags);
        var drawList = ImGui.GetWindowDrawList();
        var iconSize = Settings.Startup.fontSettings.IconSize;
        var imagePos = homePos + new Vector2(iconSize+5 + ImGui.GetStyle().FramePadding.X, 0);
        drawList.AddImage(GetAssetIcon(type), imagePos, imagePos+new Vector2(iconSize, iconSize));
        return treeOpen;
    }
    
    /// <summary>
    /// Creates a blank character in the font with the size of an icon, with a code point of 0xE000 (start of the
    /// Unicode Private Use Area)
    /// </summary>
    /// <param name="font">The font the glyph will exist inside of.</param>
    public static unsafe void CreateIconGlyph(ImFontPtr font)
    {
        var io = ImGui.GetIO();
        const int puaStart = 0xE000;
        var iconSize = Settings.Startup.fontSettings.IconSize;
        io.Fonts.AddCustomRectFontGlyph(font, (ushort)puaStart, iconSize, iconSize, iconSize + 5);
    }
}