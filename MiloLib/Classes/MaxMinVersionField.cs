using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Defines the minimum version required for a field to be read.
    /// Milo reads the version to provide backwards compatibility for assets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MinVersionAttribute : Attribute
    {
        public int Version { get; }
        public MinVersionAttribute(int version) => Version = version;
    }

    /// <summary>
    /// Defines the maximum version required for a field to be read.
    /// Milo reads the version to provide backwards compatibility for assets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MaxVersionAttribute : Attribute
    {
        public int Version { get; }
        public MaxVersionAttribute(int version) => Version = version;
    }

    /// <summary>
    /// Defines the name of a field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class NameAttribute : Attribute
    {
        public string Value { get; }
        public NameAttribute(string value)
        {
            Value = value;
        }
    }

    /// <summary>
    ///  Defines the description of a field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        public string Value { get; }
        public DescriptionAttribute(string value)
        {
            Value = value;
        }
    }
}
