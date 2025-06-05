using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using IconFonts;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using TinyDialogsNet;
using Object = MiloLib.Assets.Object;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace ImMilo;

using MiloLib.Assets.Ham;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

public static partial class Program
{
    private static Sdl2Window? _window;
    public static GraphicsDevice? gd;
    private static CommandList? _cl;
    public static ImGuiController? controller;

    private static readonly Vector3 ClearColor = new(0.45f, 0.55f, 0.6f);

    private static MiloFile? _currentScene;
    public static MiloFile? currentScene
    {
        get
        {
            return _currentScene;
        }
        private set
        {
            _currentScene = value;
            if (value != null)
            {
                _window.Title = $"ImMilo - {value.filePath}";
            }
            else
            {
                _window.Title = "ImMilo";
            }

        }
    }

    private static Exception? errorModalException;
    private static bool errorModalOpen;
    private static string errorModalMessage = "";

    private static object? viewingObject;
    private static string filter = "";
    private static bool filterActive;
    private static readonly List<object> Breadcrumbs = [];

    private static Settings.Theme currentTheme;

    public static List<TaskCompletionSource> callAfterFrame = new();
    public static List<Action> defferedActions = new();
    public static bool showDemoWindow = false;

    public static readonly FileFilter MiloFileFilter = new FileFilter("Milo Scenes",
    [
        "*.milo_ps2", "*.milo_xbox", "*.milo_ps3", "*.milo_wii", "*.milo_pc", "*.rnd", "*.rnd_ps2",
        "*.rnd_xbox", "*.rnd_gc", "*.kr"
    ]);

    private static bool ShowAboutWindow;
    private static nint? MiloTexture;

    public static bool NoSettingsReload => viewingObject is Settings;
    private static Dictionary<ImGuiMouseCursor, SDL_Cursor> cursorCache = new();

    static void Main(string[] args)
    {
        Settings.Load();
        var graphicsDebug = false;
        var backend = VeldridStartup.GetPlatformDefaultBackend();
        if (Debugger.IsAttached && backend == GraphicsBackend.OpenGL)
        {
            // On my machine, there's a weird bug where running the app with the debugger attached causes
            // CreateWindowAndGraphicsDevice to crash with little to no information.
            graphicsDebug = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }


        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            backend = GraphicsBackend.Vulkan; // I'm just going to force Vulkan on Windows, getting D3D to work with my shaders has bested me.
        }

