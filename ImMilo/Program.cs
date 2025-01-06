using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using ImMilo.imgui;
using MiloLib;
using MiloLib.Assets;
using TinyDialogsNet;

namespace ImMilo;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

class Program
{
    
    private static Sdl2Window _window;
    private static GraphicsDevice _gd;
    private static CommandList _cl;
    private static ImGuiController _controller;
    
    private static Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    private static MiloFile currentScene;

    private static Exception errorModalException;
    private static bool errorModalOpen;
    
    static void Main(string[] args)
    {
        // Create window, GraphicsDevice, and all resources necessary for the demo.
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImMilo"),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out _window,
            out _gd);
        _window.Resized += () =>
        {
            _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            _controller.WindowResized(_window.Width, _window.Height);
        };
        _cl = _gd.ResourceFactory.CreateCommandList();
        _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        
        var stopwatch = Stopwatch.StartNew();
        float deltaTime;
        // Main application loop
        while (_window.Exists)
        {
            deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { break; }
            _controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

            MainUI();

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

        // Clean up Veldrid resources
        _gd.WaitForIdle();
        _controller.Dispose();
        _cl.Dispose();
        _gd.Dispose();
    }


    static void MainUI()
    {
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.Begin("ImMilo", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus);
        MenuBar();
        UIContent();
        DrawErrorModal();
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.ShowDemoWindow();
    }

    static void OpenErrorModal(Exception e)
    {
        errorModalException = e;
        ImGui.OpenPopup("Error");
    }

    static void DrawErrorModal()
    {
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        if (ImGui.BeginPopup("Error", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("An error has occurred.");
            ImGui.Text(errorModalException.GetType().Name);
            ImGui.Text(errorModalException.Message);
            ImGui.EndPopup();
        }
    }

    static void MenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open"))
                {
                    var filter = new FileFilter("Milo Scenes",
                    [
                        "*.milo_ps2", "*.milo_xbox", "*.milo_ps3", "*.milo_wii", "*.milo_pc", "*.rnd", "*.rnd_ps2",
                        "*.rnd_xbox", "*.rnd_gc", "*.kr"
                    ]);
                    var (canceled, paths) = TinyDialogs.OpenFileDialog("Open Milo Scene", "", false, filter);
                    if (!canceled)
                    {
                        try
                        {
                            currentScene = new MiloFile(paths.First());
                        }
                        catch (Exception e)
                        {
                            OpenErrorModal(e);
                            currentScene = null;
                        }
                        
                    }
                }
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    static void DirNode(DirectoryMeta dir, int id = 0, bool root = false)
    {
        ImGui.PushID(id);
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                    ImGuiTreeNodeFlags.SpanAvailWidth;
        if (root)
        {
            flags |= ImGuiTreeNodeFlags.DefaultOpen;
        }
        if (ImGui.TreeNodeEx(dir.name, flags))
        {
            var i = 0;
            //ImGui.Indent();
            if (dir.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                i = 0;
                foreach (var subDir in objDir.inlineSubDirs)
                {
                    DirNode(subDir, i);
                    i++;
                }
            }

            foreach (var entry in dir.entries)
            {
                if (entry.dir != null)
                {
                    //ImGui.Button("Test");
                    //ImGui.SameLine();
                    
                    DirNode(entry.dir, i);
                }
                else
                {
                    ImGui.Selectable(entry.name);
                }

                i++;
            }
            ImGui.TreePop();
            //ImGui.Unindent();
        }
        ImGui.PopID();
    }

    static void UIContent()
    {
        {
            ImGui.BeginChild("left pane", new Vector2(150, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
            if (currentScene != null)
            {
                if (currentScene.dirMeta != null)
                {
                    DirNode(currentScene.dirMeta,  0, true);
                }
                
            }
            ImGui.EndChild();
        }
        ImGui.SameLine();
        {
            ImGui.BeginGroup();
            ImGui.BeginChild("right pane", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()));
            ImGui.Text("hello");
            ImGui.Indent();
            ImGui.Text("yep");
            ImGui.Unindent();
            ImGui.EndChild();
            ImGui.EndGroup();
        }
    }
}