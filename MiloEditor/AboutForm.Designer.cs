namespace MiloEditor
{
    partial class AboutForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 103);
            label1.Name = "label1";
            label1.Size = new Size(280, 23);
            label1.TabIndex = 0;
            label1.Text = "Milo Editor (1.0)";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label2.Location = new Point(12, 126);
            label2.Name = "label2";
            label2.Size = new Size(280, 106);
            label2.TabIndex = 1;
            label2.Text = "An editor for Milo scenes. \r\n\r\nThanks to everyone who has ever contributed to reverse engineering and sharing knowledge about this engine. This would not have been possible without you.";
            label2.TextAlign = ContentAlignment.TopCenter;
            label2.Click += label2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.milo;
            pictureBox1.Location = new Point(115, 25);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(75, 75);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 241);
            Controls.Add(pictureBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            MaximumSize = new Size(320, 280);
            MinimumSize = new Size(320, 280);
            Name = "AboutForm";
            Text = "About Milo Editor";
            Load += AboutForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Label label2;
        private PictureBox pictureBox1;
    }
}