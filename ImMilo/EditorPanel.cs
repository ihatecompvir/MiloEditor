using System.Reflection;
using ImGuiNET;
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
    }
}