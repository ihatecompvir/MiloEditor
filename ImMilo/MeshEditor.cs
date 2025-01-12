using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using Veldrid;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

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
    private static Vector3 centerPos;

    private static Matrix4x4 modelRotation = Matrix4x4.Identity;
    private static Vector3 modelOffset = Vector3.Zero;
    private static float zoom = -40f;
    private static Vector2 prevMousePos;
    
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
            new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(true, true, ComparisonKind.Greater),
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Wireframe, FrontFace.Clockwise, true, false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, [vertexShader, fragmentShader]),
            [layout],
            framebuffer.OutputDescription,
            ResourceBindingModel.Default);
        pipeline = factory.CreateGraphicsPipeline(ref pd);

        mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(layout, matrixBuffer));

    }

    static void UpdateMesh(RndMesh newMesh)
    {
        var gd = Program.gd;
        curMesh = newMesh;
        vertices.Clear();
        faces.Clear();
        modelRotation = Matrix4x4.Identity;
        modelOffset = Vector3.Zero;
        zoom = -40;

        var min = Vector3.Zero;
        var max = Vector3.Zero;
        var i = 0;
        foreach (var vertex in newMesh.vertices.vertices)
        {
            var packedVertex = new PackedVertex(vertex);
            var vecVert = new Vector3(packedVertex.X, packedVertex.Y, packedVertex.Z);
            if (i == 0)
            {
                min = vecVert;
                max = vecVert;
            }
            min = Vector3.Min(min, vecVert);
            max = Vector3.Max(max, vecVert);
            vertices.Add(packedVertex);
            i++;
        }
        Console.WriteLine("Min: " + min + ", Max: " + max);
        centerPos = (min + max) / 2f;
        Console.WriteLine("Center: " + centerPos);
        //var modelPos = centerPos;
        //modelMatrix = Matrix4x4.CreateRotationX(float.DegreesToRadians(90), centerPos);
        modelMatrix = Matrix4x4.CreateTranslation(-centerPos); //negative z forward pls?

        foreach (var face in newMesh.faces)
        {
            faces.Add(new PackedFace(face));
        }

        uint totalVBSize = (uint)(vertices.Count * Unsafe.SizeOf<PackedVertex>());
        if (totalVBSize > vertexBuffer.SizeInBytes)
        {
            gd.DisposeWhenIdle(vertexBuffer);
            vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.Dynamic | BufferUsage.VertexBuffer));
        }
        
        uint totalIBSize = (uint)(faces.Count * Unsafe.SizeOf<PackedFace>());
        if (totalIBSize > indexBuffer.SizeInBytes)
        {
            gd.DisposeWhenIdle(indexBuffer);
            indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.Dynamic | BufferUsage.IndexBuffer));
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
            //projectionMatrix = Matrix4x4.CreateOrthographic(40, 40, 0.1f, 100);
            initialized = true;
        }
        if (commandList == null)
        {
            throw new Exception("Command list is null");
        }
        commandList.Begin();

        if (mesh != curMesh)
        {
            UpdateMesh(mesh);
        }

        var offset = new Vector3(0, 0, zoom);
        
        projectionMatrix = Matrix4x4.CreateLookAt(offset, Vector3.Zero, Vector3.UnitY);
        projectionMatrix *= Matrix4x4.CreatePerspectiveFieldOfView(float.DegreesToRadians(75), viewportSize.X / viewportSize.Y, 0.1f, 10000f);
        modelMatrix = Matrix4x4.CreateTranslation(-centerPos+modelOffset);
        modelMatrix *= modelRotation;


        Program.gd.UpdateBuffer(matrixBuffer, 0, projectionMatrix);
        Program.gd.UpdateBuffer(matrixBuffer, 64, modelMatrix);
        commandList.SetFramebuffer(framebuffer);
        commandList.SetFullViewports();
        commandList.ClearDepthStencil(0);
        commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0.0f));
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, mainResourceSet);
        commandList.DrawIndexed((uint)faces.Count);
        commandList.End();
        Program.gd.SubmitCommands(commandList);
        
        ImGui.Image(viewportId, viewportSize);
        if (ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var delta = (ImGui.GetMousePos() - prevMousePos)*0.02f;
            modelRotation *= Matrix4x4.CreateRotationX(delta.Y);
            modelRotation *= Matrix4x4.CreateRotationY(delta.X);
        } 
        else if (ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            var delta = (ImGui.GetMousePos() - prevMousePos)*0.1f;

            Matrix4x4.Invert(modelRotation, out var inverted);
            
            modelOffset += Vector3.TransformNormal(-Vector3.UnitX*delta.X, inverted);
            modelOffset += Vector3.TransformNormal(Vector3.UnitY*delta.Y, inverted);
        }

        if (ImGui.IsItemHovered())
        {
            zoom += ImGui.GetIO().MouseWheel;
        }

        prevMousePos = ImGui.GetMousePos();
    }
}