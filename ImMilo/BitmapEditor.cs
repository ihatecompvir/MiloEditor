using ImGuiNET;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using Pfim;
using TinyDialogsNet;
using Veldrid;
using Vector2 = System.Numerics.Vector2;

namespace ImMilo;

public static class BitmapEditor
{
    private static RndTex curTexture;
    private static Texture previewTexture;
    private static TextureView previewTextureView;
    private static nint texID;

    private static bool oneToOne;
    private static float zoom = 1f;

    private static bool supportedEncoding = false;
    private static Exception encodingError = null;

    private static void UpdateTexture(RndTex texture)
    {
        if (previewTexture != null)
        {
            Dispose();
        }

        curTexture = texture;
        previewTexture = null;
        previewTextureView = null;
        texID = nint.Zero;
        supportedEncoding = false;
        encodingError = null;

        if (texture == null || texture.bitmap == null)
        {
            encodingError = new ArgumentNullException(nameof(texture), "Provided texture or its bitmap is null.");
            supportedEncoding = false;
            return;
        }


        try
        {
            if (texture.bitmap.encoding == RndBitmap.TextureEncoding.DXT1_BC1 ||
                texture.bitmap.encoding == RndBitmap.TextureEncoding.DXT5_BC3 ||
                texture.bitmap.encoding == RndBitmap.TextureEncoding.ATI2_BC5)
            {
                byte[] imageData = texture.bitmap.ConvertToImage().ToArray();
                if (imageData == null || imageData.Length == 0)
                {
                    throw new NullReferenceException("ConvertToImage returned null or empty data.");
                }

                using (var ms = new MemoryStream(imageData))
                using (var image = Pfimage.FromStream(ms))
                {
                    if (image == null)
                    {
                        throw new NullReferenceException("Pfimage.FromStream returned null.");
                    }

                    image.Decompress();

                    if (image.Format != Pfim.ImageFormat.Rgba32)
                    {
                        Console.WriteLine($"Warning: Decompressed to {image.Format}, expected Rgba32. Veldrid update might fail or look incorrect.");
                    }

                    Console.WriteLine(image.Width + " x " + image.Height);

                    if (image.Width <= 0 || image.Height <= 0)
                    {
                        Console.WriteLine($"Invalid image dimensions after decoding: {image.Width}x{image.Height}");
                    }
                    uint bytesPerPixel = (uint)image.BitsPerPixel / 8;
                    if (bytesPerPixel == 0)
                        Console.WriteLine($"Invalid BitsPerPixel ({image.BitsPerPixel}) from decoded image.");

                    uint expectedDataSize = (uint)(image.Width * image.Height * bytesPerPixel);
                    if (image.DataLen != expectedDataSize)
                    {
                        throw new Exception($"Decompressed image data length ({image.DataLen}) does not match expected size ({expectedDataSize}) for {image.Width}x{image.Height} @ {image.BitsPerPixel}bpp. Format was {image.Format}.");
                    }

                    previewTexture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                        (uint)image.Width, (uint)image.Height, 1, 1,
                        PixelFormat.B8_G8_R8_A8_UNorm,
                        TextureUsage.Sampled));

                    Program.gd.UpdateTexture(
                        previewTexture,
                        image.Data,
                        0, 0, 0,
                        (uint)image.Width, (uint)image.Height, 1,
                        0, 0);

                    previewTextureView = Program.gd.ResourceFactory.CreateTextureView(previewTexture);
                    texID = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, previewTextureView);
                    supportedEncoding = true;
                }
            }
            else
            {
                supportedEncoding = false;
            }
        }
        catch (Exception e)
        {
            supportedEncoding = false;
            encodingError = e;
            try { previewTextureView?.Dispose(); } catch { }
            try { previewTexture?.Dispose(); } catch { }
            texID = nint.Zero;
        }
    }

    public async static void Dispose()
    {
        // Vulkan was complaining that the texture was being destroyed while the TextureView was still accessing it.
        // Waiting until the end of the frame seems to have fixed it.
        var viewToDispose = previewTextureView;
        var textureToDispose = previewTexture;

        if (viewToDispose == null && textureToDispose == null)
        {
            return;
        }

        previewTextureView = null;
        previewTexture = null;
        texID = nint.Zero;

        var tcs = new TaskCompletionSource();
        if (Program.callAfterFrame != null)
        {
            Program.callAfterFrame.Add(tcs);
        }
        else
        {
            try
            {
                viewToDispose?.Dispose();
                textureToDispose?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during immediate disposal fallback: {ex}");
            }
            return;
        }


        try
        {
            await tcs.Task;

            viewToDispose?.Dispose();
            textureToDispose?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during delayed disposal: {ex}");
            try { viewToDispose?.Dispose(); } catch { /* Ignored */ }
            try { textureToDispose?.Dispose(); } catch { /* Ignored */ }
        }
    }

    public static Vector2 FitToRect(Vector2 child, Vector2 container)
    {
        var scale = MathF.Min(container.X / child.X, container.Y / child.Y);
        return new Vector2(child.X * scale, child.Y * scale);
    }

    public static void Draw(RndTex texture)
    {
        if (texture != curTexture)
        {
            UpdateTexture(texture);
        }

        if (ImGui.Button("Import"))
            ImportButton();
        ImGui.SameLine();
        if (ImGui.Button("Export"))
            ExportButton();
        if (!supportedEncoding)
        {
            if (encodingError != null)
            {
                ImGui.TextWrapped("Failed to decode texture: " + encodingError.Message);
            }
            else
            {
                ImGui.Text("Unsupported encoding: " + texture.bitmap.encoding);
            }
            return;
        }
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
        var scaledSize = oneToOne ? imageSize : FitToRect(imageSize, windowSize) * zoom;
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
        ImGui.BeginChild("texture window", windowSize, 0, ImGuiWindowFlags.HorizontalScrollbar);
        var drawList = ImGui.GetWindowDrawList();
        ImGui.SetCursorPos(imagePos);
        ImGui.Image(texID, scaledSize);
        ImGui.EndChild();
        drawList.AddRectFilled(ImGui.GetWindowViewport().Pos, ImGui.GetWindowViewport().Pos + windowSize, 0x000000ff);
        ImGui.PopStyleVar();
    }

    private static void ExportButton()
    {
        var bytes = curTexture.bitmap.ConvertToImage();
        var (canceled, path) = TinyDialogs.SaveFileDialog("Save As DDS", "", new FileFilter("DDS files", ["*.dds"]));

        if (!canceled)
        {
            File.WriteAllBytes(path, bytes.ToArray());
        }
    }

    private static void ImportButton()
    {
        var (canceled, path) = TinyDialogs.OpenFileDialog("Import DDS", "", false, new FileFilter("DDS files", ["*.dds"]));

        if (canceled) return;
        // read the file into a MemoryStream
        byte[] bytes = File.ReadAllBytes(path.First());
        MemoryStream stream = new MemoryStream(bytes);

        // create an EndianReader on the stream
        EndianReader reader = new EndianReader(stream, Endian.LittleEndian);

        // create a DDS object and read the data from the stream
        DDS dds = new DDS().Read(reader);

        // update the curTexture with the new DDS data
        curTexture.bitmap.height = (ushort)dds.dwHeight;
        curTexture.bitmap.width = (ushort)dds.dwWidth;
        curTexture.bitmap.bpp = (byte)dds.pf.dwRGBBitCount;
        curTexture.bitmap.mipMaps = (byte)(dds.dwMipMapCount - 1);

        if (curTexture.bitmap.platform == DirectoryMeta.Platform.Xbox)
        {

            // scramble every 4 dds pixels for 360
            List<List<byte>> swappedBytes = new List<List<byte>>();
            for (int i = 0; i < dds.pixels.Count; i += 4)
            {
                List<byte> swapped = new List<byte>();
                swapped.Add(dds.pixels[i + 1]);
                swapped.Add(dds.pixels[i]);
                swapped.Add(dds.pixels[i + 3]);
                swapped.Add(dds.pixels[i + 2]);
                swappedBytes.Add(swapped);
            }

            curTexture.bitmap.textures = swappedBytes;
        }
        else
        {
            // don't do anything special for other platforms
            curTexture.bitmap.textures = new List<List<byte>>() { dds.pixels };
        }

        curTexture.height = dds.dwHeight;
        curTexture.width = dds.dwWidth;
        switch (dds.pf.dwFourCC)
        {
            case 0x31545844:
                curTexture.bitmap.encoding = RndBitmap.TextureEncoding.DXT1_BC1;
                curTexture.bpp = 4;
                curTexture.bitmap.bpp = 4;
                curTexture.bitmap.bpl = (ushort)((curTexture.bitmap.bpp * curTexture.bitmap.width) / 8);
                break;
            case 0x35545844:
                curTexture.bitmap.encoding = RndBitmap.TextureEncoding.DXT5_BC3;
                curTexture.bpp = 8;
                curTexture.bitmap.bpp = 8;
                curTexture.bitmap.bpl = (ushort)((curTexture.bitmap.bpp * curTexture.bitmap.width) / 8);
                break;
            case 0x32495441:
                curTexture.bitmap.encoding = RndBitmap.TextureEncoding.ATI2_BC5;
                curTexture.bpp = 8;
                curTexture.bitmap.bpp = 8;
                curTexture.bitmap.bpl = (ushort)((curTexture.bitmap.bpp * curTexture.bitmap.width) / 8);
                break;
        }

        // Update the preview
        UpdateTexture(curTexture);
    }
}