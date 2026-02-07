using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MiloLib.Assets;
using MiloLib.Classes;

namespace MiloLib.Tests;

/// <summary>
/// Helper class that generates fully populated "fake" instances of any given Type for testing purposes.
/// Essentially this is used to "fuzz" asset classes with random data to ensure that the serialization and deserialization logic can handle a wide variety of inputs without throwing exceptions and alsot est for correctness.
/// </summary>
public class AssetFuzzer
{
    private readonly Random _random;
    private int _recursionDepth = 0;
    private const int MAX_RECURSION_DEPTH = 3;

    /// <summary>
    /// Creates a new AssetFuzzer with an optional seed for deterministic testing.
    /// </summary>
    public AssetFuzzer(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Creates a fully populated instance of the given type.
    /// </summary>
    public object? Create(Type type)
    {
        if (_recursionDepth > MAX_RECURSION_DEPTH)
        {
            return GetDefaultValue(type);
        }

        _recursionDepth++;
        try
        {
            return CreateInternal(type);
        }
        finally
        {
            _recursionDepth--;
        }
    }

    private object? CreateInternal(Type type)
    {
        // Handle nullables
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }

        // Handle primitives
        if (type.IsPrimitive)
        {
            return CreatePrimitive(type);
        }

        // Handle strings
        if (type == typeof(string))
        {
            return GenerateRandomString();
        }

        // Handle enums
        if (type.IsEnum)
        {
            return CreateEnum(type);
        }

        // Handle MiloLib.Classes.Symbol
        if (type == typeof(Symbol))
        {
            string randomStr = GenerateRandomString();
            return new Symbol((uint)randomStr.Length, randomStr);
        }

        // Handle MiloLib.Classes.Matrix
        if (type == typeof(Matrix))
        {
            var matrix = new Matrix();
            matrix.m11 = _random.NextSingle();
            matrix.m12 = _random.NextSingle();
            matrix.m13 = _random.NextSingle();
            matrix.m21 = _random.NextSingle();
            matrix.m22 = _random.NextSingle();
            matrix.m23 = _random.NextSingle();
            matrix.m31 = _random.NextSingle();
            matrix.m32 = _random.NextSingle();
            matrix.m33 = _random.NextSingle();
            matrix.m41 = _random.NextSingle();
            matrix.m42 = _random.NextSingle();
            matrix.m43 = _random.NextSingle();
            return matrix;
        }

        // Handle MiloLib.Classes.Matrix3
        if (type == typeof(Matrix3))
        {
            var matrix = new Matrix3();
            matrix.m11 = _random.NextSingle();
            matrix.m12 = _random.NextSingle();
            matrix.m13 = _random.NextSingle();
            matrix.m21 = _random.NextSingle();
            matrix.m22 = _random.NextSingle();
            matrix.m23 = _random.NextSingle();
            matrix.m31 = _random.NextSingle();
            matrix.m32 = _random.NextSingle();
            matrix.m33 = _random.NextSingle();
            return matrix;
        }

        // Handle MiloLib.Classes.Vector2
        if (type == typeof(Vector2))
        {
            return new Vector2(_random.NextSingle(), _random.NextSingle());
        }

        // Handle MiloLib.Classes.Vector3
        if (type == typeof(Vector3))
        {
            return new Vector3(_random.NextSingle(), _random.NextSingle(), _random.NextSingle());
        }

        // Handle MiloLib.Classes.Vector4
        if (type == typeof(Vector4))
        {
            var vec4 = new Vector4(_random.NextSingle(), _random.NextSingle(), _random.NextSingle());
            vec4.w = _random.NextSingle();
            return vec4;
        }

        // Handle MiloLib.Classes.HmxColor3
        if (type == typeof(HmxColor3))
        {
            return new HmxColor3(_random.NextSingle(), _random.NextSingle(), _random.NextSingle());
        }

        // Handle MiloLib.Classes.HmxColor4
        if (type == typeof(HmxColor4))
        {
            return new HmxColor4(_random.NextSingle(), _random.NextSingle(), _random.NextSingle(), _random.NextSingle());
        }

        // Handle MiloLib.Classes.Rect
        if (type == typeof(Rect))
        {
            var rect = new Rect();
            rect.x = _random.NextSingle();
            rect.y = _random.NextSingle();
            rect.width = _random.NextSingle();
            rect.height = _random.NextSingle();
            return rect;
        }

        // Handle MiloLib.Classes.Sphere
        if (type == typeof(Sphere))
        {
            var sphere = new Sphere();
            sphere.x = _random.NextSingle();
            sphere.y = _random.NextSingle();
            sphere.z = _random.NextSingle();
            sphere.radius = _random.NextSingle();
            return sphere;
        }

        // Handle MiloLib.Assets.ObjectFields.DTBParent struct
        if (type.FullName == "MiloLib.Assets.ObjectFields+DTBParent" || 
            (type.Name == "DTBParent" && type.Namespace == "MiloLib.Assets"))
        {
            var dtbParent = Activator.CreateInstance(type);
            
            // Set hasTree to false so children won't be accessed during Write
            var hasTreeField = type.GetField("hasTree");
            if (hasTreeField != null)
            {
                hasTreeField.SetValue(dtbParent, false);
            }
            
            // Initialize children to empty list to prevent NullReferenceException
            var childrenField = type.GetField("children");
            if (childrenField != null)
            {
                // Get the generic type argument for List<DTBNode>
                var childrenListType = typeof(List<>);
                var nodeType = type.Assembly.GetType("MiloLib.Assets.ObjectFields+DTBNode");
                if (nodeType != null)
                {
                    var genericListType = childrenListType.MakeGenericType(nodeType);
                    var emptyList = Activator.CreateInstance(genericListType);
                    childrenField.SetValue(dtbParent, emptyList);
                }
            }
            
            return dtbParent;
        }

        // Handle arrays
        if (type.IsArray)
        {
            return CreateArray(type);
        }

        // Handle generic collections (List<T>, etc.)
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>))
            {
                return CreateList(type);
            }
        }

        // Handle classes (including asset classes)
        if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
        {
            return CreateClass(type);
        }

        return GetDefaultValue(type);
    }

    private object CreatePrimitive(Type type)
    {
        if (type == typeof(bool))
            return _random.Next(2) == 1;
        if (type == typeof(byte))
            return (byte)_random.Next(256);
        if (type == typeof(sbyte))
            return (sbyte)_random.Next(-128, 128);
        if (type == typeof(short))
            return (short)_random.Next(short.MinValue, short.MaxValue);
        if (type == typeof(ushort))
            return (ushort)_random.Next(ushort.MaxValue);
        if (type == typeof(int))
            return _random.Next();
        if (type == typeof(uint))
            return (uint)_random.NextInt64(0, uint.MaxValue);
        if (type == typeof(long))
            return _random.NextInt64();
        if (type == typeof(ulong))
            return (ulong)_random.NextInt64(0, long.MaxValue);
        if (type == typeof(float))
            return _random.NextSingle();
        if (type == typeof(double))
            return _random.NextDouble();
        if (type == typeof(decimal))
            return (decimal)_random.NextDouble();
        if (type == typeof(char))
            return (char)_random.Next(32, 127);

        return GetDefaultValue(type);
    }

    private object CreateEnum(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        if (values.Length == 0)
            return GetDefaultValue(enumType);
        
        return values.GetValue(_random.Next(values.Length))!;
    }

    private string GenerateRandomString()
    {
        int length = _random.Next(1, 20);
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append((char)_random.Next('A', 'Z' + 1));
        }
        return sb.ToString();
    }

    private object? CreateArray(Type arrayType)
    {
        Type elementType = arrayType.GetElementType()!;
        int length = _random.Next(1, 5);
        var array = Array.CreateInstance(elementType, length);
        
        for (int i = 0; i < length; i++)
        {
            var element = Create(elementType);
            array.SetValue(element, i);
        }
        
        return array;
    }

    private object? CreateList(Type listType)
    {
        Type elementType = listType.GetGenericArguments()[0];
        var list = (IList)Activator.CreateInstance(listType)!;
        
        int count = _random.Next(1, 5);
        for (int i = 0; i < count; i++)
        {
            var element = Create(elementType);
            if (element != null)
            {
                list.Add(element);
            }
        }
        
        return list;
    }

    private object? CreateClass(Type type)
    {
        // Try to instantiate the class
        object? instance = null;

        // First, try the standard Milo constructor: (ushort revision, ushort altRevision)
        var revisionConstructor = type.GetConstructor(new[] { typeof(ushort), typeof(ushort) });
        if (revisionConstructor != null)
        {
            instance = revisionConstructor.Invoke(new object[] { (ushort)0, (ushort)0 });
        }
        else
        {
            // Try with just ushort revision
            var singleRevisionConstructor = type.GetConstructor(new[] { typeof(ushort) });
            if (singleRevisionConstructor != null)
            {
                instance = singleRevisionConstructor.Invoke(new object[] { (ushort)0 });
            }
            else
            {
                // Try parameterless constructor
                var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
                if (parameterlessConstructor != null)
                {
                    instance = parameterlessConstructor.Invoke(Array.Empty<object>());
                }
                else
                {
                    // Fall back to Activator.CreateInstance
                    try
                    {
                        instance = Activator.CreateInstance(type);
                    }
                    catch
                    {
                        return GetDefaultValue(type);
                    }
                }
            }
        }

        if (instance == null)
        {
            return GetDefaultValue(type);
        }

        // Populate all fields (public, private, protected, and from base classes)
        Type currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
            foreach (var field in fields)
            {
                // Skip readonly fields that might cause issues
                if (field.IsInitOnly)
                    continue;

                try
                {
                    // Check if field is an array that's already initialized
                    var existingValue = field.GetValue(instance);
                    if (existingValue != null && field.FieldType.IsArray)
                    {
                        // Field already has an array - populate its elements instead of replacing it
                        var existingArray = existingValue as Array;
                        if (existingArray != null)
                        {
                            for (int i = 0; i < existingArray.Length; i++)
                            {
                                var elementType = field.FieldType.GetElementType();
                                if (elementType != null)
                                {
                                    var elementValue = Create(elementType);
                                    if (elementValue != null)
                                    {
                                        existingArray.SetValue(elementValue, i);
                                    }
                                }
                            }
                            // Don't replace the array, just populate it
                            continue;
                        }
                    }
                    
                    var fieldValue = Create(field.FieldType);
                    if (fieldValue != null)
                    {
                        field.SetValue(instance, fieldValue);
                    }
                }
                catch
                {
                    // If we can't set the field, skip it
                }
            }

            currentType = currentType.BaseType;
        }

        return instance;
    }

    private object? GetDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}

