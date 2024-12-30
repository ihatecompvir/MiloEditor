namespace MiloEditor
{
    partial class NewMiloForm
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
            directoryTypeDropdown = new ComboBox();
            label1 = new Label();
            directoryRevisionDropdown = new ComboBox();
            sceneVersionDropdown = new ComboBox();
            label3 = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // dirTypeLabel
            // 
            dirTypeLabel.AutoSize = true;
            dirTypeLabel.Location = new Point(12, 9);
            dirTypeLabel.Name = "dirTypeLabel";
            dirTypeLabel.Size = new Size(83, 15);
            dirTypeLabel.TabIndex = 0;
            dirTypeLabel.Text = "Directory Type";
            // 
            // directoryTypeDropdown
            // 
            directoryTypeDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            directoryTypeDropdown.FormattingEnabled = true;
            directoryTypeDropdown.Location = new Point(126, 6);
            directoryTypeDropdown.Name = "directoryTypeDropdown";
            directoryTypeDropdown.Size = new Size(194, 23);
            directoryTypeDropdown.TabIndex = 1;
            directoryTypeDropdown.SelectedIndexChanged += directoryTypeDropdown_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 40);
            label1.Name = "label1";
            label1.Size = new Size(102, 15);
            label1.TabIndex = 2;
            label1.Text = "Directory Revision";
            // 
            // directoryRevisionDropdown
            // 
            directoryRevisionDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            directoryRevisionDropdown.FormattingEnabled = true;
            directoryRevisionDropdown.Location = new Point(126, 37);
            directoryRevisionDropdown.Name = "directoryRevisionDropdown";
            directoryRevisionDropdown.Size = new Size(194, 23);
            directoryRevisionDropdown.TabIndex = 3;
            // 
            // sceneVersionDropdown
            // 
            sceneVersionDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sceneVersionDropdown.FormattingEnabled = true;
            sceneVersionDropdown.Location = new Point(126, 67);
            sceneVersionDropdown.Name = "sceneVersionDropdown";
            sceneVersionDropdown.Size = new Size(194, 23);
            sceneVersionDropdown.TabIndex = 5;
            sceneVersionDropdown.SelectedIndexChanged += sceneVersionDropdown_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 70);
            label3.Name = "label3";
            label3.Size = new Size(79, 15);
            label3.TabIndex = 4;
            label3.Text = "Scene Version";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button1.Location = new Point(12, 103);
            button1.Name = "button1";
            button1.Size = new Size(308, 23);
            button1.TabIndex = 6;
            button1.Text = "Create Milo Scene";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // NewMiloForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(332, 134);
            Controls.Add(button1);
            Controls.Add(sceneVersionDropdown);
            Controls.Add(label3);
            Controls.Add(directoryRevisionDropdown);
            Controls.Add(label1);
            Controls.Add(directoryTypeDropdown);
            Controls.Add(dirTypeLabel);
            MaximumSize = new Size(348, 173);
            MinimumSize = new Size(348, 173);
            Name = "NewMiloForm";
            Text = "New Milo Scene";
            Load += NewMiloForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label dirTypeLabel;
        private ComboBox directoryTypeDropdown;
        private Label label1;
        private ComboBox directoryRevisionDropdown;
        private ComboBox sceneVersionDropdown;
        private Label label3;
        private Button button1;
    }
}