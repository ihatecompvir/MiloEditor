using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloLib.Assets.Rnd;
using Veldrid;

namespace ImMilo;

public static class MeshEditor
{
    private static RndMesh curMesh;
    private static Vector2 viewportSize;
    private static bool initialized;
    private static Framebuffer? framebuffer;
    private static Texture? colorBuffer;
    private static Texture? depthBuffer;
    private static nint viewportId;
    private static CommandList? commandList;
    private static Matrix4x4 modelMatrix = Matrix4x4.Identity;
    private static Matrix4x4 projectionMatrix = Matrix4x4.Identity;
    
    private static DeviceBuffer? vertexBuffer;
    private static DeviceBuffer? indexBuffer;
    private static DeviceBuffer? matrixBuffer;
    private static Pipeline? pipeline;

    private static Shader? fragmentShader;
    private static Shader? vertexShader;

    private static ResourceLayout? layout;
    private static ResourceSet? mainResourceSet;

    private static List<PackedVertex> vertices = new();
    private static List<PackedFace> faces = new();
    
    public struct PackedVertex
    {
        public float X, Y, Z;

        public PackedVertex(Vertex vertex)
        {
            X = vertex.x;
            Y = vertex.y;
            Z = vertex.z;
        }
    }

    public struct PackedFace(RndMesh.Face face)
    {
        public ushort idx1 = face.idx1, idx2 = face.idx2, idx3 = face.idx3;
    }

    static void CreateDeviceResources()
    {
        var gd = Program.gd;
        var controller = Program.controller;
        ResourceFactory factory = gd.ResourceFactory;

        vertexBuffer =
            factory.CreateBuffer(new BufferDescription(100000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        vertexBuffer.Name = "Mesh Preview Vertex Buffer";
        indexBuffer = 
            factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        indexBuffer.Name = "Mesh Preview Index Buffer";

        matrixBuffer =
            factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        matrixBuffer.Name = "Mesh Preview Matrix Buffer (Projection and Rotation)";

        byte[] vertexShaderBytes =
            controller.LoadEmbeddedShaderCode(factory, "meshpreview-vertex", ShaderStages.Vertex);
        byte[] fragmentShaderBytes = 
            controller.LoadEmbeddedShaderCode(factory, "meshpreview-frag", ShaderStages.Fragment);
        vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes,
            gd.BackendType == GraphicsBackend.Metal ? "VS" : "main"));
        fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes,
            gd.BackendType == GraphicsBackend.Metal ? "FS" : "main"));

        VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
        {
            new VertexLayoutDescription(new VertexElementDescription("in_position", VertexElementSemantic.Position,
                VertexElementFormat.Float3))
        };

        layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(true, true, ComparisonKind.Greater),
            new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, new [] {vertexShader, fragmentShader}),
                new ResourceLayout[] {layout},
            framebuffer.OutputDescription,
            ResourceBindingModel.Default);
        pipeline = factory.CreateGraphicsPipeline(pd);

        mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(layout, matrixBuffer));

    }

    static void UpdateMesh(RndMesh newMesh)
    {
        curMesh = newMesh;
        modelMatrix = Matrix4x4.CreateTranslation(0, 0, -30f); //negative z forward pls?
        vertices.Clear();
        faces.Clear();
        
        foreach (var vertex in newMesh.vertices.vertices)
        {
            vertices.Add(new PackedVertex(vertex));
        }

        foreach (var face in newMesh.faces)
        {
            faces.Add(new PackedFace(face));
        }
        
        commandList.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());
        commandList.UpdateBuffer(indexBuffer, 0, faces.ToArray());
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
        if (ImGui.GetContentRegionAvail() != viewportSize)
        {
            CreateFramebuffer(ImGui.GetContentRegionAvail());
        }
        
        if (!initialized)
        {
            CreateDeviceResources();
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(float.DegreesToRadians(75), viewportSize.X / viewportSize.Y, 0.1f, 1000f);
            initialized = true;
        }
        
        if (mesh != curMesh)
        {
            UpdateMesh(mesh);
        }

        if (commandList == null)
        {
            throw new Exception("Command list is null");
        }
        
        commandList.Begin();
        Program.gd.UpdateBuffer(matrixBuffer, 0, new [] { projectionMatrix, modelMatrix });
        commandList.SetFramebuffer(framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(1, 0, 1, 1.0f));
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, mainResourceSet);
        commandList.DrawIndexed((uint)faces.Count);
        commandList.End();
        Program.gd.SubmitCommands(commandList);
        
        ImGui.Image(viewportId, viewportSize);
    }
}