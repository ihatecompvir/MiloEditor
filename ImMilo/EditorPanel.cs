using System.Collections;
using System.Reflection;
using System.Text;
using ImGuiNET;
using ImMilo.ImGuiUtils;
using MiloLib.Assets;
using MiloLib.Classes;
using Veldrid;
using Object = MiloLib.Assets.Object;
using Vector2 = System.Numerics.Vector2;

namespace ImMilo;

public class EditorPanel
{
    // Cache for Reflection info
    private static Dictionary<Type, List<FieldInfo>> _fieldCache = new Dictionary<Type, List<FieldInfo>>();
    private static Dictionary<Type, NameAttribute> _nameAttributeCache = new Dictionary<Type, NameAttribute>();

    private static Dictionary<Type, DescriptionAttribute> _descriptionAttributeCache =
        new Dictionary<Type, DescriptionAttribute>();

    private static Dictionary<Type, List<string>> _enumValueCache = new Dictionary<Type, List<string>>();

    private static List<string> GetCachedEnumValues(Type enumType)
    {
        if (!_enumValueCache.TryGetValue(enumType, out var enumValues))
        {
            enumValues = new List<string>();
            foreach (var value in Enum.GetValues(enumType))
            {
                enumValues.Add(value.ToString());
            }
        }

        return enumValues;
    }

