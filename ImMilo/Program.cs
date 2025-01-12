using System.Diagnostics;
using System.Numerics;
using System.Text;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloIcons;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using TinyDialogsNet;
using Object = MiloLib.Assets.Object;

namespace ImMilo;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

class Program
{
    
    private static Sdl2Window _window;
    public static GraphicsDevice gd;
    private static CommandList _cl;
    public static ImGuiController controller;
    
    private static Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    private static MiloFile currentScene;

    private static Exception errorModalException;
    private static bool errorModalOpen;
    private static string errorModalMessage;

    private static object viewingObject;
    private static string filter = "";
    private static bool filterActive;
    private static List<object> breadcrumbs = [];
    
    private static Settings.Theme currentTheme;

    public static bool NoSettingsReload => viewingObject is Settings;
    
    static void Main(string[] args)
    {
        Settings.Load();
        // Create window, GraphicsDevice, and all resources necessary for the demo.
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImMilo"),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out _window,
            out gd);
        _window.Resized += () =>
        {
            gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            controller.WindowResized(_window.Width, _window.Height);
        };
        _cl = gd.ResourceFactory.CreateCommandList();
        controller = new ImGuiController(gd, gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        //ImGui.StyleColorsLight();
        ImGui.GetStyle().ScaleAllSizes(Settings.Loaded.UIScale);
        ImGui.GetStyle().FrameBorderSize = Settings.Loaded.UIScale;
        ImGui.GetStyle().ChildBorderSize = Settings.Loaded.UIScale;
        ImGui.GetIO().FontGlobalScale = Settings.Loaded.UIScale;

        if (ImGuiController.customFontFailed)
        {
            OpenErrorModal(new FileNotFoundException("Could not find font file at " + Settings.Loaded.fontSettings.CustomFontFilePath), "Failed to load custom font.");
        }

        _window.DragDrop += (DragDropEvent evt) =>
        {
            OpenFile(evt.File);
        };
        
        var stopwatch = Stopwatch.StartNew();
        float deltaTime;
        // Main application loop
        while (_window.Exists)
        {
            deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { break; }
            controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
            
            MainUI();

            _cl.Begin();
            _cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            controller.Render(gd, _cl);
            _cl.End();
            gd.SubmitCommands(_cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }
        
        Settings.Save();

        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Windows))
        {
            // Linux bodge: Veldrid has some shitty bugs on Linux that cause WaitForIdle to hang. Just exit!
            Environment.Exit(0);
        }
        
