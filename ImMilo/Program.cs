using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using IconFonts;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloIcons;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using MiloLib.Utils.Conversion;
using TinyDialogsNet;
using Object = MiloLib.Assets.Object;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

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
    private static string modalString;

    /// <summary>
    /// temporary state
    /// </summary>
    private static object modalObject;

    private static object modalObject2;

    public static bool NoSettingsReload => viewingObject is Settings;

    static void Main(string[] args)
    {
        Console.WriteLine(VeldridStartup.GetPlatformDefaultBackend());
        Settings.Load();
        var graphicsDebug = true;
        if (System.Diagnostics.Debugger.IsAttached)
        {
            // On my machine, there's a weird bug where running the app with the debugger attached causes
            // CreateWindowAndGraphicsDevice to crash with little to no information.
            graphicsDebug = !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
        // Create window, GraphicsDevice, and all resources necessary for the demo.
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImMilo"),
            new GraphicsDeviceOptions(graphicsDebug, null, true, ResourceBindingModel.Improved, true, true),
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

        _window.DragDrop += (DragDropEvent evt) => { OpenFile(evt.File); };

        var stopwatch = Stopwatch.StartNew();
        float deltaTime;
        // Main application loop
        while (_window.Exists)
        {
            deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
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

    static unsafe bool IsThemeDark()
    {
        var col = ImGui.GetStyleColorVec4(ImGuiCol.Text);
        ImGui.ColorConvertRGBtoHSV(col->X, col->Y, col->Z, out _, out _, out var value);
        return value > 0.5f;
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
        ImGui.Begin("ImMilo",
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus);
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
                if (ImGui.MenuItem("Open", "Ctrl+O"))
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
        var iterId = 0;
        DirNode(dir, ref iterId, id, root);
    }

    static void DirNode(DirectoryMeta dir, ref int iterId, int id = 0, bool root = false, DirectoryMeta parent = null,
        bool inlined = false, DirectoryMeta.Entry? thisEntry = null, bool useEntryContextMenu = false)
    {
        if (Settings.Editing.compactScreneTree)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 0f));
        }
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
        var lineX = cursor.X + iconSize / 2f + framePadding.X;
        var lineY = cursor.Y + ImGui.GetFrameHeight() - ImGui.GetStyle().ItemSpacing.Y - framePadding.Y;
        var drawList = ImGui.GetWindowDrawList();
        var treeLineColor = ImGui.GetColorU32(ImGuiCol.Text);
        treeLineColor = (0x00ffffff & treeLineColor) | 0x80000000;

        void DrawChildLine(bool dir)
        {
            var cursor = ImGui.GetCursorScreenPos();
            var lineEndX = cursor.X + framePadding.X;
            if (!dir)
            {
                lineEndX += iconSize;
            }

            var yOffset = iconSize / 2f;
            drawList.AddLine(new Vector2(lineX + 1, cursor.Y + yOffset), new Vector2(lineEndX, cursor.Y + yOffset),
                treeLineColor);
        }

        void DirectoryContextMenu(ref int iterId)
        {
            if (useEntryContextMenu)
            {
                ItemContextMenu(thisEntry, ref iterId);
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().FramePadding);
            if (ImGui.BeginPopupContextItem())
            {
                ImGui.PushFont(Util.mainFont);
                if (parent != null)
                {
                    if (ImGui.MenuItem(FontAwesome5.TrashAlt + "  Delete Directory"))
                    {
                        if (viewingObject == dir.directory)
                        {
                            viewingObject = null;
                        }

                        if (inlined)
                        {
                            var objDir = (ObjectDir)parent.directory;
                            objDir.inlineSubDirs.Remove(dir);
                        }
                        else
                        {
                            parent.entries.Remove(thisEntry);
                        }

                        iterId--;
                    }

                    ImGui.MenuItem(FontAwesome5.Clone + "  Duplicate Directory");
                }

                ImGui.MenuItem(FontAwesome5.Edit + "  Rename Directory");
                ImGui.MenuItem(FontAwesome5.CodeMerge + "  Merge Directory");
                ImGui.MenuItem(FontAwesome5.Share + "  Export Directory");
                ImGui.MenuItem(FontAwesome5.PlusSquare + "  Add Inlined Directory");
                ImGui.Separator();
                if (ImGui.MenuItem(FontAwesome5.FileImport + "  Import Asset"))
                {
                    var (canceled, paths) = TinyDialogs.OpenFileDialog("Import Asset", "", false);
                    if (!canceled)
                    {
                        var path = paths.First();
                        // special handling of certain asset types

                        // detect .prefab file
                        if (Path.GetExtension(path) == ".prefab")
                        {
                            // read file
                            BandCharDesc desc = NautilusInterop.ToBandCharDesc(File.ReadAllText(path));
                            // add the BandCharDesc to the MiloFile
                            currentScene.dirMeta.entries.Add(new DirectoryMeta.Entry("BandCharDesc",
                                "prefab_" + Path.GetFileNameWithoutExtension(path), desc));
                        }
                        else
                        {
                            modalObject = path;
                            modalObject2 = 0;
                            modalString = "\uf000Import Asset" + id;
                        }
                    }
                }

                ImGui.MenuItem(FontAwesome5.PlusCircle + "  New Asset");
                ImGui.PopFont();
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();
        }

        void ItemContextMenu(DirectoryMeta.Entry entry, ref int curIndex)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().FramePadding);
            if (ImGui.BeginPopupContextItem())
            {
                ImGui.PushFont(Util.mainFont);
                if (ImGui.MenuItem(FontAwesome5.TrashAlt + "  Delete Asset"))
                {
                    if (viewingObject == entry.obj)
                    {
                        viewingObject = null;
                    }

                    dir.entries.Remove(entry);
                    curIndex--;
                }

                if (ImGui.MenuItem(FontAwesome5.Clone + "  Duplicate Asset"))
                {
                    modalObject = entry;
                    modalString = "\uf000Duplicate" + id; // Hack to get around the lack of global ids
                }

                if (ImGui.MenuItem(FontAwesome5.Edit + "  Rename Asset"))
                {
                    modalObject = entry;
                    modalString = "\uf000Rename" + id;
                }

                ImGui.Separator();
                if (ImGui.MenuItem(FontAwesome5.Share + "  Extract Asset"))
                {
                    var (canceled, path) = TinyDialogs.SaveFileDialog("Extract Asset", entry.name);

                    if (!canceled)
                    {
                        File.WriteAllBytes(path, entry.objBytes.ToArray());
                    }
                }

                if (ImGui.MenuItem(FontAwesome5.Recycle + "  Replace Asset"))
                {
                    var (canceled, paths) = TinyDialogs.OpenFileDialog("Replace Asset", "", false);
                    if (!canceled)
                    {
                        var backupSuccess = false;
                        var backupStream = new MemoryStream();
                        try
                        {
                            
                            // Backup the asset in case of an error while reading
                            entry.obj.GetType().GetMethod("Write").Invoke(entry.obj,
                                [new EndianWriter(backupStream, currentScene.endian), false, dir, entry]);
                            backupSuccess = true;
                            var path = paths.First();

                            byte[] fileBytes = File.ReadAllBytes(path);
                            entry.objBytes = fileBytes.ToList();
                            // Use reflection to call the read method as the comment below only calls the Object's Read()
                            entry.obj.GetType().GetMethod("Read").Invoke(entry.obj, [new EndianReader(new MemoryStream(fileBytes), currentScene.endian), false, dir, entry]);
                            //entry.obj.Read(new EndianReader(new MemoryStream(fileBytes), currentScene.endian), false, dir, entry);
                        }
                        catch (Exception e)
                        {
                            OpenErrorModal(e, "Cannot replace asset.");
                            if (backupSuccess)
                            {
                                backupStream.Seek(0, SeekOrigin.Begin);
                                entry.obj.GetType().GetMethod("Read").Invoke(entry.obj, [new EndianReader(backupStream, currentScene.endian), false, dir, entry]);
                            }
                        }
                    }
                }
                ImGui.PopFont();
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();
        }

        if (modalString == "\uf000Duplicate" + id)
        {
            modalString = ((DirectoryMeta.Entry)modalObject).name;
            ImGui.OpenPopup("Duplicate##" + id);
        }

        if (modalString == "\uf000Rename" + id)
        {
            modalString = ((DirectoryMeta.Entry)modalObject).name;
            ImGui.OpenPopup("Rename##" + id);
        }

        if (modalString == "\uf000Import Asset" + id)
        {
            modalString = (string)modalObject;
            Console.WriteLine("Import Asset##" + id);
            ImGui.OpenPopup("Import Asset##" + id);
        }

        ImGui.PushFont(Util.mainFont);
        if (ImGui.BeginPopupModal("Duplicate##" + id, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var entry = (DirectoryMeta.Entry)modalObject;
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            var enter = ImGui.InputText("New Asset Name", ref modalString, 128, ImGuiInputTextFlags.EnterReturnsTrue);
            if (ImGui.Button("OK") || enter)
            {
                try
                {
                    var newEntry = DuplicateEntry(entry, parent);

                    newEntry.name = modalString;
                    dir.entries.Add(newEntry);
                    ImGui.CloseCurrentPopup();
                    modalObject = null;
                }
                catch (Exception e)
                {
                    OpenErrorModal(e, "Failed to duplicate asset.");
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                modalObject = null;
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopupModal("Rename##" + id, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var entry = (DirectoryMeta.Entry)modalObject;
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            var enter = ImGui.InputText("New Asset Name", ref modalString, 128, ImGuiInputTextFlags.EnterReturnsTrue);
            if (ImGui.Button("OK") || enter)
            {
                entry.name = modalString;
                if (entry.dir != null)
                {
                    entry.dir.name = modalString;
                }

                ImGui.CloseCurrentPopup();
                modalObject = null;
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                modalObject = null;
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopupModal("Import Asset##" + id, ImGuiWindowFlags.AlwaysAutoResize))
        {
            string[] assetTypes = ["Object", "Tex", "Group", "Trans", "BandSongPref", "Sfx", "BandCharDesc"];
            

            var curType = (int)modalObject2;
            
            ImGui.Combo("Asset type", ref curType, assetTypes, assetTypes.Length);

            modalObject2 = curType;

            if (ImGui.Button("OK"))
            {
                ImportAsset(dir, modalString, assetTypes[curType]);
                ImGui.CloseCurrentPopup();
                modalObject = null;
                modalObject2 = null;
            }

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                modalObject = null;
                modalObject2 = null;
            }
            ImGui.EndPopup();
        }
        ImGui.PopFont();

        var childrenDrawn = 0;
        if (Util.SceneTreeItem(dir, flags))
        {
            DirectoryContextMenu(ref iterId);
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

            if (ImGui.IsItemActivated() && !ImGui.IsItemToggledOpen())
                NavigateObject(dir.directory);
            var nodeId = 200;
            //ImGui.Indent();
            if (dir.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                DrawChildLine(true);
                if (Util.IconTreeItem("ObjectDir", "Inline Subdirectories", ImGuiTreeNodeFlags.SpanFullWidth))
                {
                    for (int subDirIndex = 0; subDirIndex < objDir.inlineSubDirs.Count; subDirIndex++)
                    {
                        var subDir = objDir.inlineSubDirs[subDirIndex];
                        DrawChildLine(true); // TODO: fix the tree traces for the inline subdirs node
                        DirNode(subDir, ref subDirIndex, nodeId, false, dir, true);
                        childrenDrawn++;
                        nodeId++;
                    }

                    ImGui.TreePop();
                }
            }

            nodeId = 1000; // Count of inlined dirs was affecting future node ids, just set it to 1000 and hope something doesn't have >1000 inlined subdirs

            for (int entryIndex = 0; entryIndex < dir.entries.Count; entryIndex++)
            {
                var entry = dir.entries[entryIndex];
                var matchesFilter = filterActive && entry.name.ToString().ToLower().Contains(filter.ToLower());
                nodeId++;
                if (matchesFilter)
                {
                    if (IsThemeDark())
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 0f, 1f));
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 0f, 0.7f, 1f));
                    }
                    
                }

                if (entry.dir != null)
                {
                    //ImGui.Button("Test");
                    //ImGui.SameLine();

                    DrawChildLine(true);
                    DirNode(entry.dir, ref entryIndex, nodeId, false, dir, false, entry, false);
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

                    DrawChildLine(false);
                    Util.SceneTreeItem(entry, leafFlags);
                    ItemContextMenu(entry, ref entryIndex);
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
                                ImGui.SetDragDropPayload("TreeEntryObject", (IntPtr)(payloadPtr),
                                    (uint)payloadBytes.Length);
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
                var stylePtr = ImGui.GetStyle();
                drawList.AddLine(new Vector2(lineX, lineY),
                    new Vector2(lineX, (ImGui.GetCursorScreenPos().Y - stylePtr.ItemSpacing.Y) - ImGui.GetFrameHeight()/2f + stylePtr.FramePadding.Y + 1f), treeLineColor);
            }
            //ImGui.Unindent();
        }
        else
        {
            DirectoryContextMenu(ref iterId);
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                NavigateObject(dir.directory);
        }

        ImGui.PopID();
        if (Settings.Editing.compactScreneTree)
        {
            ImGui.PopStyleVar();
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