        Console.WriteLine(backend);
        // Create window, GraphicsDevice, and all resources necessary for the demo.
        Sdl2Native.SDL_SetHint("SDL_HINT_VIDEO_DRIVER", "wayland");
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImMilo"),
            new GraphicsDeviceOptions(graphicsDebug, null, true, ResourceBindingModel.Improved, true, true),
            backend,
            out _window,
            out gd);
        _window.Resized += () =>
        {
            gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            controller.WindowResized(_window.Width, _window.Height);
        };
        _cl = gd.ResourceFactory.CreateCommandList();
        controller = new ImGuiController(gd, gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width,
            _window.Height);
        //ImGui.StyleColorsLight();
        ImGui.GetStyle().ScaleAllSizes(Settings.Loaded.UIScale);
        ImGui.GetStyle().FrameBorderSize = Settings.Loaded.UIScale;
        ImGui.GetStyle().ChildBorderSize = Settings.Loaded.UIScale;
        ImGui.GetIO().FontGlobalScale = Settings.Loaded.UIScale;

        if (ImGuiController.customFontFailed)
        {
            OpenErrorModal(
                new FileNotFoundException("Could not find font file at " +
                                          Settings.Loaded.fontSettings.CustomFontFilePath),
                "Failed to load custom font.");
        }

        _window.DragDrop += evt => { OpenFile(evt.File); };
        _window.KeyDown += evt =>
        {
            if ((evt.Modifiers & ModifierKeys.Control) > 0)
            {
                switch (evt.Key)
                {
                    case Key.N:
                        PromptNew();
                        break;
                    case Key.O:
                        PromptOpen();
                        break;
                    case Key.W:
                        CloseAssetAndScene();
                        break;
                    case Key.S:
                        if (currentScene is null)
                        {
                            break;
                        }
                        if ((evt.Modifiers & ModifierKeys.Shift) > 0)
                        {
                            PromptSaveCurrentScene();
                        }
                        else
                        {
                            SaveCurrentScene();
                        }

                        break;
                    case Key.F:
                        SearchWindow.FocusMainWindow();
                        break;
                }
            }
        };

        var stopwatch = Stopwatch.StartNew();
        // Main application loop
        while (_window.Exists)
        {
            var deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists)
            {
                break;
            }

            controller.Update(deltaTime,
                snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

            MainUI();

            _cl.Begin();
            _cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1f));
            controller.Render(gd, _cl);
            _cl.End();
            gd.SubmitCommands(_cl);
            gd.SwapBuffers(gd.MainSwapchain);

            foreach (var task in callAfterFrame)
            {
                task.SetResult();
            }
            callAfterFrame.Clear();

            foreach (var action in defferedActions)
            {
                action?.Invoke();
            }
            defferedActions.Clear();
        }

        Settings.Save();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform
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

    static unsafe bool IsThemeDark()
    {
        var col = ImGui.GetStyleColorVec4(ImGuiCol.Text);
        ImGui.ColorConvertRGBtoHSV(col->X, col->Y, col->Z, out _, out _, out var value);
        return value > 0.5f;
    }

    // ReSharper disable once InconsistentNaming
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
        ImGui.Begin("ImMilo",
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus);
        MenuBar();
        DrawErrorModal();
        UIContent();
        ProcessPrompts();

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        if (showDemoWindow)
        {
            ImGui.ShowDemoWindow();
        }
        DrawAboutWindow();
        SearchWindow.mainWindow.TargetScene = currentScene;
        SearchWindow.mainWindow.DrawWindow(ref SearchWindow.mainWindowOpen);
        UpdateMouseCursor();
    }

    private static void UpdateMouseCursor()
    {
        var cursor = ImGui.GetMouseCursor();
        if (!cursorCache.TryGetValue(cursor, out var sdlCursor))
        {
            sdlCursor = cursor switch
            {
                ImGuiMouseCursor.Hand => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Hand),
                ImGuiMouseCursor.ResizeAll => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.SizeAll),
                ImGuiMouseCursor.TextInput => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.IBeam),
                ImGuiMouseCursor.ResizeNS => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.SizeNS),
                ImGuiMouseCursor.ResizeEW => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.SizeWE),
                ImGuiMouseCursor.NotAllowed => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.No),
                _ => Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Arrow)
            };
            cursorCache[cursor] = sdlCursor;
        }
        Sdl2Native.SDL_SetCursor(sdlCursor);
    }

    public static void OpenErrorModal(Exception e, string message)
    {
        errorModalException = e;
        errorModalOpen = true;
        errorModalMessage = message;
        if (Debugger.IsAttached)
        {
            throw e;
        }
    }

    static void DrawAboutWindow()
    {
        if (ShowAboutWindow)
        {

            if (MiloTexture is null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var str = assembly.GetManifestResourceStream("milo.png");

                var tex = Util.QuickCreateTexture(str);
                MiloTexture = controller.GetOrCreateImGuiBinding(gd.ResourceFactory, tex);
            }


            ImGui.Begin("About", ref ShowAboutWindow, ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.PushFont(Util.bigFont);
            ImGui.Text("ImMilo");
            ImGui.PopFont();
            ImGui.TextDisabled("An editor for Milo scenes");
            if (ImGui.TextLink("GitHub Repo"))
            {
                Util.OpenBrowser("https://github.com/ihatecompvir/MiloEditor");
            }
            if (ImGui.TextLink("MiloHax"))
            {
                Util.OpenBrowser("https://milohax.org/");
            }
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2f - 64f));
            ImGui.Image(MiloTexture.Value, new Vector2(128, 128));
            ImGui.SeparatorText("Credits");

            // Name, contribution, url
            List<(string, string, string)> credits = new()
            {
                ( "ihatecompvir", "The bulk of the Milo saving and loading logic, original MiloEditor", "https://github.com/ihatecompvir" ),
                ( "Sulfrix", "ImGui user interface", "https://github.com/Sulfrix" ),
                ( "PikminGuts92 (Cisco)", "Creating the 010 Editor templates, Mackiloha, and Grim that have all been a gigantic help in understanding Milo scenes", "https://github.com/PikminGuts92" ),
                ( "RB3 Decomp team and contributors", "Decompiling the game into clean, human readable source code, making it significantly easer to write MiloLib", "https://github.com/DarkRTA/rb3" ),
                ( "Dear ImGui", "The UI library used here", "https://github.com/ocornut/imgui")
            };

            foreach (var credit in credits)
            {
                var (name, contribution, url) = credit;
                if (ImGui.TextLink(name))
                {
                    Util.OpenBrowser(url);
                }

                //ImGui.SameLine();
                ImGui.PushTextWrapPos(256);
                ImGui.Text($"{contribution}");
                ImGui.PopTextWrapPos();
            }

            ImGui.End();
        }
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
            if (errorModalException != null)
            {
                ImGui.Text(errorModalException.GetType().Name);
                ImGui.Text(errorModalException.Message);
                if (errorModalException is not FileNotFoundException)
                {
                    if (ImGui.CollapsingHeader("Callstack"))
                    {
                        ImGui.Text(errorModalException.StackTrace);
                    }
                }
            }
            if (ImGui.Button("OK", new Vector2(ImGui.GetContentRegionAvail().X, 0.0f)))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    static void OpenFile(string path)
    {
        if (Debugger.IsAttached)
        {
            currentScene = new MiloFile(path);
            viewingObject = null;
        }
        else
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

    }

    static async void PromptNew()
    {
        var file = await ShowNewFilePrompt();
        if (file != null)
        {
            CloseAssetAndScene();
            currentScene = file;
        }
    }

    static void MenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New...", "Ctrl+N"))
                {
                    PromptNew();
                }

                if (ImGui.MenuItem("Open...", "Ctrl+O"))
                {
                    PromptOpen();
                }

                if (ImGui.MenuItem("Close", "Ctrl+W", false, viewingObject != null || currentScene != null))
                {
                    CloseAssetAndScene();
                }

                if (ImGui.MenuItem("Save", "Ctrl+S", false, currentScene != null))
                {
                    SaveCurrentScene();
                }

                if (ImGui.MenuItem("Save As...", "Ctrl+Shift+S", false, currentScene != null))
                {
                    PromptSaveCurrentScene();
                }

                if (ImGui.MenuItem("Settings"))
                {
                    NavigateObject(Settings.Editing, false);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Search", "Ctrl+F", SearchWindow.mainWindowOpen))
                {
                    SearchWindow.FocusMainWindow();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Hide Field Descriptions", null, ref Settings.Editing.HideFieldDescriptions);
                ImGui.MenuItem("Hide Nested Hmx::Object Fields", null, ref Settings.Editing.HideNestedHMXObjectFields);
                ImGui.MenuItem("Nerd Names", null, ref Settings.Editing.NerdNames);
                if (ImGui.MenuItem("VSync", null, gd.MainSwapchain.SyncToVerticalBlank))
                {
                    gd.MainSwapchain.SyncToVerticalBlank = !gd.MainSwapchain.SyncToVerticalBlank;
                }

                ImGui.MenuItem("ImGui Demo Window", null, ref showDemoWindow);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Utilities"))
            {
                if (ImGui.MenuItem("Fix Instrument Folder"))
                {
                    CharAssetFixer.PromptInstrumentFix();
                }

                if (ImGui.MenuItem("Fix Character Asset Folder"))
                {
                    CharAssetFixer.PromptCharAssetFix();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("About..."))
                {
                    ShowAboutWindow = true;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }

    private static async void PromptSaveCurrentScene()
    {
        var settings = await ShowSavePrompt(currentScene);
        if (settings == null)
        {
            return;
        }
        var (canceled, path) = TinyDialogs.SaveFileDialog("Save Milo Scene", currentScene.filePath, MiloFileFilter);

        if (!canceled)
        {
            try
            {
                currentScene.endian = settings.endianness;
                currentScene.dirMeta.platform = settings.platform;
                currentScene.Save(path, settings.compressionType, 2064U, Endian.LittleEndian, currentScene.endian);
            }
            catch (Exception e)
            {
                OpenErrorModal(e, "Error occurred while saving file:");
            }
        }
    }

    private static void SaveCurrentScene()
    {
        try
        {
            var compression = currentScene.compressionType;
            if (currentScene.dirMeta.platform == DirectoryMeta.Platform.PS3)
            {
                // Apparently PS3 milos don't save correctly compressed, so we'll do this hack.
                // TODO: Find a better way to do this.
                compression = MiloFile.Type.Uncompressed;
            }
            currentScene.Save(null, compression, 2064U, Endian.LittleEndian, currentScene.endian);
        }
        catch (Exception e)
        {
            OpenErrorModal(e, "Error occurred while saving file:");
        }
    }

    private static void CloseAssetAndScene()
    {
        viewingObject = null;
        currentScene = null;
        BitmapEditor.Dispose();
        SearchWindow.mainWindow.Results.Clear();
    }

    private static void PromptOpen()
    {
        var fileFilter = new FileFilter("Milo Scenes",
        [
            "*.milo_ps2", "*.milo_xbox", "*.milo_ps3", "*.milo_wii", "*.milo_pc", "*.rnd", "*.rnd_ps2",
            "*.rnd_xbox", "*.rnd_gc", "*.kr"
        ]);
        var (canceled, paths) = TinyDialogs.OpenFileDialog("Open Milo Scene", "", false, fileFilter);
        if (!canceled)
        {
            OpenFile(paths.First());
        }
    }

    /// <summary>
    /// Makes the editor pane view the specified object, for editing.
    /// </summary>
    /// <param name="toView">The object to view.</param>
    /// <param name="breadcrumb">Whether or not to lay "breadcrumbs", which allows for easy navigation to the parent object</param>
    public static void NavigateObject(object toView, bool breadcrumb = false)
    {
        if (toView == null)
        {
            return;
        }
        Console.WriteLine("Navigating to " + toView.GetType().Name);
        viewingObject = toView;
        if (breadcrumb)
        {
            if (!Breadcrumbs.Contains(toView))
            {
                Breadcrumbs.Add(toView);
            }
            else
            {
                var index = Breadcrumbs.IndexOf(toView);
                Breadcrumbs.RemoveAll(x => Breadcrumbs.IndexOf(x) > index);
            }
        }
        else
        {
            Breadcrumbs.Clear();
            Breadcrumbs.Add(toView);
        }
    }



    public static void ImportAsset(DirectoryMeta dir, string path, string assetType)
    {
        // read the file into a byte array
        byte[] fileBytes = File.ReadAllBytes(path);

        // create a new entry
        DirectoryMeta.Entry entry = DirectoryMeta.Entry.CreateDirtyAssetFromBytes(assetType,
            Path.GetFileName(path), fileBytes.ToList<byte>());

        // add the entry to the parent dir
        dir.entries.Add(entry);

        // use an EndianReader to read the bytes into an Object
        using (EndianReader reader =
               new EndianReader(new MemoryStream(fileBytes), Endian.BigEndian))
        {
            // read the object
            switch (entry.type.value)
            {
                case "Tex":
                    entry.obj = new RndTex().Read(reader, false, dir, entry);
                    break;
                case "Group":
                    entry.obj = new RndGroup().Read(reader, false, dir, entry);
                    break;
                case "Trans":
                    entry.obj = new RndTrans().Read(reader, false, dir, entry);
                    break;
                case "BandSongPref":
                    entry.obj = new BandSongPref().Read(reader, false, dir, entry);
                    break;
                case "Sfx":
                    entry.obj = new Sfx().Read(reader, false, dir, entry);
                    break;
                case "BandCharDesc":
                    entry.obj = new BandCharDesc().Read(reader, false, dir, entry);
                    break;
                default:
                    Debug.WriteLine("Unknown asset type: " + entry.type.value);
                    entry.obj = new Object().Read(reader, false, dir, entry);
                    break;
            }
        }
    }

    public static DirectoryMeta.Entry DuplicateEntry(DirectoryMeta.Entry entry, DirectoryMeta parent)
    {
        DirectoryMeta.Entry newEntry;

        if (entry.typeRecognized)
        {
            newEntry = new DirectoryMeta.Entry(entry.type, entry.name, entry.obj);

            // Using the same hack as in MiloEditor
            var updatedBytes = entry.objBytes.ToArray().Concat(new byte[] { 0xAD, 0xDE, 0xAD, 0xDE })
                .ToArray();
            using (MemoryStream ms = new MemoryStream(updatedBytes))
            {
                EndianReader reader = new EndianReader(ms, currentScene.endian);
                parent.ReadEntry(reader, entry);
            }
            entry.objBytes = updatedBytes.ToList();
        }
        else
        {
            newEntry = DirectoryMeta.Entry.CreateDirtyAssetFromBytes(entry.type, entry.name,
                entry.objBytes);
        }

        return newEntry;
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
                    DirNode(currentScene.dirMeta, 0, true);
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

            if (Breadcrumbs.Count > 1)
            {
                for (int i = 0; i < Breadcrumbs.Count; i++)
                {
                    var obj = Breadcrumbs[i];
                    if (i < Breadcrumbs.Count - 1)
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

                if(viewingObject is HamMove || viewingObject is MoveGraph) { hasCustomEditor = true; }

                if (hasCustomEditor)
                {
                    if (ImGui.BeginTabBar("Editors")) {

                        if (viewingObject is HamMove move) {
                            if (ImGui.BeginTabItem("Output")) {
                                if (ImGui.BeginChild("ScrollText", new Vector2(0, 1000), ImGuiChildFlags.Borders, ImGuiWindowFlags.AlwaysVerticalScrollbar|ImGuiWindowFlags.AlwaysHorizontalScrollbar)) {

                                    ImGui.TextUnformatted(move.ToString());
                                    ImGui.EndChild();
                                }
                                ImGui.EndTabItem();
                            }
                        }
                        else if(viewingObject is MoveGraph graph) {
                            if (ImGui.BeginTabItem("Output")) {
                                if (ImGui.BeginChild("ScrollText", new Vector2(0, 1000), ImGuiChildFlags.Borders, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar)) {

                                    ImGui.TextUnformatted(graph.ToString());
                                    ImGui.EndChild();
                                }
                                ImGui.EndTabItem();
                            }
                        }

                        if (viewingObject is RndTex tex)
                        {
                            if (ImGui.BeginTabItem(FontAwesome5.Image + "  Texture"))
                            {
                                BitmapEditor.Draw(tex);
                                ImGui.EndTabItem();
                            }
                        }

                        if (viewingObject is RndMesh mesh)
                        {
                            if (ImGui.BeginTabItem(FontAwesome5.Cube + "  Mesh"))
                            {
                                MeshEditor.Draw(mesh);
                                ImGui.EndTabItem();
                            }
                        }

                        if (ImGui.BeginTabItem(FontAwesome5.Table + "  Fields"))
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