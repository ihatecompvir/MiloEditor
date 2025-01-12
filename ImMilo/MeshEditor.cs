using System.Numerics;
using ImGuiNET;
using MiloLib.Assets.Rnd;
using Veldrid;

namespace ImMilo;

public static class MeshEditor
{
    static RndMesh curMesh;
    private static Vector2 viewportSize;
    private static bool initialized;
    private static Framebuffer? framebuffer;
    private static Texture? colorBuffer;
    private static Texture? depthBuffer;
    private static nint viewportId;
    private static CommandList? commandList;

    static void UpdateMesh(RndMesh newMesh)
    {
        curMesh = newMesh;
    }

    static void CreateFramebuffer(Vector2 newSize)
    {
        viewportSize = newSize;
        if (framebuffer != null)
        {
            framebuffer.Dispose();
            framebuffer = null;
        }
        if (colorBuffer != null)
        {
            colorBuffer.Dispose();
            colorBuffer = null;
        }
        if (depthBuffer != null)
        {
            depthBuffer.Dispose();
            depthBuffer = null;
        }

        if (commandList == null)
        {
            commandList = Program.gd.ResourceFactory.CreateCommandList();
        }

        colorBuffer = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)newSize.X,
            (uint)newSize.Y, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        depthBuffer = Program.gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)newSize.X,
            (uint)newSize.Y, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil));
        framebuffer = Program.gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthBuffer, colorBuffer));
        viewportId = Program.controller.GetOrCreateImGuiBinding(Program.gd.ResourceFactory, colorBuffer);
    }

    public static void Draw(RndMesh mesh)
    {
        if (mesh != curMesh)
        {
            UpdateMesh(mesh);
        }

        if (!initialized || ImGui.GetContentRegionAvail() != viewportSize)
        {
            CreateFramebuffer(ImGui.GetContentRegionAvail());
        }
        
        commandList.Begin();
        commandList.SetFramebuffer(framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(1, 0, 1, 1.0f));
        commandList.End();
        Program.gd.SubmitCommands(commandList);
        
        ImGui.Image(viewportId, viewportSize);
    }
}