        // Clean up Veldrid resources
        gd.WaitForIdle();
        controller.Dispose();
        _cl.Dispose();
        gd.Dispose();
    }

    static void UpdateTheme()
    {
        currentTheme = Settings.Editing.useTheme;
        switch (currentTheme)
        {
            case Settings.Theme.Dark:
                ImGui.StyleColorsDark();
                break;
            case Settings.Theme.Light:
                ImGui.StyleColorsLight();
                break;
            case Settings.Theme.ImGuiClassic:
                ImGui.StyleColorsClassic();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    static void MainUI()
    {
        if (Settings.Editing.useTheme != currentTheme)
        {
            UpdateTheme();
        }
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

    public static void OpenErrorModal(Exception e, string message)
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
            if (errorModalException is not FileNotFoundException)
            {
                if (ImGui.CollapsingHeader("Callstack"))
                {
                    ImGui.Text(errorModalException.StackTrace);
                }
            }
            if (ImGui.Button("OK", new Vector2(ImGui.GetContentRegionAvail().X, 0.0f)))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    static void OpenFile(string path)
    {
        try
        {
            currentScene = new MiloFile(path);
            viewingObject = null;
        }
        catch (Exception e)
        {
            OpenErrorModal(e, "Error occurred while loading file:");
            currentScene = null;
            Console.WriteLine(e.Message);
        }
    }

    static void MenuBar()
    {
        if (ImGui.BeginMainMenuBar()) // TODO: keyboard shortcuts
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open" , "Ctrl+O"))
                {
                    var filter = new FileFilter("Milo Scenes",
                    [
                        "*.milo_ps2", "*.milo_xbox", "*.milo_ps3", "*.milo_wii", "*.milo_pc", "*.rnd", "*.rnd_ps2",
                        "*.rnd_xbox", "*.rnd_gc", "*.kr"
                    ]);
                    var (canceled, paths) = TinyDialogs.OpenFileDialog("Open Milo Scene", "", false, filter);
                    if (!canceled)
                    {
                        OpenFile(paths.First());
                    }
                }

                if (ImGui.MenuItem("Close", "Ctrl+W", false, viewingObject != null))
                {
                    viewingObject = null;
                    currentScene = null;
                    BitmapEditor.Dispose();
                }

                if (ImGui.MenuItem("Save", "Ctrl+S", false, viewingObject != null))
                {
                    try
                    {
                        currentScene.Save(null, null);
                    }
                    catch (Exception e)
                    {
                        OpenErrorModal(e, "Error occurred while saving file:");
                    }
                }

                if (ImGui.MenuItem("Save As...", "Ctrl+Shift+S", false, viewingObject != null))
                {
                    var filter = new FileFilter("Milo Scenes",
                    [
                        "*.milo_ps2", "*.milo_xbox", "*.milo_ps3", "*.milo_wii", "*.milo_pc", "*.rnd", "*.rnd_ps2",
                        "*.rnd_xbox", "*.rnd_gc", "*.kr"
                    ]);
                    var (canceled, path) = TinyDialogs.SaveFileDialog("Save Milo Scene", currentScene.filePath, filter);

                    if (!canceled)
                    {
                        try
                        {
                            currentScene.Save(path, null);
                        }
                        catch (Exception e)
                        {
                            OpenErrorModal(e, "Error occurred while saving file:");
                        }
                    }
                }

                if (ImGui.MenuItem("Settings"))
                {
                    NavigateObject(Settings.Editing, false);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Hide Field Descriptions", null, ref Settings.Editing.HideFieldDescriptions);
                ImGui.MenuItem("Hide Nested Hmx::Object Fields", null, ref Settings.Editing.HideNestedHMXObjectFields);
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    /// <summary>
    /// Makes the editor pane view the specified object, for editing.
    /// </summary>
    /// <param name="toView">The object to view.</param>
    /// <param name="breadcrumb">Whether or not to lay "breadcrumbs", which allows for easy navigation to the parent object</param>
    public static void NavigateObject(object toView, bool breadcrumb = false)
    {
        Console.WriteLine("Navigating to " + toView.GetType().Name);
        viewingObject = toView;
        if (breadcrumb)
        {
            if (!breadcrumbs.Contains(toView))
            {
                breadcrumbs.Add(toView);
            }
            else
            {
                var index = breadcrumbs.IndexOf(toView);
                breadcrumbs.RemoveAll(x => breadcrumbs.IndexOf(x) > index);
            }
        }
        else
        {
            breadcrumbs.Clear();
            breadcrumbs.Add(toView);
        }
    }

    static void DirNode(DirectoryMeta dir, int id = 0, bool root = false)
    {
        var filterActive = filter != "";
        ImGui.PushID(id);
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick
            | ImGuiTreeNodeFlags.SpanFullWidth;
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

        var cursor = ImGui.GetCursorScreenPos();
        var iconSize = Settings.Loaded.ScaledIconSize;
        var framePadding = ImGui.GetStyle().FramePadding;
        var lineX = cursor.X+iconSize/2f+framePadding.X;
        var lineY = cursor.Y+ImGui.GetFrameHeight()-ImGui.GetStyle().ItemSpacing.Y-framePadding.Y;
        var drawList = ImGui.GetWindowDrawList();
        var treeLineColor = ImGui.GetColorU32(ImGuiCol.Text);
        treeLineColor = (0x00ffffff & treeLineColor) | 0x80000000;
        var drawChildLine = (bool dir) =>
        {
            var cursor = ImGui.GetCursorScreenPos();
            var lineEndX = cursor.X + framePadding.X;
            if (!dir)
            {
                lineEndX += iconSize;
            }
            drawList.AddLine(new Vector2(lineX+1, cursor.Y+iconSize/2f), new Vector2(lineEndX, cursor.Y+iconSize/2f), treeLineColor);
        };
        var childrenDrawn = 0;
        if (Util.SceneTreeItem(dir, flags))
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
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !ImGui.IsItemToggledOpen())
                NavigateObject(dir.directory);
            var i = 0;
            //ImGui.Indent();
            if (dir.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                i = 0;
                foreach (var subDir in objDir.inlineSubDirs)
                {
                    drawChildLine(true);
                    DirNode(subDir, i);
                    childrenDrawn++;
                    i++;
                }
            }

            foreach (var entry in dir.entries)
            {
                var matchesFilter = filterActive && entry.name.ToString().ToLower().Contains(filter.ToLower());
                i++;
                if (matchesFilter)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 0f, 1f, 1f));
                }
                if (entry.dir != null)
                {
                    //ImGui.Button("Test");
                    //ImGui.SameLine();

                    drawChildLine(true);
                    DirNode(entry.dir, i);
                    childrenDrawn++;
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
                    ImGuiTreeNodeFlags leafFlags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth;
                    if (viewingObject != null && viewingObject == entry.obj)
                    {
                        leafFlags |= ImGuiTreeNodeFlags.Selected;
                    }

                    drawChildLine(false);
                    Util.SceneTreeItem(entry, leafFlags);
                    childrenDrawn++;
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && entry.obj != null)
                    {
                        NavigateObject(entry.obj);
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
                    ImGui.TreePop();
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
            if (childrenDrawn > 0)
            {
                drawList.AddLine(new Vector2(lineX, lineY), new Vector2(lineX, ImGui.GetCursorScreenPos().Y-ImGui.GetFrameHeight()/2f), treeLineColor);
            }
            //ImGui.Unindent();
        }
        else
        {
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                NavigateObject(dir.directory);
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
                    ImGui.PushFont(Util.iconFont);
                    DirNode(currentScene.dirMeta,  0, true);
                    ImGui.PopFont();
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

            if (breadcrumbs.Count > 1)
            {
                for (int i = 0; i < breadcrumbs.Count; i++)
                {
                    var obj = breadcrumbs[i];
                    if (i < breadcrumbs.Count - 1)
                    {
                        if (ImGui.Button(obj.ToString()))
                        {
                            NavigateObject(obj, true);
                        }
                        ImGui.SameLine();
                        ImGui.Text(">");
                        ImGui.SameLine();
                    }
                    else
                    {
                        ImGui.Text(obj.ToString());
                    }
                }
            }
            
            if (viewingObject != null)
            {
                bool hasCustomEditor = false;
                if (viewingObject is RndTex)
                {
                    hasCustomEditor = true;
                }
                if (viewingObject is RndMesh)
                {
                    hasCustomEditor = true;
                }
                if (hasCustomEditor)
                {
                    if (ImGui.BeginTabBar("Editors"))
                    {
                        if (viewingObject is RndTex tex)
                        {
                            if (ImGui.BeginTabItem("Texture"))
                            {
                                BitmapEditor.Draw(tex);
                                ImGui.EndTabItem();
                            }
                        }
                        
                        if (ImGui.BeginTabItem("Fields"))
                        {
                            EditorPanel.Draw(viewingObject);
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                }
                else
                {
                    EditorPanel.Draw(viewingObject);
                }
                
            }
            ImGui.EndChild();
            ImGui.EndGroup();
        }
    }
}