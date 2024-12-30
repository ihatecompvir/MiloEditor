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
            versionLabel = new Label();
            aboutLabel = new Label();
            miloIcon = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)miloIcon).BeginInit();
            SuspendLayout();
            // 
            // versionLabel
            // 
            versionLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            versionLabel.Location = new Point(12, 103);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(280, 23);
            versionLabel.TabIndex = 0;
            versionLabel.Text = "Milo Editor (1.0)";
            versionLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // aboutLabel
            // 
            aboutLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            aboutLabel.Location = new Point(12, 126);
            aboutLabel.Name = "aboutLabel";
            aboutLabel.Size = new Size(280, 106);
            aboutLabel.TabIndex = 1;
            aboutLabel.Text = "An editor for Milo scenes. \r\n\r\nThanks to everyone who has ever contributed to reverse engineering and sharing knowledge about this engine. This would not have been possible without you.";
            aboutLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // miloIcon
            // 
            miloIcon.Image = Properties.Resources.milo;
            miloIcon.Location = new Point(115, 25);
            miloIcon.Name = "miloIcon";
            miloIcon.Size = new Size(75, 75);
            miloIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            miloIcon.TabIndex = 2;
            miloIcon.TabStop = false;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 241);
            Controls.Add(miloIcon);
            Controls.Add(aboutLabel);
            Controls.Add(versionLabel);
            MaximumSize = new Size(320, 280);
            MinimumSize = new Size(320, 280);
            Name = "AboutForm";
            Text = "About Milo Editor";
            Load += AboutForm_Load;
            ((System.ComponentModel.ISupportInitialize)miloIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label versionLabel;
        private Label aboutLabel;
        private PictureBox miloIcon;
    }
}