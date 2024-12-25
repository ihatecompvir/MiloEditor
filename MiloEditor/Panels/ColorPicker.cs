using System;
using System.Drawing;
using System.Windows.Forms;

public class ColorPicker : Panel
{
    private Color _color;
    private Panel _colorDisplayPanel;

    public ColorPicker()
    {
        _color = Color.White;
        InitializeColorDisplayPanel();
        UpdateColorDisplay();
        this.Resize += ColorPicker_Resize; // Subscribe to resize event
    }


    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColorDisplay();
            OnColorChanged(EventArgs.Empty);
        }
    }

    private void InitializeColorDisplayPanel()
    {
        _colorDisplayPanel = new Panel
        {
            BackColor = _color,
            BorderStyle = BorderStyle.None // Remove border, we'll use the panel's
        };
        _colorDisplayPanel.Click += ColorDisplayPanel_Click;
        this.Controls.Add(_colorDisplayPanel);
    }


    private void UpdateColorDisplay()
    {
        if (_colorDisplayPanel != null)
        {
            _colorDisplayPanel.BackColor = _color;
            UpdateColorDisplaySizeAndPosition();
        }
    }

    private void UpdateColorDisplaySizeAndPosition()
    {
        _colorDisplayPanel.Size = new Size(256, 256); // Make the display panel a square
    }


    private void ColorDisplayPanel_Click(object sender, EventArgs e)
    {
        using (ColorDialog colorDialog = new ColorDialog())
        {
            colorDialog.Color = _color;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color = colorDialog.Color;
            }
        }
    }

    private void ColorPicker_Resize(object sender, EventArgs e)
    {
        UpdateColorDisplaySizeAndPosition();
    }

    public event EventHandler ColorChanged;

    protected virtual void OnColorChanged(EventArgs e)
    {
        ColorChanged?.Invoke(this, e);
    }
}