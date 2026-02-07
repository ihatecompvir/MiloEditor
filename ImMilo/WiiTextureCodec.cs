using System;

namespace ImMilo;

/// <summary>
/// Decodes Wii TPL texture formats with proper block-based (tiled) memory layout.
/// All multi-byte values are Big-Endian and must be byte-swapped.
/// </summary>
public static class WiiTextureCodec
{
    /// <summary>
    /// Decodes CMP (DXT1 variant) format. 8x8 blocks containing four 4x4 DXT1 sub-blocks.
    /// </summary>
    public static byte[] DecodeCMP(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 8;
        int blocksY = height / 8;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                DecodeDXT1SubBlock(data, ref srcOffset, output, width, bx * 8 + 0, by * 8 + 0);
                DecodeDXT1SubBlock(data, ref srcOffset, output, width, bx * 8 + 4, by * 8 + 0);
                DecodeDXT1SubBlock(data, ref srcOffset, output, width, bx * 8 + 0, by * 8 + 4);
                DecodeDXT1SubBlock(data, ref srcOffset, output, width, bx * 8 + 4, by * 8 + 4);
            }
        }

        return output;
    }

    private static void DecodeDXT1SubBlock(byte[] data, ref int offset, byte[] output, int width, int baseX, int baseY)
    {
        ushort color0 = ReadUInt16BE(data, offset);
        offset += 2;
        ushort color1 = ReadUInt16BE(data, offset);
        offset += 2;

        uint lookupTable = ReadUInt32BE(data, offset);
        offset += 4;

        byte[] c0 = DecodeRGB565(color0);
        byte[] c1 = DecodeRGB565(color1);

        byte[] c2, c3;
        if (color0 > color1)
        {
            c2 = new byte[4];
            c3 = new byte[4];
            for (int i = 0; i < 3; i++)
            {
                c2[i] = (byte)((2 * c0[i] + c1[i]) / 3);
                c3[i] = (byte)((c0[i] + 2 * c1[i]) / 3);
            }
            c2[3] = 255;
            c3[3] = 255;
        }
        else
        {
            c2 = new byte[4];
            c3 = new byte[] { 0, 0, 0, 0 };
            for (int i = 0; i < 3; i++)
            {
                c2[i] = (byte)((c0[i] + c1[i]) / 2);
            }
            c2[3] = 255;
        }

        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 4; px++)
            {
                int pixelIndex = py * 4 + px;
                int colorIndex = (int)((lookupTable >> (30 - pixelIndex * 2)) & 0x03);

                byte[] color = colorIndex switch
                {
                    0 => c0,
                    1 => c1,
                    2 => c2,
                    _ => c3
                };

                int outX = baseX + px;
                int outY = baseY + py;
                int outOffset = (outY * width + outX) * 4;

                output[outOffset + 0] = color[0];
                output[outOffset + 1] = color[1];
                output[outOffset + 2] = color[2];
                output[outOffset + 3] = color[3];
            }
        }
    }

    /// <summary>
    /// Decodes RGB5A3 format. 4x4 blocks with variable alpha encoding.
    /// </summary>
    public static byte[] DecodeRGB5A3(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 4;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 4; px++)
                    {
                        ushort pixel = ReadUInt16BE(data, srcOffset);
                        srcOffset += 2;

                        byte r, g, b, a;

                        if ((pixel & 0x8000) == 0)
                        {
                            int a3 = (pixel >> 12) & 0x7;
                            a = (byte)((a3 << 5) | (a3 << 2) | (a3 >> 1));

                            int r4 = (pixel >> 8) & 0xF;
                            r = (byte)((r4 << 4) | r4);

                            int g4 = (pixel >> 4) & 0xF;
                            g = (byte)((g4 << 4) | g4);

                            int b4 = pixel & 0xF;
                            b = (byte)((b4 << 4) | b4);
                        }
                        else
                        {
                            a = 255;

                            int r5 = (pixel >> 10) & 0x1F;
                            r = (byte)((r5 << 3) | (r5 >> 2));

                            int g5 = (pixel >> 5) & 0x1F;
                            g = (byte)((g5 << 3) | (g5 >> 2));

                            int b5 = pixel & 0x1F;
                            b = (byte)((b5 << 3) | (b5 >> 2));
                        }

                        int outX = bx * 4 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = r;
                        output[outOffset + 1] = g;
                        output[outOffset + 2] = b;
                        output[outOffset + 3] = a;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes RGB565 format. 4x4 blocks.
    /// </summary>
    public static byte[] DecodeRGB565Format(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 4;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 4; px++)
                    {
                        ushort pixel = ReadUInt16BE(data, srcOffset);
                        srcOffset += 2;

                        byte[] rgb = DecodeRGB565(pixel);

                        int outX = bx * 4 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = rgb[0];
                        output[outOffset + 1] = rgb[1];
                        output[outOffset + 2] = rgb[2];
                        output[outOffset + 3] = 255;   
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes I4 format (4-bit intensity). 8x8 blocks.
    /// </summary>
    public static byte[] DecodeI4(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 8;
        int blocksY = height / 8;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 8; py++)
                {
                    for (int px = 0; px < 8; px += 2)
                    {
                        byte packed = data[srcOffset++];

                        int i1 = (packed >> 4) & 0xF;
                        int i2 = packed & 0xF;

                        byte intensity1 = (byte)((i1 << 4) | i1);
                        byte intensity2 = (byte)((i2 << 4) | i2);

                        int outX1 = bx * 8 + px;
                        int outY1 = by * 8 + py;
                        int outOffset1 = (outY1 * width + outX1) * 4;
                        output[outOffset1 + 0] = intensity1;
                        output[outOffset1 + 1] = intensity1;
                        output[outOffset1 + 2] = intensity1;
                        output[outOffset1 + 3] = 255;

                        int outX2 = bx * 8 + px + 1;
                        int outY2 = by * 8 + py;
                        int outOffset2 = (outY2 * width + outX2) * 4;
                        output[outOffset2 + 0] = intensity2;
                        output[outOffset2 + 1] = intensity2;
                        output[outOffset2 + 2] = intensity2;
                        output[outOffset2 + 3] = 255;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes I8 format (8-bit intensity). 8x4 blocks.
    /// </summary>
    public static byte[] DecodeI8(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 8;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 8; px++)
                    {
                        byte intensity = data[srcOffset++];

                        int outX = bx * 8 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = intensity;
                        output[outOffset + 1] = intensity;
                        output[outOffset + 2] = intensity;
                        output[outOffset + 3] = 255;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes IA4 format (4-bit intensity + 4-bit alpha). 8x4 blocks.
    /// </summary>
    public static byte[] DecodeIA4(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 8;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 8; px++)
                    {
                        byte packed = data[srcOffset++];

                        int a4 = (packed >> 4) & 0xF;
                        int i4 = packed & 0xF;

                        byte alpha = (byte)((a4 << 4) | a4);
                        byte intensity = (byte)((i4 << 4) | i4);

                        int outX = bx * 8 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = intensity;
                        output[outOffset + 1] = intensity;
                        output[outOffset + 2] = intensity;
                        output[outOffset + 3] = alpha;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes IA8 format (8-bit intensity + 8-bit alpha). 4x4 blocks.
    /// </summary>
    public static byte[] DecodeIA8(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 4;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 4; px++)
                    {
                        ushort packed = ReadUInt16BE(data, srcOffset);
                        srcOffset += 2;

                        byte alpha = (byte)(packed >> 8);
                        byte intensity = (byte)(packed & 0xFF);

                        int outX = bx * 4 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = intensity;
                        output[outOffset + 1] = intensity;
                        output[outOffset + 2] = intensity;
                        output[outOffset + 3] = alpha;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Decodes RGBA8 format (split channel). 4x4 blocks with AR and GB planes.
    /// </summary>
    public static byte[] DecodeRGBA8(byte[] data, int width, int height)
    {
        byte[] output = new byte[width * height * 4];
        int blocksX = width / 4;
        int blocksY = height / 4;

        int srcOffset = 0;

        for (int by = 0; by < blocksY; by++)
        {
            for (int bx = 0; bx < blocksX; bx++)
            {
                byte[] arPlane = new byte[32];
                byte[] gbPlane = new byte[32];

                Array.Copy(data, srcOffset, arPlane, 0, 32);
                Array.Copy(data, srcOffset + 32, gbPlane, 0, 32);
                srcOffset += 64;

                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 4; px++)
                    {
                        int pixelIndex = py * 4 + px;

                        ushort ar = ReadUInt16BE(arPlane, pixelIndex * 2);
                        byte alpha = (byte)(ar >> 8);
                        byte red = (byte)(ar & 0xFF);

                        ushort gb = ReadUInt16BE(gbPlane, pixelIndex * 2);
                        byte green = (byte)(gb >> 8);
                        byte blue = (byte)(gb & 0xFF);

                        int outX = bx * 4 + px;
                        int outY = by * 4 + py;
                        int outOffset = (outY * width + outX) * 4;

                        output[outOffset + 0] = red;
                        output[outOffset + 1] = green;
                        output[outOffset + 2] = blue;
                        output[outOffset + 3] = alpha;
                    }
                }
            }
        }

        return output;
    }

    // Helper methods

    private static ushort ReadUInt16BE(byte[] data, int offset)
    {
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static uint ReadUInt32BE(byte[] data, int offset)
    {
        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                     (data[offset + 2] << 8) | data[offset + 3]);
    }

    private static byte[] DecodeRGB565(ushort color)
    {
        int r5 = (color >> 11) & 0x1F;
        int g6 = (color >> 5) & 0x3F;
        int b5 = color & 0x1F;

        byte r = (byte)((r5 << 3) | (r5 >> 2));
        byte g = (byte)((g6 << 2) | (g6 >> 4));
        byte b = (byte)((b5 << 3) | (b5 >> 2));

        return new byte[] { r, g, b, 255 };
    }
}
