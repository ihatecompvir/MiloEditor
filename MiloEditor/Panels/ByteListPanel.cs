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
            saveFileDialog.Filter = "All files (*.*)|*.*";
            saveFileDialog.Title = "Export Bytes to File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] bytes = _byteList.ToArray();

                    // detect if the first 4 bytes are "iKIB"
                    if (bytes != null && bytes.Length > 0)
                    {
                        if (bytes[0] == 0x69 && bytes[1] == 0x4B && bytes[2] == 0x49 && bytes[3] == 0x42)
                        {
                            // this is a bik, so byte swap every 4 bytes
                            for (int i = 0; i < bytes.Length; i += 4)
                            {
                                byte temp = bytes[i];
                                bytes[i] = bytes[i + 3];
                                bytes[i + 3] = temp;

                                temp = bytes[i + 1];
                                bytes[i + 1] = bytes[i + 2];
                                bytes[i + 2] = temp;
                            }
                        }
                    }


                    File.WriteAllBytes(saveFileDialog.FileName, bytes);
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

                    // detect if the first 3 bytes are "BIK"
                    if (importedBytes != null && importedBytes.Length > 0)
                    {
                        if (importedBytes[0] == 0x42 && importedBytes[1] == 0x49 && importedBytes[2] == 0x4B)
                        {
                            // this is a bik, so byte swap every 4 bytes because for some reason the bytes are reversed inside milo scenes? cool
                            for (int i = 0; i < importedBytes.Length; i += 4)
                            {
                                byte temp = importedBytes[i];
                                importedBytes[i] = importedBytes[i + 3];
                                importedBytes[i + 3] = temp;

                                temp = importedBytes[i + 1];
                                importedBytes[i + 1] = importedBytes[i + 2];
                                importedBytes[i + 2] = temp;
                            }
                        }
                    }
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