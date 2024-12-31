using MiloLib;
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
    public partial class MiloSaveOptionsForm : Form
    {
        public MiloFile.Type compressionType;
        public bool useBigEndian;

        public List<string> bodyEndians = new List<string>() { "Little (GH2 and earlier)", "Big (RB1 and later)" };
        public List<(string, MiloFile.Type)> compressionTypes = new List<(string, MiloFile.Type)>() {
            ("Uncompressed", MiloFile.Type.Uncompressed),
            ("ZLIB Alternate (RB3 and later)", MiloFile.Type.CompressedZlibAlt),
            ("ZLIB (AntiGrav to GDRB)", MiloFile.Type.CompressedZlib),
            ("GZIP (Amplitude and earlier)", MiloFile.Type.CompressedGzip),
        };

        public MiloSaveOptionsForm()
        {
            InitializeComponent();
        }

        private void NewMiloForm_Load(object sender, EventArgs e)
        {
            // add body endians
            foreach (string endian in bodyEndians)
            {
                bodyEndianDropdown.Items.Add(endian);
            }

            // add compression types
            foreach ((string, MiloFile.Type) compressionType in compressionTypes)
            {
                compressionTypeDropdown.Items.Add(compressionType.Item1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void compressionTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            compressionType = compressionTypes[compressionTypeDropdown.SelectedIndex].Item2;
        }

        private void bodyEndianDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            useBigEndian = bodyEndians[bodyEndianDropdown.SelectedIndex] == "Big (RB1 and later)";
        }
    }
}