    private static List<FieldInfo> GetCachedFields(Type type)
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
                fields.AddRange(current.GetFields(BindingFlags.Public | BindingFlags.Instance |
                                                  BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy));
            }

            _fieldCache[type] = fields;
        }

        return fields;
    }

    private static NameAttribute GetCachedNameAttribute(Type type)
    {
        if (!_nameAttributeCache.TryGetValue(type, out NameAttribute attribute))
        {
            attribute = type.GetCustomAttribute<NameAttribute>();
            _nameAttributeCache[type] = attribute;
        }

        return attribute;
    }

    private static DescriptionAttribute GetCachedDescriptionAttribute(Type type)
    {
        if (!_descriptionAttributeCache.TryGetValue(type, out DescriptionAttribute attribute))
        {
            attribute = type.GetCustomAttribute<DescriptionAttribute>();
            _descriptionAttributeCache[type] = attribute;
        }

        return attribute;
    }

    private object ResolveFieldOwner(object currentObject, FieldInfo field)
    {
        Type declaringType = field.DeclaringType;

        if (declaringType.IsInstanceOfType(currentObject))
        {
            return currentObject;
        }

        var fields = currentObject.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

    private static void DrawPrimitiveEdit(object obj, object primitiveValue, FieldInfo field)
    {
        bool isNumber = field.FieldType == typeof(int) || field.FieldType == typeof(uint) ||
                        field.FieldType == typeof(short) || field.FieldType == typeof(ushort) ||
                        field.FieldType == typeof(long) || field.FieldType == typeof(ulong) ||
                        field.FieldType == typeof(float) || field.FieldType == typeof(double) ||
                        field.FieldType == typeof(decimal) || field.FieldType == typeof(byte);

        if (isNumber)
        {
            object newVal = primitiveValue;
            bool changed = false;
            switch (primitiveValue)
            {
                case float f:
                    changed = ImGui.InputFloat("", ref f);
                    newVal = f;
                    break;
                case int i:
                    changed = ImGui.InputInt("", ref i);
                    newVal = i;
                    break;
                case uint u:
                    changed = Util.InputUInt("", ref u);
                    newVal = u;
                    break;
                case short s:
                    changed = Util.InputShort("", ref s);
                    newVal = s;
                    break;
                case ushort us:
                    changed = Util.InputUShort("", ref us);
                    newVal = us;
                    break;
                case long l:
                    changed = Util.InputLong("", ref l);
                    newVal = l;
                    break;
                case ulong ul:
                    changed = Util.InputULong("", ref ul);
                    newVal = ul;
                    break;
                case double d:
                    changed = ImGui.InputDouble("", ref d);
                    newVal = d;
                    break;
                case decimal d:
                    if ((decimal)(double)d != d)
                    {
                        Console.WriteLine("Warning: Decimal field " + field.Name +
                                          " has a value that is truncated when converted to Double.");
                    }

                    var tempDouble = (double)d;
                    changed = ImGui.InputDouble("", ref tempDouble);
                    newVal = tempDouble; //Not sure how to implement decimal properly. Just using a double.
                    break;
                case byte b:
                    changed = Util.InputByte("", ref b);
                    newVal = b;
                    break;
            }

            if (changed)
            {
                field.SetValue(obj, newVal);
            }
        }
    }

    public static void Draw(object obj, int id = 0, bool drawLabels = true,
        ImGuiTableFlags toggleFlags = ImGuiTableFlags.None)
    {
        Type objType = obj.GetType();

        var revisionField = obj.GetType()
            .GetField("revision", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        uint revision = 0;

        if (revisionField != null && revisionField.FieldType == typeof(ushort))
        {
            revision = Convert.ToUInt32(revisionField.GetValue(obj));
        }

        if (drawLabels)
        {
            var objNameAttr = GetCachedNameAttribute(objType);
            var objDescriptionAttr = GetCachedDescriptionAttribute(objType);

            ImGui.Text(objNameAttr?.Value ?? $"Type: {objType.Name}");
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            ImGui.TextWrapped(objDescriptionAttr?.Value ?? "No description available.");

            ImGui.PopStyleVar();
            ImGui.BeginChild("editor values##" + objType.Name);
        }


        ImGui.PushID(id);
        ImGui.PushID(obj.GetHashCode());

        ImGuiTableFlags flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp;
        flags ^= toggleFlags;

        var subID = 0;
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(2, 3));
        if (ImGui.BeginTable("Object fields", 2, flags))
        {
            var fields = GetCachedFields(objType);

            //ImGui.TableSetColumnIndex(0);
            ImGui.TableSetupColumn("Field label", ImGuiTableColumnFlags.None, 20);
            //ImGui.TableSetColumnIndex(1);
            ImGui.TableSetupColumn("Field value", ImGuiTableColumnFlags.None, 80);

            foreach (var field in fields)
            {
                ImGui.PushID(subID);
                
                var nameAttr = field.GetCustomAttribute<NameAttribute>();
                var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();

                string displayName = nameAttr?.Value ?? field.Name;
                string description = descriptionAttr?.Value;

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.TextWrapped(displayName);
                if (description != null)
                {
                    if (Settings.Current.HideFieldDescriptions)
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled("(?)");
                        if (ImGui.BeginItemTooltip())
                        {
                            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 50.0f);
                            ImGui.TextWrapped(description);
                            ImGui.PopTextWrapPos();
                            ImGui.EndTooltip();
                        }
                    }
                    else
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                        ImGui.TextWrapped(description);
                        ImGui.PopStyleVar();
                    }
                }   

                ImGui.TableSetColumnIndex(1);
                
                ValueEditor(field, obj, drawLabels, id);
                id++;
                subID++;
            }

            ImGui.EndTable();
        }

        ImGui.PopStyleVar();
        ImGui.PopID();
        ImGui.PopID();
        if (drawLabels)
        {
            ImGui.EndChild();
        }
    }

    public static void ValueEditor(FieldInfo field, object parent, bool drawLabels = false, int id = 0)
    {
        var fieldValue = field.GetValue(parent);
        switch (fieldValue)
        {
            case object stringValue when field.FieldType == typeof(string) || field.FieldType.Name == "Symbol":
                var str = stringValue.ToString();
                if (ImGui.InputText("", ref str, 128))
                {
                    // check if type is Symbol
                    if (fieldValue != null && fieldValue.GetType().Name == "Symbol")
                    {
                        field.SetValue(parent, new Symbol((uint)str.Length, str));
                    }
                    else
                    {
                        // if it's not a Symbol, just set the value to the text
                        field.SetValue(parent, new String(str));
                    }
                }

                if (fieldValue.GetType().Name == "Symbol")
                {
                    unsafe
                    {
                        if (ImGui.BeginDragDropTarget())
                        {
                            ImGuiPayloadPtr
                                payload = ImGui.AcceptDragDropPayload(
                                    "TreeEntryObject"); //TODO: maybe make a wrapper for drag and drop, it will be used later.
                            if (payload.NativePtr != null)
                            {
                                byte* payDataPtr = (byte*)payload.NativePtr->Data;
                                byte[] payData = new byte[payload.DataSize];
                                for (int i = 0; i < payload.DataSize; i++)
                                {
                                    payData[i] = payDataPtr[i];
                                }

                                var rebuildString = Encoding.UTF8.GetString(payData);
                                field.SetValue(parent, new Symbol((uint)rebuildString.Length, rebuildString));
                            }

                            payload = ImGui.AcceptDragDropPayload("TreeEntryDir");
                            if (payload.NativePtr != null)
                            {
                                byte* payDataPtr = (byte*)payload.NativePtr->Data;
                                byte[] payData = new byte[payload.DataSize];
                                for (int i = 0; i < payload.DataSize; i++)
                                {
                                    payData[i] = payDataPtr[i];
                                }

                                var rebuildString = Encoding.UTF8.GetString(payData);
                                field.SetValue(parent, new Symbol((uint)rebuildString.Length, rebuildString));
                            }

                            ImGui.EndDragDropTarget();
                        }
                    }
                }

                break;
            case List<Symbol> symbolsValue:
                var buttonSize = new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight());
                ImGui.BeginChild("symbols##" + field.GetHashCode(), new Vector2(0, 100f),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
                for (int i = 0; i < symbolsValue.Count; i++)
                {
                    var symbol = symbolsValue[i];
                    var stringValue = symbol.ToString();
                    if (ImGui.Button("-##" + i, buttonSize))
                    {
                        symbolsValue.Remove(symbol);
                        field.SetValue(parent, symbolsValue);
                        i--;
                        continue;
                    }

                    ImGui.SameLine();
                    if (ImGui.InputText("##" + i, ref stringValue, 128))
                    {
                        var symbolValue = new Symbol((uint)stringValue.Length, stringValue);
                        symbolsValue[i] = symbolValue;
                        field.SetValue(parent, symbolsValue);
                    }
                }

                if (ImGui.Button("+", buttonSize))
                {
                    symbolsValue.Add(new Symbol(0, ""));
                    field.SetValue(parent, symbolsValue);
                }

                ImGui.EndChild();
                unsafe
                {
                    if (ImGui.BeginDragDropTarget())
                    {
                        ImGuiPayloadPtr
                            payload = ImGui.AcceptDragDropPayload(
                                "TreeEntryObject"); //TODO: maybe make a wrapper for drag and drop, it will be used later.
                        if (payload.NativePtr != null)
                        {
                            byte* payDataPtr = (byte*)payload.NativePtr->Data;
                            byte[] payData = new byte[payload.DataSize];
                            for (int i = 0; i < payload.DataSize; i++)
                            {
                                payData[i] = payDataPtr[i];
                            }

                            var rebuildString = Encoding.UTF8.GetString(payData);
                            symbolsValue.Add(new Symbol((uint)rebuildString.Length, rebuildString));
                            field.SetValue(parent, symbolsValue);
                        }

                        payload = ImGui.AcceptDragDropPayload("TreeEntryDir");
                        if (payload.NativePtr != null)
                        {
                            byte* payDataPtr = (byte*)payload.NativePtr->Data;
                            byte[] payData = new byte[payload.DataSize];
                            for (int i = 0; i < payload.DataSize; i++)
                            {
                                payData[i] = payDataPtr[i];
                            }

                            var rebuildString = Encoding.UTF8.GetString(payData);
                            symbolsValue.Add(new Symbol((uint)rebuildString.Length, rebuildString));
                            field.SetValue(parent, symbolsValue);
                        }

                        ImGui.EndDragDropTarget();
                    }
                }

                break;
            case IEnumerable collection:
            {
                //ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.BeginChild("values##" + field.GetHashCode(), new Vector2(0, 50),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
                var i = 0;
                foreach (var value in collection)
                {
                    i++;
                    ImGui.PushID(i);
                    if (ImGui.CollapsingHeader(value.ToString() + "##" + i))
                    {
                        /*
                        Editing the object in a way that changes its ToString output causes the header
                        to change, changing the storage values for what ImGui uses to track open state.
                        This causes the header to close when an edit is made.
                        TODO: fix this!
                        */
                        ImGui.Indent();
                        if (ImGui.Button("Full View"))
                        {
                            Program.NavigateObject(value, true);
                        }

                        Draw(value, i, false);
                        ImGui.Unindent();
                    }

                    ImGui.PopID();
                }

                ImGui.EndChild();
                //ImGui.PopStyleVar();
                break;
            }
            case bool boolValue:
                if (ImGui.Checkbox("", ref boolValue))
                {
                    field.SetValue(parent, boolValue);
                }

                break;
            case Matrix matrixValue:
                ImGui.Text("(Matrix hidden)"); // TODO: nice gridded matrix editor
                break;
            case HmxColor3 colorValue:
            {
                var tempVec = new System.Numerics.Vector3(colorValue.r, colorValue.g, colorValue.b);
                if (ImGui.ColorEdit3("##color3", ref tempVec, ImGuiColorEditFlags.Float))
                {
                    colorValue.r = tempVec.X;
                    colorValue.g = tempVec.Y;
                    colorValue.b = tempVec.Z;
                    field.SetValue(parent, colorValue);
                }

                break;
            }
            case HmxColor4 colorValue:
            {
                var tempVec = new System.Numerics.Vector4(colorValue.r, colorValue.g, colorValue.b, colorValue.a);
                if (ImGui.ColorEdit4("##color3", ref tempVec, ImGuiColorEditFlags.Float))
                {
                    colorValue.r = tempVec.X;
                    colorValue.g = tempVec.Y;
                    colorValue.b = tempVec.Z;
                    colorValue.a = tempVec.W;
                    field.SetValue(parent, colorValue);
                }

                break;
            }
            case object primitiveValue when field.FieldType.IsPrimitive:
                DrawPrimitiveEdit(parent, primitiveValue, field);
                ImGui.SameLine();
                ImGui.TextDisabled(field.FieldType.Name);
                break;
            case object enumValue when field.FieldType.IsEnum:
                var values = GetCachedEnumValues(field.FieldType);
                var curValue = values.IndexOf(enumValue.ToString());
                if (ImGui.Combo("", ref curValue, values.ToArray(), values.Count))
                {
                    field.SetValue(parent, Enum.Parse(field.FieldType, values[curValue]));
                }

                break;
            case null when field.FieldType.FullName == "Symbol":
                ImGui.Text("(null symbol)");
                ImGui.SameLine();
                if (ImGui.Button("Create Symbol"))
                {
                    field.SetValue(parent, new Symbol(0, ""));
                }

                break;
            case object nestedObject when fieldValue != null:

                if (Settings.Current.HideNestedHMXObjectFields && !drawLabels && nestedObject.GetType() == typeof(ObjectFields))
                {
                    ImGui.TextDisabled("(nested fields hidden)");
                }
                else
                {
                    Draw(nestedObject, id + 1, false, ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.BordersOuter);
                }

                break;
            default:
                ImGui.TextWrapped(field.FieldType.FullName);
                ImGui.Text("(No editor)");
                break;
        }

        ImGui.PopID();
    }
}