using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Forms.TextBox;
using ComboBox = System.Windows.Forms.ComboBox;

public class EditorPanel : Panel
{
    private object targetObject;
    private bool drawTypeLabels;
    public uint revision;

    // Cache for Reflection info
    private static Dictionary<Type, List<FieldInfo>> _fieldCache = new Dictionary<Type, List<FieldInfo>>();
    private static Dictionary<Type, NameAttribute> _nameAttributeCache = new Dictionary<Type, NameAttribute>();
    private static Dictionary<Type, DescriptionAttribute> _descriptionAttributeCache = new Dictionary<Type, DescriptionAttribute>();

    // Cache for UI Elements
    private static Dictionary<Type, (Label NameLabel, Label DescriptionLabel)> _uiElementCache = new Dictionary<Type, (Label, Label)>();

    public EditorPanel(object target, uint objRevision, bool drawTypeLabels = true)
    {
        this.revision = objRevision;
        this.targetObject = target;
        this.AutoScroll = true;
        this.drawTypeLabels = drawTypeLabels;

        if (targetObject != null)
        {
            BuildUI(targetObject, 10, 10, drawTypeLabels);
        }
        else
        {
            Label emptyMessage = new Label
            {
                Text = "The current object type does not yet have a definition and cannot be edited.",
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Padding = new Padding(10)
            };
            this.Controls.Add(emptyMessage);
        }
    }

