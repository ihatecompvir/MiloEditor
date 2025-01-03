using MiloLib;
using MiloLib.Assets;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using Pfim;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static MiloLib.Assets.Rnd.RndBitmap;

namespace MiloEditor.Panels
{
    public partial class RndTexEditor : UserControl
    {
        private RndTex tex;

        private List<(string, int)> encodingTypeNames = new List<(string, int)>() {
            ("ARGB", 1),
            ("RGBA", 3),
            ("DXT1/BC1", 8),
            ("DXT5/BC3", 24),
            ("ATI2/BC5", 32),
            ("TPL_CMP", 72),
            ("TPL_CMP_ALPHA", 328),
            ("TPL_CMP_2", 583)
        };
        public RndTexEditor(RndTex tex)
        {
            this.tex = tex;
            InitializeComponent();
        }

        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            int buttonWidth = flowLayoutPanel1.ClientSize.Width / 2;
            exportButton.Width = buttonWidth;
            importButton.Width = buttonWidth;
        }

        private void RndTexEditor_Load(object sender, EventArgs e)
        {

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();


            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Field", Name = "fieldColumn", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Info", Name = "infoColumn", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dataGridView1.Rows.Add("Width", tex.width);
            dataGridView1.Rows.Add("Height", tex.height);

            dataGridView1.Rows.Add("BPP", tex.bpp);

            dataGridView1.Rows.Add("Uses External Path", tex.useExternalPath);
            dataGridView1.Rows.Add("External Path", tex.externalPath);

            string encodingName = encodingTypeNames.Find(x => x.Item2 == (int)tex.bitmap.encoding).Item1;

            dataGridView1.Rows.Add("Texture Encoding", encodingName);
            dataGridView1.Rows.Add("Number of Mip Maps", tex.bitmap.mipMaps);

            bitmapBox.SizeMode = PictureBoxSizeMode.Zoom;

            // convert the tex into a DDS and load it into the picturebox
            if (tex.bitmap.encoding == TextureEncoding.DXT1_BC1 || tex.bitmap.encoding == TextureEncoding.DXT5_BC3 || tex.bitmap.encoding == TextureEncoding.ATI2_BC5)
                LoadDdsIntoPictureBox(tex.bitmap.ConvertToImage(), bitmapBox);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            // convert the bitmap to an image
            List<byte> bytes = tex.bitmap.ConvertToImage();

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "DDS Files|*.dds";
            dialog.Title = "Save the texture as a DDS file";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                File.WriteAllBytes(dialog.FileName, bytes.ToArray());
            }
        }

        public static void LoadDdsIntoPictureBox(List<byte> ddsData, PictureBox pictureBox)
        {
            if (ddsData == null || ddsData.Count == 0)
            {
                return;
            }

            using (MemoryStream ddsStream = new MemoryStream(ddsData.ToArray()))
            using (var image = Pfimage.FromStream(ddsStream))
            {
                PixelFormat format;

                // Map Pfim.ImageFormat to GDI+ PixelFormat
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                        format = PixelFormat.Format32bppArgb;
                        break;
                    case Pfim.ImageFormat.Rgb24:
                        format = PixelFormat.Format24bppRgb;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Bitmap bitmap = new Bitmap(image.Width, image.Height, format);
                BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.WriteOnly,
                    format);

                try
                {
                    // Copy only the required bytes to match the expected size
                    int copyLength = Math.Min(image.Data.Length, Math.Abs(bitmapData.Stride) * image.Height);

                    Marshal.Copy(image.Data, 0, bitmapData.Scan0, copyLength);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                pictureBox.Image = bitmap;
            }
        }


        public void SetTexture(RndTex newTex)
        {
            tex = newTex;

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Field", Name = "fieldColumn", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Info", Name = "infoColumn", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dataGridView1.Rows.Add("Width", tex.width);
            dataGridView1.Rows.Add("Height", tex.height);

            dataGridView1.Rows.Add("BPP", tex.bpp);

            dataGridView1.Rows.Add("Uses External Path", tex.useExternalPath);
            dataGridView1.Rows.Add("External Path", tex.externalPath);

            string encodingName = encodingTypeNames.Find(x => x.Item2 == (int)tex.bitmap.encoding).Item1;

            dataGridView1.Rows.Add("Texture Encoding", encodingName);
            dataGridView1.Rows.Add("Number of Mip Maps", tex.bitmap.mipMaps);

            bitmapBox.SizeMode = PictureBoxSizeMode.Zoom;

            // convert the tex into a DDS and load it into the picturebox
            if (tex.bitmap.encoding == TextureEncoding.DXT1_BC1 || tex.bitmap.encoding == TextureEncoding.DXT5_BC3 || tex.bitmap.encoding == TextureEncoding.ATI2_BC5)
                LoadDdsIntoPictureBox(tex.bitmap.ConvertToImage(), bitmapBox);

        }
        private void importButton_Click(object sender, EventArgs e)
        {
            // bring up a file dialog for the user to select a DDS file
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "DDS Files|*.dds";
            dialog.Title = "Open a DDS file";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                // read the file into a MemoryStream
                byte[] bytes = File.ReadAllBytes(dialog.FileName);
                MemoryStream stream = new MemoryStream(bytes);

                // create an EndianReader on the stream
                EndianReader reader = new EndianReader(stream, Endian.LittleEndian);

                // create a DDS object and read the data from the stream
                DDS dds = new DDS().Read(reader);

                // update the tex with the new DDS data
                tex.bitmap.height = (ushort)dds.dwHeight;
                tex.bitmap.width = (ushort)dds.dwWidth;
                tex.bitmap.bpp = (byte)dds.pf.dwRGBBitCount;
                tex.bitmap.mipMaps = (byte)(dds.dwMipMapCount - 1);

                if (tex.bitmap.platform == DirectoryMeta.Platform.Xbox)
                {

                    // scramble every 4 dds pixels for 360
                    List<List<byte>> swappedBytes = new List<List<byte>>();
                    for (int i = 0; i < dds.pixels.Count; i += 4)
                    {
                        List<byte> swapped = new List<byte>();
                        swapped.Add(dds.pixels[i + 1]);
                        swapped.Add(dds.pixels[i]);
                        swapped.Add(dds.pixels[i + 3]);
                        swapped.Add(dds.pixels[i + 2]);
                        swappedBytes.Add(swapped);
                    }

                    tex.bitmap.textures = swappedBytes;
                }
                else
                {
                    // don't do anything special for other platforms
                    tex.bitmap.textures = new List<List<byte>>() { dds.pixels };
                }

                tex.height = dds.dwHeight;
                tex.width = dds.dwWidth;
                switch (dds.pf.dwFourCC)
                {
                    case 0x31545844:
                        tex.bitmap.encoding = TextureEncoding.DXT1_BC1;
                        tex.bpp = 4;
                        tex.bitmap.bpp = 4;
                        tex.bitmap.bpl = (ushort)((tex.bitmap.bpp * tex.bitmap.width) / 8);
                        break;
                    case 0x35545844:
                        tex.bitmap.encoding = TextureEncoding.DXT5_BC3;
                        tex.bpp = 8;
                        tex.bitmap.bpp = 8;
                        tex.bitmap.bpl = (ushort)((tex.bitmap.bpp * tex.bitmap.width) / 8);
                        break;
                    case 0x32495441:
                        tex.bitmap.encoding = TextureEncoding.ATI2_BC5;
                        tex.bpp = 8;
                        tex.bitmap.bpp = 8;
                        tex.bitmap.bpl = (ushort)((tex.bitmap.bpp * tex.bitmap.width) / 8);
                        break;
                }

                // update the picturebox with the new image
                LoadDdsIntoPictureBox(bytes.ToList(), bitmapBox);


            }
        }
    }
}
