using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using MiloLib.Utils.Conversion;
using MiloIcons;
using System.Collections.Immutable;
using System.Diagnostics;
using System.DirectoryServices;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml.Linq;
using static MiloLib.Assets.DirectoryMeta;
using Object = MiloLib.Assets.Object;

namespace MiloEditor
{
    public partial class MainForm : Form
    {
        private MiloFile currentMiloScene;

        private ImageList imageList = new ImageList();

        public MainForm()
        {
            InitializeComponent();

            LoadAssetClassImages();

            return;
        }

        private int GetImageIndex(ImageList imageList, string key)
        {
            if (imageList.Images.ContainsKey(key))
            {
                return imageList.Images.IndexOfKey(key);
            }
            else
            {
                // Default to a placeholder index or -1 if not found
                return 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Milo Editor (" + Application.ProductVersion + ")";

            filterComboBox.SelectedIndex = 0;

            // create handler for when the filter combobox changes
            filterComboBox.SelectedIndexChanged -= FilterComboBox_SelectedIndexChanged;
            filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private Image GetMiloIconImage(string assetType)
        {
            return Image.FromStream(Icons.GetMiloIconStream(Icons.MapTypeName(assetType)));
        }

        private void LoadAssetClassImages()
        {
            imageList.ImageSize = new Size(26, 26);
            imageList.Images.Add("default", Image.FromFile("Images/default.png"));
            imageList.Images.Add("ObjectDir", Image.FromFile("Images/ObjectDir.png"));
            imageList.Images.Add("RndDir", Image.FromFile("Images/RndDir.png"));
            imageList.Images.Add("Object", Image.FromFile("Images/default.png"));
            imageList.Images.Add("BandSongPref", Image.FromFile("Images/BandSongPref.png"));
            imageList.Images.Add("Tex", Image.FromFile("Images/RndTex.png"));
            imageList.Images.Add("TexRenderer", Image.FromFile("Images/RndTex.png"));
            imageList.Images.Add("Group", Image.FromFile("Images/Group.png"));
            imageList.Images.Add("Mat", Image.FromFile("Images/RndMat.png"));
            imageList.Images.Add("Mesh", Image.FromFile("Images/RndMesh.png"));
            imageList.Images.Add("MultiMesh", Image.FromFile("Images/RndMultiMesh.png"));
            imageList.Images.Add("Trans", Image.FromFile("Images/RndTrans.png"));
            imageList.Images.Add("TransAnim", Image.FromFile("Images/TransAnim.png"));
            imageList.Images.Add("Sfx", Image.FromFile("Images/Sfx.png"));
            imageList.Images.Add("SynthSample", Image.FromFile("Images/Sfx.png"));
            imageList.Images.Add("PanelDir", Image.FromFile("Images/RndDir.png"));
            imageList.Images.Add("Character", Image.FromFile("Images/Character.png"));
            imageList.Images.Add("Color", Image.FromFile("Images/UIColor.png"));
            imageList.Images.Add("BandCharDesc", Image.FromFile("Images/BandCharDesc.png"));
            imageList.Images.Add("ColorPalette", Image.FromFile("Images/ColorPalette.png"));
            imageList.Images.Add("Light", Image.FromFile("Images/RndLight.png"));
            imageList.Images.Add("WorldDir", Image.FromFile("Images/WorldDir.png"));
            imageList.Images.Add("ScreenMask", Image.FromFile("Images/ScreenMask.png"));
            imageList.Images.Add("TexMovie", Image.FromFile("Images/TexMovie.png"));
            imageList.Images.Add("BandCrowdMeterDir", Image.FromFile("Images/BandCrowdMeterDir.png"));
            imageList.Images.Add("Font", Image.FromFile("Images/Font.png"));
            imageList.Images.Add("Text", Image.FromFile("Images/Text.png"));
            imageList.Images.Add("BandList", Image.FromFile("Images/Text.png"));
            imageList.Images.Add("BandCamShot", Image.FromFile("Images/Camera.png"));
            imageList.Images.Add("UIColor", Image.FromFile("Images/UIColor.png"));
            imageList.Images.Add("UILabel", Image.FromFile("Images/UILabel.png"));
            imageList.Images.Add("BandLabel", Image.FromFile("Images/UILabel.png"));
            imageList.Images.Add("CharLipSync", Image.FromFile("Images/Lipsync.png"));
            imageList.Images.Add("UIButton", Image.FromFile("Images/Button.png"));
            imageList.Images.Add("BandButton", Image.FromFile("Images/Button.png"));
            imageList.Images.Add("BandStarDisplay", Image.FromFile("Images/BandStarDisplay.png"));
            imageList.Images.Add("Environ", Image.FromFile("Images/Environ.png"));
            imageList.Images.Add("PropAnim", Image.FromFile("Images/PropAnim.png"));
            imageList.Images.Add("MeshAnim", Image.FromFile("Images/PropAnim.png"));
            imageList.Images.Add("Line", Image.FromFile("Images/Line.png"));
            imageList.Images.Add("AnimFilter", Image.FromFile("Images/AnimFilter.png"));
            imageList.Images.Add("UITrigger", Image.FromFile("Images/Trigger.png"));
            imageList.Images.Add("EventTrigger", Image.FromFile("Images/Trigger.png"));
            imageList.Images.Add("Cam", Image.FromFile("Images/Camera.png"));
            imageList.Images.Add("MeshAnim", Image.FromFile("Images/MeshAnim.png"));
            imageList.Images.Add("ParticleSys", Image.FromFile("Images/ParticleSys.png"));
            imageList.Images.Add("MatAnim", Image.FromFile("Images/MatAnim.png"));
            imageList.Images.Add("CharClip", Image.FromFile("Images/CharClip.png"));
            imageList.Images.Add("CharClipSet", Image.FromFile("Images/CharClip.png"));
            imageList.Images.Add("MidiInstrument", Image.FromFile("Images/MidiInstrument.png"));
            imageList.Images.Add("FileMerger", Image.FromFile("Images/FileMerger.png"));
            imageList.Images.Add("TransProxy", Image.FromFile("Images/TransProxy.png"));
            imageList.Images.Add("PostProc", Image.FromFile("Images/PostProc.png"));
            imageList.Images.Add("WorldInstance", Image.FromFile("Images/WorldInstance.png"));
            imageList.Images.Add("CharClipGroup", Image.FromFile("Images/CharClipGroup.png"));
            imageList.Images.Add("CheckboxDisplay", Image.FromFile("Images/CheckboxDisplay.png"));
            imageList.Images.Add("UIListDir", Image.FromFile("Images/UIListDir.png"));
            imageList.Images.Add("UIGuide", Image.FromFile("Images/UIGuide.png"));
            imageList.Images.Add("InlineHelp", Image.FromFile("Images/InlineHelp.png"));
            imageList.Images.Add("CharInterest", Image.FromFile("Images/CharInterest.png"));
            imageList.Images.Add("RandomGroupSeq", Image.FromFile("Images/RandomGroupSeq.png"));
            imageList.Images.Add("Set", Image.FromFile("Images/Set.png"));
            imageList.Images.Add("", Image.FromFile("Images/NoDir.png"));

            imageList.ColorDepth = ColorDepth.Depth32Bit;
        }

        private void PopulateListWithEntries()
        {
            if (currentMiloScene == null)
            {
                throw new Exception("Tried to populate list with milo scene entries when no Milo scene was loaded!");
            }

            HashSet<string> expandedNodes = new HashSet<string>();
            foreach (TreeNode node in miloSceneItemsTree.Nodes)
            {
                SaveExpandedNodes(node, expandedNodes);
            }

            miloSceneItemsTree.Nodes.Clear();
            miloSceneItemsTree.ShowNodeToolTips = true;

            splitContainer1.Panel2.Controls.Clear();
            miloSceneItemsTree.ImageList = imageList;

            // Add a root node for the Milo scene and its dir
            string rootName = currentMiloScene.dirMeta?.name ?? "Scene Has No Root Directory";
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = "<empty name>";
            }

            TreeNode rootNode = new TreeNode(rootName, GetImageIndex(imageList, currentMiloScene.dirMeta?.type), GetImageIndex(imageList, currentMiloScene.dirMeta?.type))
            {
                Tag = currentMiloScene.dirMeta,
                ToolTipText = $"{currentMiloScene.dirMeta?.name ?? "<empty name>"} ({currentMiloScene.dirMeta?.type ?? "Unknown"})"
            };

            miloSceneItemsTree.Nodes.Add(rootNode);

            // Handle inline subdirectories of root
            if (currentMiloScene.dirMeta != null && currentMiloScene.dirMeta.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                TreeNode inlineSubdirsNode = new TreeNode("Inline Subdirectories", GetImageIndex(imageList, "ObjectDir"), GetImageIndex(imageList, "ObjectDir"));
                foreach (var subDir in objDir.inlineSubDirs)
                {
                    AddDirectoryNode(subDir, inlineSubdirsNode, filterComboBox.Text);
                }
                rootNode.Nodes.Add(inlineSubdirsNode);
            }

            // Add root entries
            if (currentMiloScene.dirMeta != null)
            {
                AddChildNodes(currentMiloScene.dirMeta, rootNode, filterComboBox.Text);
            }

            foreach (TreeNode node in miloSceneItemsTree.Nodes)
            {
                RestoreExpandedNodes(node, expandedNodes);
            }

            miloSceneItemsTree.NodeMouseClick -= MiloSceneItemsTree_NodeMouseClick;
            miloSceneItemsTree.NodeMouseClick += MiloSceneItemsTree_NodeMouseClick;
        }

        private void SaveExpandedNodes(TreeNode node, HashSet<string> expandedNodes)
        {
            if (node.IsExpanded)
            {
                expandedNodes.Add(node.FullPath);
            }

            foreach (TreeNode child in node.Nodes)
            {
                SaveExpandedNodes(child, expandedNodes);
            }
        }

        private void RestoreExpandedNodes(TreeNode node, HashSet<string> expandedNodes)
        {
            if (expandedNodes.Contains(node.FullPath))
            {
                node.Expand();
            }

            foreach (TreeNode child in node.Nodes)
            {
                RestoreExpandedNodes(child, expandedNodes);
            }
        }


        private void AddChildNodes(DirectoryMeta parentDirMeta, TreeNode parentNode, string? filterByType)
        {
            if (parentDirMeta == null || parentDirMeta.entries == null) return;

            foreach (DirectoryMeta.Entry entry in parentDirMeta.entries)
            {
                if (filterComboBox.SelectedIndex != 0 && filterByType != null && entry.type.value != filterByType)
                {
                    continue;
                }
                // create node for entry
                TreeNode node = new TreeNode(entry.name.value, GetImageIndex(imageList, entry.type), GetImageIndex(imageList, entry.type))
                {
                    Tag = entry,
                    ToolTipText = $"{entry.name ?? "<empty name>"} ({entry.type ?? "Unknown"})"
                };


                // if this entry is actually a dir, we need to create a node under it called the dir entry
                // confusing but dirs as entries have the entry, and then the dir itself, with its own fields (but not exactly the same fields!) harmonix whyyyyyyyyyyyy
                if (entry.dir != null)
                {
                    TreeNode dirDirectoryNode = new TreeNode("Directory Entry", GetImageIndex(imageList, entry.dir.type), GetImageIndex(imageList, entry.dir.type))
                    {
                        Tag = entry.dir,
                        ToolTipText = $"{entry.dir.name ?? "<empty name>"} ({entry.dir.type ?? "Unknown"})"
                    };

                    node.Nodes.Add(dirDirectoryNode);


                    // handle inlined subdirs of entries
                    if (entry.dir.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
                    {
                        TreeNode inlinedSubdirsNode = new TreeNode("Inlined Subdirectories", GetImageIndex(imageList, "ObjectDir"), GetImageIndex(imageList, "ObjectDir"));
                        node.Nodes.Add(inlinedSubdirsNode);

                        foreach (var subDir in objDir.inlineSubDirs)
                        {
                            AddDirectoryNode(subDir, inlinedSubdirsNode, filterByType);
                        }
                    }
                    AddChildNodes(entry.dir, node, filterByType);
                }


                parentNode.Nodes.Add(node);
            }
        }


        private void AddDirectoryNode(DirectoryMeta dirMeta, TreeNode parentNode, string? filterByType)
        {
            TreeNode subDirNode = new TreeNode(dirMeta.name.value, GetImageIndex(imageList, dirMeta.type), GetImageIndex(imageList, dirMeta.type))
            {
                Tag = dirMeta,
                ToolTipText = $"{dirMeta.name ?? "<empty name>"} ({dirMeta.type ?? "Unknown"})"
            };

            parentNode.Nodes.Add(subDirNode);

            if (dirMeta.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                TreeNode inlinedSubdirsNode = new TreeNode("Inlined Subdirectories", GetImageIndex(imageList, "ObjectDir"), GetImageIndex(imageList, "ObjectDir"));
                subDirNode.Nodes.Add(inlinedSubdirsNode);

                foreach (var subDir in objDir.inlineSubDirs)
                {
                    // i love recursion
                    AddDirectoryNode(subDir, inlinedSubdirsNode, filterByType);
                }
            }

            if (dirMeta != null)
            {
                AddChildNodes(dirMeta, subDirNode, filterByType);
            }
        }


        private void MiloSceneItemsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            TreeViewHitTestInfo hitTestInfo = treeView.HitTest(e.Location);

            // Ignore clicks on whitespace or the plus/minus button
            if (hitTestInfo.Node == null || hitTestInfo.Location == TreeViewHitTestLocations.PlusMinus)
                return;

            // Prevent redundant UI redraws for already selected nodes
            if (treeView.SelectedNode != e.Node && e.Button == MouseButtons.Left)
            {
                treeView.SelectedNode = e.Node;
            }

            // Update selected node
            treeView.SelectedNode = e.Node;

            // handle clicking on dirs
            if (e.Node.Tag is DirectoryMeta obj && e.Button == MouseButtons.Left)
            {
                if (obj.directory != null)
                    CreateEditorPanelForAsset(obj.directory);
            }

            // handle clicking on assets
            if (e.Node.Tag is DirectoryMeta.Entry entry && e.Button == MouseButtons.Left)
            {
                if (entry.obj != null)
                {
                    CreateEditorPanelForAsset(entry.obj);
                }
                else
                {
                    // show an unsupported asset panel so its clear the app is still actually functioning, instead of just showing nothing
                    splitContainer1.Panel2.Controls.Clear();

                    var unsupportedPanel = new Panel
                    {
                        Dock = DockStyle.Fill
                    };

                    var unsupportedLabel = new Label
                    {
                        Text = "The asset type " + entry.type.value + " is not currently supported.",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Black,
                    };

                    unsupportedPanel.Controls.Add(unsupportedLabel);
                    splitContainer1.Panel2.Controls.Add(unsupportedPanel);
                }
            }

            // context menu logic for right-click
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Tag != null)
                {
                    ContextMenuStrip contextMenu = new ContextMenuStrip();

                    // Context menu for DirectoryMeta
                    if (e.Node.Tag is DirectoryMeta)
                    {
                        contextMenu.Items.Add(new ToolStripMenuItem("Delete Directory", SystemIcons.Error.ToBitmap(), (s, ev) => DeleteDirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripMenuItem("Duplicate Directory", SystemIcons.Information.ToBitmap(), (s, ev) => DuplicateDirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripMenuItem("Rename Directory", SystemIcons.Information.ToBitmap(), (s, ev) => RenameDirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripMenuItem("Merge Directory", SystemIcons.Information.ToBitmap(), (s, ev) => MergeDirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripMenuItem("Export Directory", SystemIcons.Information.ToBitmap(), (s, ev) => ExportDirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("Add Inlined Subdirectory", SystemIcons.Information.ToBitmap(), (s, ev) => AddInlinedSubdirectory(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("Import Asset", SystemIcons.Application.ToBitmap(), (s, ev) => ImportAsset(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("New Asset", SystemIcons.WinLogo.ToBitmap(), (s, ev) => NewAsset(e.Node)));
                    }
                    else
                    {
                        contextMenu.Items.Add(new ToolStripMenuItem("Delete Asset", SystemIcons.Error.ToBitmap(), (s, ev) => DeleteAsset(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("Duplicate Asset", SystemIcons.Information.ToBitmap(), (s, ev) => DuplicateAsset(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("Rename Asset", SystemIcons.Information.ToBitmap(), (s, ev) => RenameAsset(e.Node)));
                        contextMenu.Items.Add(new ToolStripSeparator());
                        contextMenu.Items.Add(new ToolStripMenuItem("Extract Asset", SystemIcons.Shield.ToBitmap(), (s, ev) => ExtractAsset(e.Node)));
                        contextMenu.Items.Add(new ToolStripMenuItem("Replace Asset", SystemIcons.Question.ToBitmap(), (s, ev) => ReplaceAsset(e.Node)));
                    }

                    contextMenu.Show(treeView, e.Location);
                }
            }
        }


        /// <summary>
        /// Handles deletion of an asset from the MiloFile
        /// </summary>
        /// <param name="node"></param>
        private void DeleteAsset(TreeNode node)
        {
            // get the parent dir of the node
            TreeNode parent = node.Parent;

            DirectoryMeta directoryEntry = (DirectoryMeta)parent.Tag;

            // convert our node's tag to an Object
            DirectoryMeta.Entry entry = (DirectoryMeta.Entry)node.Tag;

            // remove the entry from the MiloFile
            directoryEntry.entries.Remove(entry);

            // redraw the UI
            PopulateListWithEntries();
        }
        private void DuplicateAsset(TreeNode node)
        {
            // get the parent dir of the node
            TreeNode parent = node.Parent;

            DirectoryMeta directoryEntry = (DirectoryMeta)parent.Tag;

            // convert our node's tag to an Object
            DirectoryMeta.Entry entry = (DirectoryMeta.Entry)node.Tag;

            DirectoryMeta.Entry newEntry;

            // create a new entry
            if (entry.typeRecognized)
            {
                newEntry = new DirectoryMeta.Entry(entry.type, entry.name, entry.obj);

                var updatedBytes = entry.objBytes.ToArray().Concat(new byte[] { 0xAD, 0xDE, 0xAD, 0xDE }).ToArray();

                // hack to clone an obj without making them clonable or writing a Copy method
                // dont like this but :shrug:
                using (MemoryStream ms = new MemoryStream(updatedBytes))
                {
                    EndianReader reader = new EndianReader(ms, currentMiloScene.endian);
                    directoryEntry.ReadEntry(reader, entry);
                }
            }
            else
            {
                // create a dirty entry
                newEntry = Entry.CreateDirtyAssetFromBytes(entry.type, entry.name, entry.objBytes);

            }

            // bring up a dialog to get the new name
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the asset", "Duplicate Asset", entry.name.value);

            if (newName == "")
                return;

            // set the new name
            newEntry.name = newName;

            // add the entry to the parent dir
            directoryEntry.entries.Add(newEntry);

            // redraw the UI
            PopulateListWithEntries();
        }
        private void RenameAsset(TreeNode node)
        {
            // get the parent dir of the node
            TreeNode parent = node.Parent;

            DirectoryMeta directoryEntry = (DirectoryMeta)parent.Tag;

            // convert our node's tag to an Object
            DirectoryMeta.Entry entry = (DirectoryMeta.Entry)node.Tag;

            // open a dialog to get the new name
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the asset", "Rename Asset", entry.name.value);

            if (newName == "")
                return;

            // set the new name
            entry.name = newName;

            // redraw the UI
            PopulateListWithEntries();
        }

        private void AddInlinedSubdirectory(TreeNode node)
        {
            DirectoryMeta dirEntry = (DirectoryMeta)node.Tag;

            ObjectDir dir = (ObjectDir)dirEntry.directory;

            // get new name of directory
            string newDirName = Microsoft.VisualBasic.Interaction.InputBox("Enter the name of the new directory", "Add Inlined Subdirectory", "New Directory");

            // create a new directory
            DirectoryMeta newDir = DirectoryMeta.New("ObjectDir", newDirName, 27, 25);

            // add the new directory to the parent directory and set the reference type
            dir.inlineSubDirs.Add(newDir);
            dir.inlineSubDirNames.Add($"{newDirName}.milo");
            dir.referenceTypes.Add(ObjectDir.ReferenceType.kInlineCached);
            dir.referenceTypesAlt.Add(ObjectDir.ReferenceType.kInlineCached);

            // redraw the UI
            PopulateListWithEntries();
        }

        private void MergeDirectory(TreeNode node)
        {
            DirectoryMeta dirEntry = (DirectoryMeta)node.Tag;
            ObjectDir dir = (ObjectDir)dirEntry.directory;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc",
                Title = "Open Milo Scene"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MiloFile externalMiloScene = new MiloFile(openFileDialog.FileName);

                // Iterate through entries in the external scene to be merged
                foreach (DirectoryMeta.Entry mergeEntry in externalMiloScene.dirMeta.entries)
                {
                    bool entryMerged = false; // Flag to track if we have either merged an entry or added a new entry

                    // Iterate through the existing entries in the current scene
                    for (int i = 0; i < currentMiloScene.dirMeta.entries.Count; i++)
                    {
                        DirectoryMeta.Entry currentEntry = currentMiloScene.dirMeta.entries[i];

                        if (mergeEntry.name.value == currentEntry.name.value)
                        {
                            DialogResult result = MessageBox.Show($"An entry with the name {currentEntry.name.value} already exists. Do you want to overwrite it?", "Overwrite Entry", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                currentMiloScene.dirMeta.entries[i].obj = mergeEntry.obj;
                            }
                            entryMerged = true;
                            break; // Entry has been processed, no need to check other entries.
                        }
                    }

                    if (!entryMerged)
                    {
                        // Add a new entry from the external file
                        dirEntry.entries.Add(mergeEntry);
                    }
                }

                foreach (DirectoryMeta externalSubDir in ((ObjectDir)externalMiloScene.dirMeta.directory).inlineSubDirs)
                {
                    bool subDirMerged = false;

                    for (int i = 0; i < dir.inlineSubDirs.Count; i++)
                    {
                        DirectoryMeta currentSubDir = dir.inlineSubDirs[i];

                        if (externalSubDir.name.value == currentSubDir.name.value)
                        {
                            DialogResult result = MessageBox.Show($"An inline subdirectory with the name {currentSubDir.name.value} already exists. Do you want to overwrite it?", "Overwrite Inline Subdirectory", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                dir.inlineSubDirs[i] = externalSubDir;
                            }
                            subDirMerged = true;
                            break;
                        }
                    }

                    if (!subDirMerged)
                    {
                        dir.inlineSubDirs.Add(externalSubDir);
                        dir.inlineSubDirNames.Add($"{externalSubDir.name.value}.milo");
                        dir.referenceTypes.Add(ObjectDir.ReferenceType.kInlineCached);
                        dir.referenceTypesAlt.Add(ObjectDir.ReferenceType.kInlineCached);
                    }
                }
                PopulateListWithEntries();
            }
        }

        private void ExportDirectory(TreeNode node)
        {
            // get directory from node
            DirectoryMeta dirEntry = (DirectoryMeta)node.Tag;

            if (dirEntry != null)
            {

                // bring up the milo save options dialog
                MiloSaveOptionsForm miloSaveOptionsForm = new MiloSaveOptionsForm();
                miloSaveOptionsForm.ShowDialog();

                if (miloSaveOptionsForm.DialogResult == DialogResult.OK)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc;*.kr",
                        Title = "Save Milo Scene As...",
                        FileName = dirEntry.name
                    };

                    // create a milofile to serialize
                    MiloFile file = new MiloFile(dirEntry);
                    file.endian = currentMiloScene.endian;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        file.Save(saveFileDialog.FileName, miloSaveOptionsForm.compressionType, 0x810, Endian.LittleEndian, miloSaveOptionsForm.useBigEndian ? Endian.BigEndian : Endian.LittleEndian);
                        MessageBox.Show("Milo scene saved to " + saveFileDialog.FileName + " successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("No Milo scene loaded to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExtractAsset(TreeNode node)
        {
            // create a file save dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Save Asset As...",
                FileName = node.Text
            };

            // present it
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // get the asset
                DirectoryMeta.Entry entry = (DirectoryMeta.Entry)node.Tag;

                // write the asset to the file
                File.WriteAllBytes(saveFileDialog.FileName, entry.objBytes.ToArray());

                // show a message box
                MessageBox.Show($"Asset extracted to {saveFileDialog.FileName} successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        private void ReplaceAsset(TreeNode node) { MessageBox.Show("Unimplemented"); }

        private void DeleteDirectory(TreeNode node) { MessageBox.Show("Unimplemented"); }
        private void DuplicateDirectory(TreeNode node) { MessageBox.Show("Unimplemented"); }
        private void RenameDirectory(TreeNode node) { MessageBox.Show("Unimplemented"); }
        private void ImportAsset(TreeNode node)
        {
            // create a popup to ask for the file to import
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files(*.*)|*.*|DirectDraw Surface Textures (*.dds)|*.dds|Nautilus Prefabs (*.prefab)|*.prefab",
                Title = "Import Asset"
            };

            // present it
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // special handling of certain asset types

                // detect .prefab file
                if (Path.GetExtension(openFileDialog.FileName) == ".prefab")
                {
                    // read file
                    BandCharDesc desc = NautilusInterop.ToBandCharDesc(File.ReadAllText(openFileDialog.FileName));
                    // add the BandCharDesc to the MiloFile
                    currentMiloScene.dirMeta.entries.Add(new DirectoryMeta.Entry("BandCharDesc", "prefab_" + Path.GetFileNameWithoutExtension(openFileDialog.FileName), desc));
                    // redraw the UI
                    PopulateListWithEntries();
                    return;
                }


                DirectoryMeta directoryEntry = (DirectoryMeta)node.Tag;

                // read the file into a byte array
                byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);

                // popup a dropdown to ask for the asset type
                string[] assetTypes = { "Object", "RndTex", "RndMat", "RndMesh", "RndTrans", "RndTransAnim", "Sfx", "RndTexRenderer", "BandSongPref", "SynthSample" };

                string assetType = Microsoft.VisualBasic.Interaction.InputBox("Enter the asset type", "Import Asset", "Object");


                // create a new entry
                DirectoryMeta.Entry entry = Entry.CreateDirtyAssetFromBytes(assetType, Path.GetFileName(openFileDialog.FileName), fileBytes.ToList<byte>());

                // add the entry to the parent dir
                directoryEntry.entries.Add(entry);

                // use an EndianReader to read the bytes into an Object
                using (EndianReader reader = new EndianReader(new MemoryStream(fileBytes), Endian.BigEndian))
                {
                    // read the object
                    switch (entry.type.value)
                    {
                        case "Tex":
                            entry.obj = new RndTex().Read(reader, false, directoryEntry, entry);
                            break;
                        case "Group":
                            entry.obj = new RndGroup().Read(reader, false, directoryEntry, entry);
                            break;
                        case "Trans":
                            entry.obj = new RndTrans().Read(reader, false, directoryEntry, entry);
                            break;
                        case "BandSongPref":
                            entry.obj = new BandSongPref().Read(reader, false, directoryEntry, entry);
                            break;
                        case "Sfx":
                            entry.obj = new Sfx().Read(reader, false, directoryEntry, entry);
                            break;
                        case "BandCharDesc":
                            entry.obj = new BandCharDesc().Read(reader, false, directoryEntry, entry);
                            break;
                        default:
                            Debug.WriteLine("Unknown asset type: " + entry.type.value);
                            entry.obj = new Object().Read(reader, false, directoryEntry, entry);
                            break;
                    }
                }

                // redraw the UI
                PopulateListWithEntries();
            }
        }
        private void NewAsset(TreeNode node) { /* Implement new asset logic */ }


        private void CreateEditorPanelForAsset(Object obj)
        {

            // anything that might already be present there
            splitContainer1.Panel2.Controls.Clear();

            var revisionField = obj.GetType().GetField("revision", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            uint revisionValue = 0;

            if (revisionField != null && revisionField.FieldType == typeof(ushort))
            {
                revisionValue = Convert.ToUInt32(revisionField.GetValue(obj));
            }

            EditorPanel editorPanel = new EditorPanel(obj, revisionValue);

            editorPanel.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(editorPanel);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMiloScene != null)
            {
                currentMiloScene.Save(null, null);
                MessageBox.Show("Milo scene saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No Milo scene loaded to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentMiloScene != null)
            {
                // bring up the milo save options dialog
                MiloSaveOptionsForm miloSaveOptionsForm = new MiloSaveOptionsForm();
                miloSaveOptionsForm.ShowDialog();

                if (miloSaveOptionsForm.DialogResult == DialogResult.OK)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc;*.kr",
                        Title = "Save Milo Scene As...",
                        FileName = currentMiloScene.dirMeta.name
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentMiloScene.Save(saveFileDialog.FileName, miloSaveOptionsForm.compressionType, 0x810, Endian.LittleEndian, miloSaveOptionsForm.useBigEndian ? Endian.BigEndian : Endian.LittleEndian);
                        MessageBox.Show("Milo scene saved to " + saveFileDialog.FileName + " successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("No Milo scene loaded to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc;*.kr",
                Title = "Open Milo Scene"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentMiloScene = new MiloFile(openFileDialog.FileName);

                miloSceneLabel.Text = currentMiloScene.ToString() + " - " + currentMiloScene.dirMeta.ToString() + " - " + currentMiloScene.dirMeta.entries.Count + " entries";

                miloSceneDetailsLabel.Text = "Milo Scene Version: " + currentMiloScene.dirMeta.revision + " - Platform: " + currentMiloScene.dirMeta.platform;

                filterComboBox.Items.Clear();
                filterComboBox.Items.Add("None");
                filterComboBox.SelectedIndex = 0;

                // this will hold all the unique types of entries across the entire scenee
                HashSet<string> uniqueTypes = new HashSet<string>();
                CollectEntryTypesRecursive(currentMiloScene.dirMeta, uniqueTypes);

                foreach (string type in uniqueTypes.ToList().ToImmutableSortedSet())
                {
                    filterComboBox.Items.Add(type);
                }


                PopulateListWithEntries();
            }
        }

        private void CollectEntryTypesRecursive(DirectoryMeta dirMeta, HashSet<string> uniqueTypes)
        {
            if (dirMeta == null) return;

            // its dirs all the way down
            if (dirMeta.entries != null)
            {
                foreach (var entry in dirMeta.entries)
                {
                    uniqueTypes.Add(entry.type.value);

                    if (entry.dir != null)
                    {
                        CollectEntryTypesRecursive(entry.dir, uniqueTypes);
                    }
                }
            }

            // handle inlines
            if (dirMeta.directory is ObjectDir objDir && objDir.inlineSubDirs.Count > 0)
            {
                foreach (var subDir in objDir.inlineSubDirs)
                {
                    CollectEntryTypesRecursive(subDir, uniqueTypes);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create and open AboutForm dialog
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMiloForm newMiloForm = new NewMiloForm();

            newMiloForm.ShowDialog();

            if (newMiloForm.DialogResult == DialogResult.OK)
            {
                currentMiloScene = new MiloFile(newMiloForm.NewMilo);
                PopulateListWithEntries();
            }
        }

        private void githubLinkMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/ihatecompvir/MiloEditor", UseShellExecute = true });
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMiloScene == null)
            {
                return;
            }

            PopulateListWithEntries();
        }
    }
}