using MiloLib.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiloEditor
{
    public partial class NewMiloForm : Form
    {
        public DirectoryMeta NewMilo { get; private set; }

        // directory type name, and then a tuple of the directory revision name and the revision number
        private List<(string, List<(string, uint)>)> directoryTypes = new List<(string, List<(string, uint)>)>
        {
            ("ObjectDir", new List<(string, uint)> { ("GH2", 16), ("GH2 360", 17), ("TBRB / GDRB", 22), ("RB3 / DC1", 27), ("DC2", 28) }),
            ("RndDir", new List<(string, uint)> { ("GH2", 8), ("GH2 360", 9), ("TBRB / GDRB / RB3 / DC1 / DC2", 10) }),
            ("Character", new List<(string, uint)> { ("GH2", 9), ("GH2 360", 10), ("TBRB / GDRB", 15), ("RB3 / DC1 / DC2", 18) }),
            ("PanelDir", new List<(string, uint)> { ("GH2 / GH2 360", 2), ("TBRB / GDRB", 7), ("RB3 / DC1", 8) })
        };
        private List<(string, uint)> miloSceneRevisions = new List<(string, uint)> { ("FreQuency", 6), ("GH1", 10), ("GH2 PS2", 24), ("GH2 360 / RB1 / RB2 / L:RB / GDRB / TBRB", 25), ("RB3", 28), ("DC1", 31), ("DC2 / RBB / DC3", 32) };
        public NewMiloForm()
        {
            InitializeComponent();
        }

        private void NewMiloForm_Load(object sender, EventArgs e)
        {
            // add all directory types
            foreach (var (name, _) in directoryTypes)
            {
                directoryTypeDropdown.Items.Add(name);
            }
            directoryTypeDropdown.SelectedIndex = 0;

            foreach (var (name, revision) in miloSceneRevisions)
            {
                sceneVersionDropdown.Items.Add($"{name} ({revision})");
            }
            sceneVersionDropdown.SelectedIndex = 0;

            // if Guitar Hero 1 or FreQuency is selected, set the directoryTypeDropdown to disabled
            directoryTypeDropdown.Enabled = false;
        }

        private void sceneVersionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sceneVersionDropdown.SelectedIndex == 0 || sceneVersionDropdown.SelectedIndex == 1)
            {
                directoryTypeDropdown.Enabled = false;
            }
            else
            {
                directoryTypeDropdown.Enabled = true;
            }

        }

        private void directoryTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // set directory revisions based on the directory type
            directoryRevisionDropdown.Items.Clear();
            foreach (var (name, revision) in directoryTypes[directoryTypeDropdown.SelectedIndex].Item2)
            {
                directoryRevisionDropdown.Items.Add($"{name} ({revision})");
            }
            directoryRevisionDropdown.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryMeta directoryMeta = DirectoryMeta.New(directoryTypes[directoryTypeDropdown.SelectedIndex].Item1, directoryNameTextBox.Text, miloSceneRevisions[sceneVersionDropdown.SelectedIndex].Item2, (ushort)directoryTypes[directoryTypeDropdown.SelectedIndex].Item2[directoryRevisionDropdown.SelectedIndex].Item2);
            NewMilo = directoryMeta;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
