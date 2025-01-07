using System.Numerics;
using ImGuiNET;
using MiloLib.Assets.Rnd;
using Pfim;
using Veldrid;

namespace ImMilo;

public class BitmapEditor
{
    private static RndTex curTexture;
    private static Texture previewTexture;
    private static TextureView previewTextureView;
    private static nint texID;

    private static bool oneToOne;
    private static float zoom = 1f;

    private static void UpdateTexture(RndTex texture)
    {
        if (previewTexture != null)
        {
            Dispose();
        }
        curTexture = texture;
        if (texture.bitmap.encoding == RndBitmap.TextureEncoding.DXT1_BC1 ||
            texture.bitmap.encoding == RndBitmap.TextureEncoding.DXT5_BC3 ||
            texture.bitmap.encoding == RndBitmap.TextureEncoding.ATI2_BC5)
        {
            using (var image = Pfimage.FromStream(new MemoryStream(texture.bitmap.ConvertToImage().ToArray())))
            {
                image.Decompress();
                Console.WriteLine(image.Width + " x " + image.Height);
                previewTexture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    (uint)image.Width, (uint)image.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled));
                Program.gd.UpdateTexture(previewTexture, image.Data, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
                previewTextureView = Program.gd.ResourceFactory.CreateTextureView(previewTexture);
                texID = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, previewTextureView);
            }
        }
        else
        {
            // We don't go here.
            throw new Exception("Unsupported texture encoding.");
        }
    }

    public static void Dispose()
    {
        previewTexture.Dispose();
        previewTextureView.Dispose();
    }

    public static Vector2 FitToRect(Vector2 child, Vector2 container)
    {
        var scale = MathF.Min(container.X/child.X, container.Y/child.Y);
        return new Vector2(child.X * scale, child.Y * scale);
    }

    public static void Draw(RndTex texture)
    {
        if (texture != curTexture)
        {
            UpdateTexture(texture);
        }

        ImGui.Button("Import");
        ImGui.SameLine();
        ImGui.Button("Export");
        ImGui.SameLine();
        ImGui.Text(texture.width + " x " + texture.height + ", " + texture.bitmap.encoding);
        ImGui.Checkbox("1:1", ref oneToOne);
        if (!oneToOne)
        {
            ImGui.SameLine();
            ImGui.SliderFloat("Zoom", ref zoom, 1, 5);
        }
        var windowSize = ImGui.GetContentRegionAvail();
        var imageSize = new Vector2(texture.width, texture.height);
        var scaledSize = oneToOne ? imageSize : FitToRect(imageSize, windowSize)*zoom;
        var centerPos = windowSize / 2f;
        var imagePos = centerPos - scaledSize / 2f;
        if (scaledSize.X > windowSize.X)
        {
            imagePos.X = 0;
        }

        if (scaledSize.Y > windowSize.Y)
        {
            imagePos.Y = 0;
        }
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.BeginChild("texture window", windowSize, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
        ImGui.SetCursorPos(imagePos);
        ImGui.Image(texID, scaledSize);
        ImGui.EndChild();
        ImGui.PopStyleVar();
    }
}