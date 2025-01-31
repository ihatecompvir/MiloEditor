namespace MiloEditor
{
    partial class MiloSaveOptionsForm
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
            dirTypeLabel = new Label();
            compressionTypeDropdown = new ComboBox();
            continueButton = new Button();
            bodyEndianLabel = new Label();
            bodyEndianDropdown = new ComboBox();
            platformDropdown = new ComboBox();
            platformLabel = new Label();
            SuspendLayout();
            // 
            // dirTypeLabel
            // 
            dirTypeLabel.AutoSize = true;
            dirTypeLabel.Location = new Point(12, 9);
            dirTypeLabel.Name = "dirTypeLabel";
            dirTypeLabel.Size = new Size(105, 15);
            dirTypeLabel.TabIndex = 0;
            dirTypeLabel.Text = "Compression Type";
            // 
            // compressionTypeDropdown
            // 
            compressionTypeDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            compressionTypeDropdown.FormattingEnabled = true;
            compressionTypeDropdown.Location = new Point(126, 6);
            compressionTypeDropdown.Name = "compressionTypeDropdown";
            compressionTypeDropdown.Size = new Size(194, 23);
            compressionTypeDropdown.TabIndex = 1;
            compressionTypeDropdown.SelectedIndexChanged += compressionTypeDropdown_SelectedIndexChanged;
            // 
            // continueButton
            // 
            continueButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            continueButton.Location = new Point(12, 100);
            continueButton.Name = "continueButton";
            continueButton.Size = new Size(308, 23);
            continueButton.TabIndex = 6;
            continueButton.Text = "Continue";
            continueButton.UseVisualStyleBackColor = true;
            continueButton.Click += button1_Click;
            // 
            // bodyEndianLabel
            // 
            bodyEndianLabel.AutoSize = true;
            bodyEndianLabel.Location = new Point(12, 38);
            bodyEndianLabel.Name = "bodyEndianLabel";
            bodyEndianLabel.Size = new Size(73, 15);
            bodyEndianLabel.TabIndex = 7;
            bodyEndianLabel.Text = "Body Endian";
            // 
            // bodyEndianDropdown
            // 
            bodyEndianDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bodyEndianDropdown.FormattingEnabled = true;
            bodyEndianDropdown.Location = new Point(126, 35);
            bodyEndianDropdown.Name = "bodyEndianDropdown";
            bodyEndianDropdown.Size = new Size(194, 23);
            bodyEndianDropdown.TabIndex = 8;
            bodyEndianDropdown.SelectedIndexChanged += bodyEndianDropdown_SelectedIndexChanged;
            // 
            // platformDropdown
            // 
            platformDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            platformDropdown.FormattingEnabled = true;
            platformDropdown.Location = new Point(126, 64);
            platformDropdown.Name = "platformDropdown";
            platformDropdown.Size = new Size(194, 23);
            platformDropdown.TabIndex = 10;
            platformDropdown.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // platformLabel
            // 
            platformLabel.AutoSize = true;
            platformLabel.Location = new Point(12, 67);
            platformLabel.Name = "platformLabel";
            platformLabel.Size = new Size(53, 15);
            platformLabel.TabIndex = 9;
            platformLabel.Text = "Platform";
            // 
            // MiloSaveOptionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(332, 135);
            Controls.Add(platformDropdown);
            Controls.Add(platformLabel);
            Controls.Add(bodyEndianDropdown);
            Controls.Add(bodyEndianLabel);
            Controls.Add(continueButton);
            Controls.Add(compressionTypeDropdown);
            Controls.Add(dirTypeLabel);
            MaximizeBox = false;
            MaximumSize = new Size(348, 250);
            MinimumSize = new Size(348, 140);
            Name = "MiloSaveOptionsForm";
            Text = "Save Options";
            Load += NewMiloForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label dirTypeLabel;
        private ComboBox compressionTypeDropdown;
        private Button continueButton;
        private Label bodyEndianLabel;
        private ComboBox bodyEndianDropdown;
        private ComboBox platformDropdown;
        private Label platformLabel;
    }
}