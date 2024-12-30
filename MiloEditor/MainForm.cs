using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using System.Diagnostics;
using System.DirectoryServices;
using System.Reflection;
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


        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadAssetClassImages()
        {
            imageList.Images.Add("default", Image.FromFile("Images/default.png"));
            imageList.Images.Add("ObjectDir", Image.FromFile("Images/ObjectDir.png"));
            imageList.Images.Add("RndDir", Image.FromFile("Images/RndDir.png"));
            imageList.Images.Add("Object", Image.FromFile("Images/default.png"));
            imageList.Images.Add("BandSongPref", Image.FromFile("Images/BandSongPref.png"));
            imageList.Images.Add("Tex", Image.FromFile("Images/RndTex.png"));
            imageList.Images.Add("TexRenderer", Image.FromFile("Images/RndTex.png"));
            imageList.Images.Add("Mat", Image.FromFile("Images/RndMat.png"));
            imageList.Images.Add("Mesh", Image.FromFile("Images/RndMesh.png"));
            imageList.Images.Add("MultiMesh", Image.FromFile("Images/RndMesh.png"));
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
            imageList.Images.Add("", Image.FromFile("Images/NoDir.png"));

            imageList.ColorDepth = ColorDepth.Depth32Bit;
        }

        private void PopulateListWithEntries()
        {
            if (currentMiloScene == null)
            {
                throw new Exception("Tried to populate list with milo scene entries when no Milo scene was loaded!");
            }

            // clear the TreeView of all existing nodes
            miloSceneItemsTree.Nodes.Clear();

            // clear the editor panel
            splitContainer1.Panel2.Controls.Clear();

            miloSceneItemsTree.ImageList = imageList;

            // add a root node for the Milo scene and its dir
            TreeNode rootNode = new TreeNode(currentMiloScene.dirMeta.name, GetImageIndex(imageList, currentMiloScene.dirMeta.type), GetImageIndex(imageList, currentMiloScene.dirMeta.type))
            {
                Tag = currentMiloScene.dirMeta
            };

            miloSceneItemsTree.Nodes.Add(rootNode);


            if (currentMiloScene.dirMeta.type != "")
            {
                // Check if there are any inline subdirectories
                var inlineSubDirs = ((ObjectDir)currentMiloScene.dirMeta.directory).inlineSubDirs;
                if (inlineSubDirs.Count > 0)
                {
                    // Add a parent node for inline subdirectories
                    TreeNode inlineSubdirsNode = new TreeNode("Inline Subdirectories", -1, -1);

                    foreach (DirectoryMeta subDir in inlineSubDirs)
                    {
                        // Create a node for the subdirectory
                        TreeNode subDirNode = new TreeNode(subDir.name.value, GetImageIndex(imageList, subDir.type), GetImageIndex(imageList, subDir.type))
                        {
                            Tag = subDir
                        };

                        foreach (DirectoryMeta.Entry entry in subDir.entries)
                        {
                            TreeNode node = new TreeNode(entry.name.value, GetImageIndex(imageList, entry.type), GetImageIndex(imageList, entry.type))
                            {
                                Tag = entry
                            };
                            subDirNode.Nodes.Add(node);
                        }

                        // Add the subdirectory node to the Inline Subdirectories node
                        inlineSubdirsNode.Nodes.Add(subDirNode);
                    }

                    // Add the Inline Subdirectories node to the root
                    rootNode.Nodes.Add(inlineSubdirsNode);
                }
            }

            // add all the nodes for the children of the root dir
            foreach (DirectoryMeta.Entry entry in currentMiloScene.dirMeta.entries)
            {
                TreeNode node = new TreeNode(entry.name.value, GetImageIndex(imageList, entry.type), GetImageIndex(imageList, entry.type))
                {
                    Tag = entry
                };

                // detect if it is a directory
                if (entry.dir != null)
                {

                    foreach (DirectoryMeta.Entry dirEntry in entry.dir.entries)
                    {
                        TreeNode dirNode = new TreeNode(dirEntry.name.value, GetImageIndex(imageList, dirEntry.type), GetImageIndex(imageList, dirEntry.type))
                        {
                            Tag = dirEntry
                        };
                        node.Nodes.Add(dirNode);
                    }
                }
                rootNode.Nodes.Add(node);
            }

            // onclick handlers so the tree view actually functions
            // first we remove any existing handlers then add some to prevent any weird dupe issues
            miloSceneItemsTree.NodeMouseClick -= MiloSceneItemsTree_NodeMouseClick;
            miloSceneItemsTree.NodeMouseClick += MiloSceneItemsTree_NodeMouseClick;
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
                    CreateEditorPanelForAsset(entry.obj);
            }

            // context menu logic for right-click
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                // Context menu for DirectoryMeta
                if (e.Node.Tag is DirectoryMeta)
                {
                    contextMenu.Items.Add(new ToolStripMenuItem("Delete Directory", SystemIcons.Error.ToBitmap(), (s, ev) => DeleteDirectory(e.Node)));
                    contextMenu.Items.Add(new ToolStripSeparator());
                    contextMenu.Items.Add(new ToolStripMenuItem("Duplicate Directory", SystemIcons.Information.ToBitmap(), (s, ev) => DuplicateDirectory(e.Node)));
                    contextMenu.Items.Add(new ToolStripSeparator());
                    contextMenu.Items.Add(new ToolStripMenuItem("Rename Directory", SystemIcons.Information.ToBitmap(), (s, ev) => RenameDirectory(e.Node)));
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

            // create a new entry
            DirectoryMeta.Entry newEntry = new DirectoryMeta.Entry(entry.type, entry.name, entry.obj);

            // bring up a dialog to get the new name
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the asset", "Duplicate Asset", entry.name.value);

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

            // set the new name
            entry.name = newName;

            // redraw the UI
            PopulateListWithEntries();
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
                Filter = "All Files(*.*)|*.* ",
                Title = "Import Asset"
            };

            // present it
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

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
                        case "RndTex":
                            entry.obj = new RndTex().Read(reader, false);
                            break;
                        case "RndGroup":
                            entry.obj = new RndGroup().Read(reader, false);
                            break;
                        case "RndTrans":
                            entry.obj = new RndTrans().Read(reader, false);
                            break;
                        case "BandSongPref":
                            entry.obj = new BandSongPref().Read(reader, false);
                            break;
                        case "Sfx":
                            entry.obj = new Sfx().Read(reader, false);
                            break;
                        case "BandCharDesc":
                            entry.obj = new BandCharDesc().Read(reader, false);
                            break;
                        default:
                            Debug.WriteLine("Unknown asset type: " + entry.type.value);
                            entry.obj = new Object().Read(reader, false);
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
                // just for testing so we don't overwrite the original milo
                // TODO: make it actually save to the original file in the future when we know we didn't break anything
                currentMiloScene.Save("test.milo_xbox", MiloFile.Type.Uncompressed);
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
                // open Save As panel
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc",
                    Title = "Save Milo Scene As...",
                    FileName = currentMiloScene.dirMeta.name
                };

                // present it
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentMiloScene.Save(saveFileDialog.FileName, MiloFile.Type.Uncompressed);
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
                Filter = "Milo Scenes|*.milo_ps2;*.milo_xbox;*.milo_ps3;*.milo_wii;*.milo_pc;*.rnd;*.rnd_ps2;*.rnd_xbox;*.rnd_gc",
                Title = "Open Milo Scene"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentMiloScene = new MiloFile(openFileDialog.FileName);


                miloSceneLabel.Text = currentMiloScene.ToString() + " - " + currentMiloScene.dirMeta.ToString();

                PopulateListWithEntries();
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
    }
}
