using System.Collections;
using System.Reflection;
using System.Text;
using IconFonts;
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
    private static Dictionary<Type, List<FieldInfo>> _fieldCache = new();
    private static Dictionary<Type, NameAttribute> _nameAttributeCache = new();
    private static Dictionary<MemberInfo, NameAttribute> _enumMemberNameCache = new();

    private static Dictionary<Type, DescriptionAttribute> _descriptionAttributeCache =
        new Dictionary<Type, DescriptionAttribute>();

    private static Dictionary<Type, List<string>> _enumValueCache = new Dictionary<Type, List<string>>();

    public static List<string> GetCachedEnumValues(Type enumType)
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
    
    private static NameAttribute? GetCachedNameAttribute(MemberInfo mInfo)
    {
        if (!_enumMemberNameCache.TryGetValue(mInfo, out NameAttribute attribute))
        {
            attribute = mInfo.GetCustomAttribute<NameAttribute>();
            _enumMemberNameCache[mInfo] = attribute;
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
                    if (Settings.Editing.HideFieldDescriptions)
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
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.Y, ImGui.GetStyle().ItemSpacing.Y));
                    if (ImGui.Button(FontAwesome5.Minus + "##" + i, buttonSize))
                    {
                        var lambda = () =>
                        {
                            symbolsValue.Remove(symbol);
                        };
                        Program.defferedActions.Add(lambda);
                    }
                    ImGui.SameLine();
                    if (i > 0)
                    {
                        if (ImGui.Button(FontAwesome5.ArrowUp + "##" + i, buttonSize))
                        {
                            var index = i;
                            var lambda = () =>
                            {
                                symbolsValue.Remove(symbol);
                                symbolsValue.Insert(index-1, symbol);
                            };
                            Program.defferedActions.Add(lambda);
                        }
                    }
                    else
                    {
                        ImGui.InvisibleButton("disableup##" + i, buttonSize);
                    }
                    ImGui.SameLine();
                    if (i < symbolsValue.Count-1)
                    {
                        if (ImGui.Button(FontAwesome5.ArrowDown + "##" + i, buttonSize))
                        {
                            var index = i;
                            var lambda = () =>
                            {
                                symbolsValue.Remove(symbol);
                                symbolsValue.Insert(index+1, symbol);
                            };
                            Program.defferedActions.Add(lambda);
                        }
                    }
                    else
                    {
                        ImGui.InvisibleButton("disabledown##" + i, buttonSize);
                    }
                    ImGui.SameLine();
                    ImGui.PopStyleVar();
                    
                    if (ImGui.InputText("##" + i, ref stringValue, 128))
                    {
                        var symbolValue = new Symbol((uint)stringValue.Length, stringValue);
                        symbolsValue[i] = symbolValue;
                        field.SetValue(parent, symbolsValue);
                    }
                }

                if (ImGui.Button(FontAwesome5.Plus, buttonSize))
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
                int? length = null;
                IList? list = null;
                if (collection is IList _list)
                {
                    list = _list;
                    length = list.Count;
                }

                Type collectionType = null;
                if (field.FieldType.GetGenericArguments().Length > 0)
                {
                    collectionType = field.FieldType.GetGenericArguments().First();
                    ImGui.Text("List: " + collectionType.Name);
                    if (length != null)
                    {
                        ImGui.Text(length.Value + " entries");
                    }
                }
                var collectionButtonSize = new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight());
                
                //ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.BeginChild("values##" + field.GetHashCode(), new Vector2(0, 50),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
                var i = 0;
                foreach (var value in collection)
                {
                    i++;
                    ImGui.PushID(i);
                    if (list != null)
                    {
                        if (ImGui.Button(FontAwesome5.Minus + "##" + i, collectionButtonSize))
                        {
                            var lambda = () =>
                            {
                                list.Remove(value);
                            };
                            Program.defferedActions.Add(lambda);
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.TreeNode(value.ToString() + "###" + i))
                    {
                        //ImGui.Indent();
                        if (ImGui.Button("Full View"))
                        {
                            Program.NavigateObject(value, true);
                        }

                        Draw(value, i, false);
                        //ImGui.Unindent();
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }

                if (collectionType != null)
                {
                    if (ImGui.Button(FontAwesome5.Plus, collectionButtonSize))
                    {
                        var constructor = collectionType.GetConstructor([]);
                        if (constructor == null)
                        {
                            Program.ShowNotifyPrompt("Cannot create new object in list; object has no parameterless constructor.", "Error");
                        }
                        else
                        {
                            var obj = constructor.Invoke([]);
                            list?.Add(obj);
                        }
                    }
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
                bool edited = false;
                var row1 = new System.Numerics.Vector4(matrixValue.m11, matrixValue.m21, matrixValue.m31, matrixValue.m41);
                var row2 = new System.Numerics.Vector4(matrixValue.m12, matrixValue.m22, matrixValue.m32, matrixValue.m42);
                var row3 = new System.Numerics.Vector4(matrixValue.m13, matrixValue.m23, matrixValue.m33, matrixValue.m43);
                ImGui.PushItemWidth(-ImGui.GetStyle().CellPadding.X);
                edited |= ImGui.InputFloat4("##row1", ref row1);
                edited |= ImGui.InputFloat4("##row2", ref row2);
                edited |= ImGui.InputFloat4("##row3", ref row3);
                ImGui.PopItemWidth();
                if (edited)
                {
                    matrixValue.m11 = row1.X;
                    matrixValue.m21 = row1.Y;
                    matrixValue.m31 = row1.Z;
                    matrixValue.m41 = row1.W;
                    
                    matrixValue.m12 = row2.X;
                    matrixValue.m22 = row2.Y;
                    matrixValue.m32 = row2.Z;
                    matrixValue.m42 = row2.W;
                    
                    matrixValue.m13 = row3.X;
                    matrixValue.m23 = row3.Y;
                    matrixValue.m33 = row3.Z;
                    matrixValue.m43 = row3.W;
                    field.SetValue(parent, matrixValue);
                }
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
                var niceValues = new string[values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    var member = field.FieldType.GetMember(values[i]).FirstOrDefault(m => m.DeclaringType == field.FieldType);
                    var nameAtt = GetCachedNameAttribute(member);
                    if (nameAtt != null)
                    {
                        niceValues[i] = nameAtt.Value;
                    }
                    else
                    {
                        niceValues[i] = values[i];
                    }
                }
                var curValue = values.IndexOf(enumValue.ToString());
                if (ImGui.Combo("", ref curValue, niceValues, values.Count))
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

                if (Settings.Editing.HideNestedHMXObjectFields && !drawLabels && nestedObject.GetType() == typeof(ObjectFields))
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