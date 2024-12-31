namespace MiloEditor.Panels
{
    partial class RndTexEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            bitmapBox = new PictureBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            exportButton = new Button();
            importButton = new Button();
            dataGridView1 = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)bitmapBox).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // bitmapBox
            // 
            bitmapBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            bitmapBox.Location = new Point(3, 3);
            bitmapBox.Name = "bitmapBox";
            bitmapBox.Size = new Size(445, 166);
            bitmapBox.TabIndex = 0;
            bitmapBox.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(exportButton);
            flowLayoutPanel1.Controls.Add(importButton);
            flowLayoutPanel1.Location = new Point(3, 364);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(445, 25);
            flowLayoutPanel1.TabIndex = 7;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.Resize += flowLayoutPanel1_Resize;
            // 
            // exportButton
            // 
            exportButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            exportButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            exportButton.Location = new Point(0, 0);
            exportButton.Margin = new Padding(0);
            exportButton.Name = "exportButton";
            exportButton.Size = new Size(75, 25);
            exportButton.TabIndex = 0;
            exportButton.Text = "Export Image";
            exportButton.UseVisualStyleBackColor = true;
            exportButton.Click += exportButton_Click;
            // 
            // importButton
            // 
            importButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            importButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            importButton.Location = new Point(75, 0);
            importButton.Margin = new Padding(0);
            importButton.Name = "importButton";
            importButton.Size = new Size(75, 25);
            importButton.TabIndex = 1;
            importButton.Text = "Import Image";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += importButton_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(3, 175);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ShowCellErrors = false;
            dataGridView1.ShowCellToolTips = false;
            dataGridView1.ShowEditingIcon = false;
            dataGridView1.ShowRowErrors = false;
            dataGridView1.Size = new Size(445, 183);
            dataGridView1.TabIndex = 8;
            // 
            // RndTexEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dataGridView1);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(bitmapBox);
            Name = "RndTexEditor";
            Size = new Size(451, 392);
            Load += RndTexEditor_Load;
            ((System.ComponentModel.ISupportInitialize)bitmapBox).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox bitmapBox;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button exportButton;
        private Button importButton;
        private DataGridView dataGridView1;
    }
}
