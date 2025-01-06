using System.Reflection;
using ImGuiNET;
using ImMilo.imgui;
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
    private static Dictionary<Type, DescriptionAttribute> _descriptionAttributeCache = new Dictionary<Type, DescriptionAttribute>();


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
                fields.AddRange(current.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy));
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

    private static void DrawPrimitiveEdit(object obj, object primitiveValue, FieldInfo field)
    {
        bool isNumber = field.FieldType == typeof(int) || field.FieldType == typeof(uint) ||
                        field.FieldType == typeof(short) || field.FieldType == typeof(ushort) ||
                        field.FieldType == typeof(long) || field.FieldType == typeof(ulong) ||
                        field.FieldType == typeof(float) || field.FieldType == typeof(double) ||
                        field.FieldType == typeof(decimal);

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
                        Console.WriteLine("Warning: Decimal field " + field.Name + " has a value that is truncated when converted to Double.");
                    }
                    var tempDouble = (double)d;
                    changed = ImGui.InputDouble("", ref tempDouble);
                    newVal = tempDouble; //Not sure how to implement decimal properly. Just using a double.
                    break;
            }

            if (changed)
            {
                field.SetValue(obj, newVal);
            }
        }
        
    }

    public static void Draw(object obj, int id = 0, bool drawLabels = true, ImGuiTableFlags toggleFlags = ImGuiTableFlags.None)
    {
        
        Type objType = obj.GetType();
        
        var revisionField = obj.GetType().GetField("revision", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
            ImGui.BeginChild("editor values##"+objType.Name);
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
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    ImGui.TextWrapped(description);
                    ImGui.PopStyleVar();
                }
                
                ImGui.TableSetColumnIndex(1);
                var fieldValue = field.GetValue(obj);
                switch (fieldValue)
                {
                    case object stringValue when field.FieldType == typeof(string) || field.FieldType.Name == "Symbol":
                        var str = stringValue.ToString();
                        if (ImGui.InputText("", ref str, 128))
                        {
                            // check if type is Symbol
                            if (fieldValue != null && fieldValue.GetType().Name == "Symbol")
                            {
                                field.SetValue(obj, new Symbol((uint)str.Length, str));
                            }
                            else
                            {
                                // if it's not a Symbol, just set the value to the text
                                field.SetValue(obj, new String(str));
                            }
                        }

                        break;
                    case bool boolValue:
                        if (ImGui.Checkbox("", ref boolValue))
                        {
                            field.SetValue(obj, boolValue);
                        }

                        break;
                    case Matrix matrixValue:
                        ImGui.Text("(Matrix hidden)"); // TODO nice gridded matrix editor
                        break;
                    case object primitiveValue when field.FieldType.IsPrimitive:
                        DrawPrimitiveEdit(obj, primitiveValue, field);
                        ImGui.SameLine();
                        ImGui.TextDisabled(field.FieldType.Name);
                        break;
                    case object nestedObject when fieldValue != null:
                        
                        Draw(nestedObject, id+1, false, ImGuiTableFlags.NoPadOuterX);
                        
                        break;
                    default:
                        ImGui.Text(field.FieldType.Name);
                        ImGui.Text("(No editor)");
                        break;
                }
                ImGui.PopID();
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
}