using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class ByteListPanel : Panel
{
    private List<byte> _byteList = new List<byte>();
    private Label _byteCountLabel;
    private Button _extractButton;
    private Button _importButton;

    public event EventHandler ByteListChanged;

    protected virtual void OnByteListChanged(EventArgs e)
    {
        ByteListChanged?.Invoke(this, e);
    }

    public ByteListPanel()
    {
        InitializePanel();
        UpdateByteCountDisplay();
    }

    [Browsable(false)]
    public List<byte> ByteList
    {
        get => _byteList;
        set
        {
            _byteList = value ?? new List<byte>();
            UpdateByteCountDisplay();
            OnByteListChanged(EventArgs.Empty);
        }
    }

    private void InitializePanel()
    {
        AutoSize = true;
        Dock = DockStyle.Top;
        _byteCountLabel = new Label
        {
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5),
            AutoSize = true
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 5)
        };

        _extractButton = new Button
        {
            Text = "Extract",
            AutoSize = true,
            Margin = new Padding(5)
        };
        _extractButton.Click += ExtractButton_Click;
        buttonPanel.Controls.Add(_extractButton);


        _importButton = new Button
        {
            Text = "Import",
            AutoSize = true,
            Margin = new Padding(5)
        };
        _importButton.Click += ImportButton_Click;
        buttonPanel.Controls.Add(_importButton);

        Controls.Add(_byteCountLabel);

        Controls.Add(buttonPanel);

    }

    private void UpdateByteCountDisplay()
    {
        _byteCountLabel.Text = $"Number of Bytes: {_byteList.Count}";
    }

    private void ExtractButton_Click(object sender, EventArgs e)
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "Binary Files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog.Title = "Export Bytes to File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(saveFileDialog.FileName, _byteList.ToArray());
                    MessageBox.Show("Bytes exported successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting bytes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }


    private void ImportButton_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Binary Files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog.Title = "Import Bytes from File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] importedBytes = File.ReadAllBytes(openFileDialog.FileName);
                    ByteList = new List<byte>(importedBytes);
                    MessageBox.Show("Bytes imported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing bytes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

}