    private List<FieldInfo> GetCachedFields(Type type)
    {
        if (!_fieldCache.TryGetValue(type, out List<FieldInfo> fields))
        {
            fields = new List<FieldInfo>();
            Type currentType = type;
            Stack<Type> typeHierarchy = new Stack<Type>();

            while (currentType != null && currentType != typeof(object))
            {
                typeHierarchy.Push(currentType);
                currentType = currentType.BaseType;
            }

            while (typeHierarchy.Count > 0)
            {
                var current = typeHierarchy.Pop();
                fields.AddRange(current.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy));
            }
            _fieldCache[type] = fields;
        }
        return fields;
    }

    private NameAttribute GetCachedNameAttribute(Type type)
    {
        if (!_nameAttributeCache.TryGetValue(type, out NameAttribute attribute))
        {
            attribute = type.GetCustomAttribute<NameAttribute>();
            _nameAttributeCache[type] = attribute;
        }
        return attribute;
    }

    private DescriptionAttribute GetCachedDescriptionAttribute(Type type)
    {
        if (!_descriptionAttributeCache.TryGetValue(type, out DescriptionAttribute attribute))
        {
            attribute = type.GetCustomAttribute<DescriptionAttribute>();
            _descriptionAttributeCache[type] = attribute;
        }
        return attribute;
    }

    private (Label NameLabel, Label DescriptionLabel) GetOrCreateCachedLabels(Type type)
    {
        if (!_uiElementCache.TryGetValue(type, out var labels))
        {
            labels.NameLabel = new Label
            {
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(5),
                Height = 25,
                AutoEllipsis = false
            };
            labels.DescriptionLabel = new Label
            {
                Font = new Font("Arial", 8, FontStyle.Regular),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DimGray,
                Padding = new Padding(5, 0, 5, 10),
                Height = 50,
                AutoSize = false,
                AutoEllipsis = true
            };
            _uiElementCache[type] = labels;
        }
        return labels;
    }

    private object ResolveFieldOwner(object currentObject, FieldInfo field)
    {
        Type declaringType = field.DeclaringType;

        if (declaringType.IsInstanceOfType(currentObject))
        {
            return currentObject;
        }

        var fields = currentObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields)
        {
            var fieldValue = f.GetValue(currentObject);
            if (fieldValue != null && declaringType.IsInstanceOfType(fieldValue))
            {
                return ResolveFieldOwner(fieldValue, field);
            }
        }
        return null;
    }

    private void BuildUI(object obj, int startX, int startY, bool drawTypeLabels)
    {
        Type objType = obj.GetType();
        var objNameAttr = GetCachedNameAttribute(objType);
        var objDescriptionAttr = GetCachedDescriptionAttribute(objType);

        var tableLayout = new TableLayoutPanel
        {
            Location = new Point(startX, startY),
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Padding = new Padding(5),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoScroll = true
        };

        this.SuspendLayout();
        tableLayout.SuspendLayout();

        try
        {
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            int currentRow = 0;

            if (drawTypeLabels && (objNameAttr != null || objDescriptionAttr != null))
            {
                string objTypeName = objNameAttr?.Value ?? $"Type: {objType.Name}";
                string objTypeDescription = objDescriptionAttr?.Value ?? "No description available.";

                (Label objTypeNameLabel, Label objTypeDescriptionLabel) = GetOrCreateCachedLabels(objType);

                objTypeNameLabel.Text = objTypeName;
                objTypeDescriptionLabel.Text = objTypeDescription;

                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayout.Controls.Add(objTypeNameLabel, 0, currentRow);
                tableLayout.SetColumnSpan(objTypeNameLabel, 2);

                currentRow++;

                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayout.Controls.Add(objTypeDescriptionLabel, 0, currentRow);
                tableLayout.SetColumnSpan(objTypeDescriptionLabel, 2);

                currentRow++;
            }


            var fields = GetCachedFields(objType);

            foreach (var field in fields)
            {
                var minVersionAttr = field.GetCustomAttribute<MinVersionAttribute>();
                var maxVersionAttr = field.GetCustomAttribute<MaxVersionAttribute>();

                int minVersion = minVersionAttr?.Version ?? 0;
                int maxVersion = maxVersionAttr?.Version ?? ushort.MaxValue;

                if (revision < minVersion || revision > maxVersion)
                {
                    continue;
                }

                var nameAttr = field.GetCustomAttribute<NameAttribute>();
                var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();

                string displayName = nameAttr?.Value ?? field.Name;
                string description = descriptionAttr?.Value;

                var labelPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    BackColor = Color.LightGray,
                    Padding = new Padding(5)
                };

                if (!string.IsNullOrEmpty(description))
                {
                    var descLabel = new Label
                    {
                        Text = description,
                        AutoSize = false,
                        AutoEllipsis = true,
                        ForeColor = Color.Gray,
                        Dock = DockStyle.Top,
                        Height = 30,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    labelPanel.Controls.Add(descLabel);
                }

                var titleLabel = new Label
                {
                    Text = displayName,
                    AutoSize = false,
                    AutoEllipsis = true,
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                labelPanel.Controls.Add(titleLabel);

                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayout.Controls.Add(labelPanel, 0, currentRow);

                var fieldValue = field.GetValue(obj);
                Control inputControl = null;


                // switch based on the field type
                // this is where we will display custom or more complex UIs (e.g., Color picker)
                switch (fieldValue)
                {
                    // color pickers for a HmxColor List (inside ColorPalettes, mainly, but can appear other places)
                    case List<HmxColor> colorList:
                        inputControl = BuildColorPicker(colorList, field);
                        break;
                    case HmxColor color:
                        inputControl = BuildColorPicker(new List<HmxColor> { color }, field);
                        break;
                    // list of bytes
                    case List<byte> byteValue:
                        var byteListPanel = new ByteListPanel();
                        byteListPanel.ByteList = byteValue;
                        byteListPanel.ByteListChanged += (sender, args) =>
                        {
                            var control = (ByteListPanel)sender;
                            object ownerObject = ResolveFieldOwner(targetObject, field);
                            if (ownerObject != null)
                            {
                                field.SetValue(ownerObject, control.ByteList);
                            }
                        };

                        inputControl = byteListPanel;
                        break;
                    // generic, view only collections
                    // all objects get cased ToString to be displayed
                    // does not support editing at this time, only viewing
                    case IEnumerable collection:
                        inputControl = BuildDataGridView(collection);
                        break;
                    // check box for boolean values
                    case bool boolValue:
                        inputControl = BuildCheckBox(boolValue, field);
                        break;
                    // text box for strings and Symbols
                    case object stringValue when field.FieldType == typeof(string) || field.FieldType.Name == "Symbol":
                        inputControl = BuildTextField(stringValue);
                        break;
                    // drop down for enums
                    // TODO: add a way to give "friendly" names to enum cases, like the Name and Description attributes
                    case object enumValue when field.FieldType.IsEnum:
                        inputControl = BuildEnumComboBox(enumValue, field);
                        break;
                    // text box for primitive types
                    // this should handle things like floating point values, integers, etc and apply validation and range enforcement (e.g. no 0xFFFFFFFF in a short)
                    case object primitiveValue when field.FieldType.IsPrimitive:
                        inputControl = BuildPrimitiveTextBox(primitiveValue, field);
                        break;
                    // nested objects
                    // used for objects within objects
                    case object nestedObject when fieldValue != null:
                        inputControl = BuildNestedPanel(nestedObject);
                        break;
                }


                if (inputControl != null)
                {
                    tableLayout.Controls.Add(inputControl, 1, currentRow);
                }

                currentRow++;
            }

            this.Controls.Clear();
            this.Controls.Add(tableLayout);
        }
        finally
        {
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }


    private Control BuildColorPicker(List<HmxColor> colorList, FieldInfo field)
    {
        var scrollingPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 200,
            AutoScroll = true
        };
        var flowLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0),
        };
        scrollingPanel.Controls.Add(flowLayoutPanel);

        int colorPickerIndex = 0;

        foreach (var hmxColor in colorList)
        {
            var colorPicker = new ColorPicker
            {
                Width = 30,
                Margin = new Padding(5),
            };

            colorPicker.BorderStyle = BorderStyle.FixedSingle;

            colorPicker.Color = Color.FromArgb(255, (int)(hmxColor.r * 255), (int)(hmxColor.g * 255), (int)(hmxColor.b * 255));

            int colorIndex = colorPickerIndex;
            colorPicker.ColorChanged += (sender, args) =>
            {
                var picker = (ColorPicker)sender;
                var color = picker.Color;
                float a = color.A / 255.0f;
                float r = color.R / 255.0f;
                float g = color.G / 255.0f;
                float b = color.B / 255.0f;

                object ownerObject = ResolveFieldOwner(targetObject, field);

                if (ownerObject != null)
                {
                    List<HmxColor> ownedList = (List<HmxColor>)field.GetValue(ownerObject);
                    if (ownedList != null && ownedList.Count > colorIndex)
                    {
                        ownedList[colorIndex].a = a;
                        ownedList[colorIndex].r = r;
                        ownedList[colorIndex].g = g;
                        ownedList[colorIndex].b = b;
                    }
                }
            };
            flowLayoutPanel.Controls.Add(colorPicker);
            colorPickerIndex++;
        }

        return scrollingPanel;
    }
    private Control BuildDataGridView(IEnumerable collection)
    {
        var dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            Margin = new Padding(5, 3, 10, 3)
        };
        dataGridView.Columns.Add("Value", "Value");

        foreach (var item in collection)
        {
            dataGridView.Rows.Add(item?.ToString() ?? string.Empty);
        }
        return dataGridView;
    }

    private Control BuildCheckBox(bool boolValue, FieldInfo field)
    {
        var checkBox = new System.Windows.Forms.CheckBox
        {
            Checked = boolValue,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 3, 10, 3)
        };
        checkBox.Click += (sender, args) =>
        {
            var control = (System.Windows.Forms.CheckBox)sender;
            object ownerObject = ResolveFieldOwner(targetObject, field);
            if (ownerObject != null)
            {
                field.SetValue(ownerObject, control.Checked);
            }
        };
        return checkBox;
    }

    private Control BuildTextField(object fieldValue)
    {
        return new TextBox
        {
            Text = fieldValue?.ToString(),
            Dock = DockStyle.Fill,
            Margin = new Padding(5, 3, 10, 3)
        };
    }

    private Control BuildEnumComboBox(object enumValue, FieldInfo field)
    {
        var comboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(5, 3, 10, 3)
        };

        foreach (var value in Enum.GetValues(field.FieldType))
        {
            comboBox.Items.Add(value);
        }
        comboBox.SelectedItem = enumValue;

        comboBox.SelectedIndexChanged += (sender, args) =>
        {
            var control = (ComboBox)sender;
            object ownerObject = ResolveFieldOwner(targetObject, field);
            if (ownerObject != null)
            {
                field.SetValue(ownerObject, control.SelectedItem);
            }
        };
        return comboBox;
    }
    private Control BuildPrimitiveTextBox(object primitiveValue, FieldInfo field)
    {
        var textBox = new TextBox
        {
            Text = primitiveValue?.ToString(),
            Dock = DockStyle.Fill,
            Margin = new Padding(5, 3, 10, 3)
        };

        bool isNumber = field.FieldType == typeof(int) || field.FieldType == typeof(uint) ||
                        field.FieldType == typeof(short) || field.FieldType == typeof(ushort) ||
                        field.FieldType == typeof(long) || field.FieldType == typeof(ulong) ||
                        field.FieldType == typeof(float) || field.FieldType == typeof(double) ||
                        field.FieldType == typeof(decimal);


        if (isNumber)
        {
            textBox.KeyPress += (sender, e) =>
            {
                string currentText = ((TextBox)sender).Text;
                string newText = currentText + e.KeyChar;

                bool isValid = field.FieldType switch
                {
                    var t when t == typeof(int) => int.TryParse(newText, out _),
                    var t when t == typeof(uint) => uint.TryParse(newText, out _),
                    var t when t == typeof(short) => short.TryParse(newText, out _),
                    var t when t == typeof(ushort) => ushort.TryParse(newText, out _),
                    var t when t == typeof(long) => long.TryParse(newText, out _),
                    var t when t == typeof(ulong) => ulong.TryParse(newText, out _),
                    var t when t == typeof(float) => float.TryParse(newText, out _),
                    var t when t == typeof(double) => double.TryParse(newText, out _),
                    var t when t == typeof(decimal) => decimal.TryParse(newText, out _),
                    _ => false
                };

                if (e.KeyChar == '-' && currentText.Length == 0 &&
                     (field.FieldType == typeof(int) || field.FieldType == typeof(short) ||
                      field.FieldType == typeof(long) || field.FieldType == typeof(float) ||
                      field.FieldType == typeof(double) || field.FieldType == typeof(decimal)))
                {
                    return;
                }

                e.Handled = !isValid && !char.IsControl(e.KeyChar);
            };
            textBox.TextChanged += (sender, e) =>
            {
                TextBox textBox = (TextBox)sender;
                string text = textBox.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                bool isValid = field.FieldType switch
                {
                    var t when t == typeof(int) => int.TryParse(text, out _),
                    var t when t == typeof(uint) => uint.TryParse(text, out _),
                    var t when t == typeof(short) => short.TryParse(text, out _),
                    var t when t == typeof(ushort) => ushort.TryParse(text, out _),
                    var t when t == typeof(long) => long.TryParse(text, out _),
                    var t when t == typeof(ulong) => ulong.TryParse(text, out _),
                    var t when t == typeof(float) => float.TryParse(text, out _),
                    var t when t == typeof(double) => double.TryParse(text, out _),
                    var t when t == typeof(decimal) => decimal.TryParse(text, out _),
                    _ => false
                };

                if (!isValid)
                {
                    textBox.Text = primitiveValue?.ToString() ?? "0";
                    textBox.SelectionStart = textBox.Text.Length;
                }

                object ownerObject = ResolveFieldOwner(targetObject, field);
                if (ownerObject != null)
                {
                    field.SetValue(ownerObject, Convert.ChangeType(textBox.Text, field.FieldType));
                }
            };
            textBox.Leave += (sender, e) =>
            {
                TextBox textBox = (TextBox)sender;

                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "0";
                    textBox.SelectionStart = textBox.Text.Length;

                    object ownerObject = ResolveFieldOwner(targetObject, field);
                    if (ownerObject != null)
                    {
                        field.SetValue(ownerObject, Convert.ChangeType(textBox.Text, field.FieldType));
                    }
                }
            };
        }

        return textBox;
    }
    private Control BuildNestedPanel(object nestedObject)
    {
        var nestedPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        var nestedTableLayout = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1
        };

        var nestedNameAttr = GetCachedNameAttribute(nestedObject.GetType());
        var nestedDescriptionAttr = GetCachedDescriptionAttribute(nestedObject.GetType());

        bool nestedDrawLabels = nestedNameAttr != null || nestedDescriptionAttr != null;

        if (nestedObject != null)
        {
            BuildUI(nestedObject, 0, 0, nestedDrawLabels);

            foreach (Control control in this.Controls)
            {
                nestedPanel.Controls.Add(control);
            }
            this.Controls.Clear();

            nestedTableLayout.Controls.Add(nestedPanel, 0, 0);
        }
        return nestedTableLayout;
    }
}