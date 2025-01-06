using System.Diagnostics;
using System.Numerics;
using System.Text;
using ImGuiNET;
using ImMilo.imgui;
using MiloLib;
using MiloLib.Assets;
using TinyDialogsNet;
using Object = MiloLib.Assets.Object;

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
    private static string errorModalMessage;

    private static Object viewingObject;
    private static string filter = "";
    private static bool filterActive;
    
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
        DrawErrorModal();
        UIContent();
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.ShowDemoWindow();
    }

    static void OpenErrorModal(Exception e, string message)
    {
        errorModalException = e;
        errorModalOpen = true;
        errorModalMessage = message;
    }

    static void DrawErrorModal()
    {
        if (errorModalOpen)
        {
            ImGui.OpenPopup("Error");
            errorModalOpen = false;
        }
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        if (ImGui.BeginPopupModal("Error", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text(errorModalMessage);
            ImGui.Text(errorModalException.GetType().Name);
            ImGui.Text(errorModalException.Message);
            if (ImGui.CollapsingHeader("Callstack"))
            {
                ImGui.Text(errorModalException.StackTrace);
            }

            if (ImGui.Button("OK", new Vector2(ImGui.GetContentRegionAvail().X, 0.0f)))
                ImGui.CloseCurrentPopup();
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
                            viewingObject = null;
                        }
                        catch (Exception e)
                        {
                            OpenErrorModal(e, "Error occurred while loading file:");
                            currentScene = null;
                            Console.WriteLine(e.Message);
                        }
                        
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Hide Field Descriptions", null, ref EditorPanel.HideFieldDescriptions);
                ImGui.MenuItem("Hide Nested Hmx::Object Fields", null, ref EditorPanel.HideNestedHMXObjectFields);
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    static void DirNode(DirectoryMeta dir, int id = 0, bool root = false)
    {
        var filterActive = filter != "";
        ImGui.PushID(id);
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                    ImGuiTreeNodeFlags.SpanAvailWidth;
        if (root || filterActive)
        {
            flags |= ImGuiTreeNodeFlags.DefaultOpen;
        }

        if (filterActive)
        {
            ImGui.SetNextItemStorageID(ImGui.GetID("filtering"));
        }

        if (viewingObject == dir.directory)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }
        if (ImGui.TreeNodeEx(dir.name, flags))
        {
            unsafe
            {
                if (ImGui.BeginDragDropSource())
                {
                    var payload = dir.name.ToString();
                    var payloadBytes = Encoding.UTF8.GetBytes(payload);
                    fixed (byte* payloadPtr = payloadBytes)
                    {
                        ImGui.SetDragDropPayload("TreeEntryDir", (IntPtr)(payloadPtr), (uint)payloadBytes.Length);
                        
                        ImGui.Text(payload);
                        ImGui.EndDragDropSource();
                    }
                        
                }
            }
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                viewingObject = dir.directory;
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
                var matchesFilter = filterActive && entry.name.ToString().ToLower().Contains(filter.ToLower());
                i++;
                if (matchesFilter)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 0f, 1f));
                }
                if (entry.dir != null)
                {
                    //ImGui.Button("Test");
                    //ImGui.SameLine();
                    
                    DirNode(entry.dir, i);
                }
                else
                {
                    if (filterActive && !matchesFilter)
                    {
                        continue;
                    }

                    if (entry.obj == null)
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    }
                    if (ImGui.Selectable(entry.name, viewingObject != null && viewingObject == entry.obj) && entry.obj != null)
                    {
                        viewingObject = entry.obj;
                    }
                    unsafe
                    {
                        if (ImGui.BeginDragDropSource())
                        {
                            var payload = entry.name.ToString();
                            var payloadBytes = Encoding.UTF8.GetBytes(payload);
                            fixed (byte* payloadPtr = payloadBytes)
                            {
                                ImGui.SetDragDropPayload("TreeEntryObject", (IntPtr)(payloadPtr), (uint)payloadBytes.Length);
                            }

                            ImGui.Text(payload);
                            ImGui.EndDragDropSource();

                        }
                    }
                    if (entry.obj == null)
                    {
                        ImGui.PopStyleVar();
                    }
                }
                if (matchesFilter)
                {
                    ImGui.PopStyleColor();
                }
            }
            ImGui.TreePop();
            //ImGui.Unindent();
        }
        else
        {
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                viewingObject = dir.directory;
        }
        ImGui.PopID();
    }

    static void UIContent()
    {
        {
            ImGui.BeginGroup();
            if (currentScene != null)
            {
                //ImGui.Text(currentScene.dirMeta.entries.Count + " entries");
            }
            ImGui.BeginChild("left pane", new Vector2(150, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
            ImGui.InputText("Filter", ref filter, 64);
            ImGui.BeginChild("entries");
            filterActive = filter == "";
            if (currentScene != null)
            {
                if (currentScene.dirMeta != null)
                {
                    DirNode(currentScene.dirMeta,  0, true);
                }
                
            }
            ImGui.EndChild();
            ImGui.EndChild();
            ImGui.EndGroup();
        }
        ImGui.SameLine();
        {
            ImGui.BeginGroup();
            ImGui.BeginChild("right pane", new Vector2(0, ImGui.GetContentRegionAvail().Y));

            if (viewingObject != null)
            {
                EditorPanel.Draw(viewingObject);
            }
            ImGui.EndChild();
            ImGui.EndGroup();
        }
    }
}