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
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            // update label1 with version of application in parentheses like Milo Editor (version)
            label1.Text = "Milo Editor (" + Application.ProductVersion + ")";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
