namespace ImMilo;

using System.Text;
using IconFonts;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloLib.Assets;
using MiloLib.Assets.Band;
using MiloLib.Utils;
using MiloLib.Utils.Conversion;
using TinyDialogsNet;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;


public partial class Program
{
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
                    var newEntry = DuplicateEntry(entry, dir);

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
            ImGui.SameLine();

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
}