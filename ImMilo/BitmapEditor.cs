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

                    byte[] pixelData;
                    if (image.Format == Pfim.ImageFormat.Rgba32)
                    {
                        pixelData = image.Data;
                    }
                    else if (image.Format == Pfim.ImageFormat.Rgb24)
                    {
                        // Convert BGR24 to BGRA32 (add alpha=255)
                        pixelData = new byte[image.Width * image.Height * 4];
                        int srcStride = image.Stride;
                        for (int y = 0; y < image.Height; y++)
                        {
                            int srcRow = y * srcStride;
                            int dstRow = y * image.Width * 4;
                            for (int x = 0; x < image.Width; x++)
                            {
                                pixelData[dstRow + x * 4 + 0] = image.Data[srcRow + x * 3 + 0];
                                pixelData[dstRow + x * 4 + 1] = image.Data[srcRow + x * 3 + 1];
                                pixelData[dstRow + x * 4 + 2] = image.Data[srcRow + x * 3 + 2];
                                pixelData[dstRow + x * 4 + 3] = 255;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Unsupported decompressed format: {image.Format}");
                    }

                    previewTexture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                        (uint)image.Width, (uint)image.Height, 1, 1,
                        PixelFormat.B8_G8_R8_A8_UNorm,
                        TextureUsage.Sampled));

                    Program.gd.UpdateTexture(
                        previewTexture,
                        pixelData,
                        0, 0, 0,
                        (uint)image.Width, (uint)image.Height, 1,
                        0, 0);

                    previewTextureView = Program.gd.ResourceFactory.CreateTextureView(previewTexture);
                    texID = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, previewTextureView);
                    supportedEncoding = true;
                }
            }
            else if (texture.bitmap.encoding == RndBitmap.TextureEncoding.TPL_CMP ||
                     texture.bitmap.encoding == RndBitmap.TextureEncoding.TPL_CMP_ALPHA ||
                     texture.bitmap.encoding == RndBitmap.TextureEncoding.TPL_CMP_2)
            {
                // Handle Wii TPL textures using WiiTextureCodec
                if (texture.bitmap.textures.Count == 0 || texture.bitmap.textures[0].Count == 0)
                {
                    throw new Exception("No texture data available for TPL texture.");
                }

                // Get raw texture data (first mipmap)
                byte[] rawData = texture.bitmap.textures[0].ToArray();
                int width = texture.bitmap.width;
                int height = texture.bitmap.height;
                int bpp = texture.bitmap.bpp;

                byte[] rgbaData;

                // Check if this texture has a separate alpha channel
                if (bpp == 4 && texture.bitmap.wiiAlphaNum == 4)
                {
                    // Hidden alpha texture after RGB data
                    int dxSize = (width * height * bpp) / 8;

                    byte[] rgbData = new byte[dxSize];
                    byte[] alphaData = new byte[dxSize];

                    Array.Copy(rawData, 0, rgbData, 0, rgbData.Length);
                    Array.Copy(rawData, rawData.Length - rgbData.Length, alphaData, 0, alphaData.Length);

                    // Decode both RGB and alpha as DXT1/CMP
                    byte[] decodedRgb = WiiTextureCodec.DecodeCMP(rgbData, width, height);
                    byte[] decodedAlpha = WiiTextureCodec.DecodeCMP(alphaData, width, height);

                    // Combine: use green channel from alpha texture as final alpha channel
                    rgbaData = new byte[decodedRgb.Length];
                    for (int i = 0; i < decodedRgb.Length; i += 4)
                    {
                        rgbaData[i + 0] = decodedRgb[i + 0];     // R
                        rgbaData[i + 1] = decodedRgb[i + 1];     // G
                        rgbaData[i + 2] = decodedRgb[i + 2];     // B
                        rgbaData[i + 3] = decodedAlpha[i + 1];   // A from green channel of alpha texture
                    }
                }
                else if (bpp == 8)
                {
                    // 8bpp: data is split in half - first half RGB, second half alpha
                    int halfSize = rawData.Length / 2;
                    byte[] rgbData = new byte[halfSize];
                    byte[] alphaData = new byte[halfSize];

                    Array.Copy(rawData, 0, rgbData, 0, halfSize);
                    Array.Copy(rawData, halfSize, alphaData, 0, halfSize);

                    // Decode both RGB and alpha as DXT1/CMP
                    byte[] decodedRgb = WiiTextureCodec.DecodeCMP(rgbData, width, height);
                    byte[] decodedAlpha = WiiTextureCodec.DecodeCMP(alphaData, width, height);

                    // Combine: use green channel from alpha texture as final alpha channel
                    rgbaData = new byte[decodedRgb.Length];
                    for (int i = 0; i < decodedRgb.Length; i += 4)
                    {
                        rgbaData[i + 0] = decodedRgb[i + 0];     // R
                        rgbaData[i + 1] = decodedRgb[i + 1];     // G
                        rgbaData[i + 2] = decodedRgb[i + 2];     // B
                        rgbaData[i + 3] = decodedAlpha[i + 1];   // A from green channel of alpha texture
                    }
                }
                else
                {
                    // Standard CMP without separate alpha (bpp == 4)
                    rgbaData = WiiTextureCodec.DecodeCMP(rawData, width, height);
                }

                // Veldrid expects B8_G8_R8_A8_UNorm (BGRA order)
                // Convert from RGBA to BGRA
                byte[] bgraData = new byte[rgbaData.Length];
                for (int i = 0; i < rgbaData.Length; i += 4)
                {
                    bgraData[i + 0] = rgbaData[i + 2];
                    bgraData[i + 1] = rgbaData[i + 1];
                    bgraData[i + 2] = rgbaData[i + 0];
                    bgraData[i + 3] = rgbaData[i + 3];
                }

                previewTexture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    (uint)width, (uint)height, 1, 1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.Sampled));

                Program.gd.UpdateTexture(
                    previewTexture,
                    bgraData,
                    0, 0, 0,
                    (uint)width, (uint)height, 1,
                    0, 0);

                previewTextureView = Program.gd.ResourceFactory.CreateTextureView(previewTexture);
                texID = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, previewTextureView);
                supportedEncoding = true;
            }
            else if (texture.bitmap.encoding == RndBitmap.TextureEncoding.RGBA ||
                     texture.bitmap.encoding == RndBitmap.TextureEncoding.ARGB)
            {
                // Handle uncompressed RGBA/ARGB textures
                if (texture.bitmap.textures.Count == 0 || texture.bitmap.textures[0].Count == 0)
                {
                    throw new Exception("No texture data available for RGBA/ARGB texture.");
                }

                // Get raw pixel data (first mipmap)
                byte[] rawData = texture.bitmap.textures[0].ToArray();
                int width = texture.bitmap.width;
                int height = texture.bitmap.height;
                int bpp = texture.bitmap.bpp;
                byte[] expandedData;

                if (bpp == 8 || bpp == 4)
                {
                    // Check if we have a color palette
                    if (texture.bitmap.colorPalette == null || texture.bitmap.colorPalette.Count == 0)
                    {
                        throw new Exception($"Paletted texture ({bpp}bpp) has no color palette data.");
                    }

                    int paletteColorCount = 1 << bpp;
                    int expectedPaletteSize = paletteColorCount * 4;

                    if (texture.bitmap.colorPalette.Count != expectedPaletteSize)
                    {
                        throw new Exception($"Expected {expectedPaletteSize} bytes in palette for {bpp}bpp, but got {texture.bitmap.colorPalette.Count}.");
                    }

                    int pixelCount = width * height;
                    expandedData = new byte[pixelCount * 4];

                    if (bpp == 8)
                    {
                        for (int i = 0; i < pixelCount; i++)
                        {
                            byte originalIndex = rawData[i];

                            int paletteIndex = (0xE7 & originalIndex) |      
                                             ((0x08 & originalIndex) << 1) | 
                                             ((0x10 & originalIndex) >> 1);  

                            int paletteOffset = paletteIndex * 4;

                            byte r = texture.bitmap.colorPalette[paletteOffset + 0];
                            byte g = texture.bitmap.colorPalette[paletteOffset + 1];
                            byte b = texture.bitmap.colorPalette[paletteOffset + 2];
                            byte a = texture.bitmap.colorPalette[paletteOffset + 3];

                            a = ((a & 0x80) != 0) ? (byte)0xFF : (byte)(a << 1);

                            expandedData[i * 4 + 0] = r;
                            expandedData[i * 4 + 1] = g;
                            expandedData[i * 4 + 2] = b;
                            expandedData[i * 4 + 3] = a;
                        }
                    }
                    else // 4bpp
                    {
                        for (int i = 0; i < rawData.Length; i++)
                        {
                            byte packedIndices = rawData[i];

                            byte indexLow = (byte)(packedIndices & 0x0F);        
                            byte indexHigh = (byte)((packedIndices >> 4) & 0x0F);

                            int paletteOffset1 = indexLow * 4;
                            int pixel1Offset = (i * 2) * 4;

                            if (pixel1Offset + 3 < expandedData.Length)
                            {
                                byte r = texture.bitmap.colorPalette[paletteOffset1 + 0];
                                byte g = texture.bitmap.colorPalette[paletteOffset1 + 1];
                                byte b = texture.bitmap.colorPalette[paletteOffset1 + 2];
                                byte a = texture.bitmap.colorPalette[paletteOffset1 + 3];

                                // PS2 alpha fix
                                a = ((a & 0x80) != 0) ? (byte)0xFF : (byte)(a << 1);

                                expandedData[pixel1Offset + 0] = r;
                                expandedData[pixel1Offset + 1] = g;
                                expandedData[pixel1Offset + 2] = b;
                                expandedData[pixel1Offset + 3] = a;
                            }

                            int paletteOffset2 = indexHigh * 4;
                            int pixel2Offset = (i * 2 + 1) * 4;

                            if (pixel2Offset + 3 < expandedData.Length)
                            {
                                byte r = texture.bitmap.colorPalette[paletteOffset2 + 0];
                                byte g = texture.bitmap.colorPalette[paletteOffset2 + 1];
                                byte b = texture.bitmap.colorPalette[paletteOffset2 + 2];
                                byte a = texture.bitmap.colorPalette[paletteOffset2 + 3];

                                // PS2 alpha fix
                                a = ((a & 0x80) != 0) ? (byte)0xFF : (byte)(a << 1);

                                expandedData[pixel2Offset + 0] = r;
                                expandedData[pixel2Offset + 1] = g;
                                expandedData[pixel2Offset + 2] = b;
                                expandedData[pixel2Offset + 3] = a;
                            }
                        }
                    }

                    rawData = expandedData;
                }
                else if (bpp != 32)
                {
                    throw new Exception($"Unsupported bpp for RGBA/ARGB: {bpp}. Supported values: 4, 8, 32.");
                }

                byte[] convertedData = new byte[rawData.Length];

                for (int i = 0; i < rawData.Length; i += 4)
                {
                    if (texture.bitmap.encoding == RndBitmap.TextureEncoding.RGBA)
                    {
                        // RGBA --> BGRA: swap R and B
                        convertedData[i + 0] = rawData[i + 2];
                        convertedData[i + 1] = rawData[i + 1];
                        convertedData[i + 2] = rawData[i + 0];
                        convertedData[i + 3] = rawData[i + 3];
                    }
                    else // ARGB
                    {
                        // ARGB <-- BGRA: rearrange channels
                        convertedData[i + 0] = rawData[i + 3];
                        convertedData[i + 1] = rawData[i + 2];
                        convertedData[i + 2] = rawData[i + 1];
                        convertedData[i + 3] = rawData[i + 0];
                    }
                }

                previewTexture = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    (uint)width, (uint)height, 1, 1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.Sampled));

                Program.gd.UpdateTexture(
                    previewTexture,
                    convertedData,
                    0, 0, 0,
                    (uint)width, (uint)height, 1,
                    0, 0);

                previewTextureView = Program.gd.ResourceFactory.CreateTextureView(previewTexture);
                texID = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, previewTextureView);
                supportedEncoding = true;
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