using System.Security.Cryptography;
using MiloLib.Classes;

namespace ImMilo;

/// <summary>
/// Persistent settings for the user. TODO: serialize this.
/// </summary>
[Name("ImMilo Settings"), Description("These do not save yet!")]
public class Settings
{

    [Name("UI Scale"), Description("Note: Text will look blurry/odd until you restart.")]
    public float UIScale = 1.0f;
    
    public class FontSettings
    {
        [Name("Font Size")]
        public int FontSize = 18;
        [Name("Icon Size"), Description("Size of the icons used in the scene tree.")]
        public int IconSize = 24;

        public enum FontType
        {
            ImGuiDefault,
            TTFBuiltIn,
            TTFCustom
        }
        
        [Name("Font Type")]
        public FontType Font = FontType.TTFBuiltIn;

        [Name("Custom Font Path"), Description("Absolute path to the custom font file. Make sure Font Type is set to TTFCustom.")]
        public string CustomFontFilePath = "";

        public override bool Equals(object? obj) => this.Equals(obj as FontSettings);
        public bool Equals(FontSettings? other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }
            return (this.FontSize == other.FontSize) &&
                   (this.IconSize == other.IconSize) &&
                   (this.Font == other.Font) &&
                   (this.CustomFontFilePath == other.CustomFontFilePath);
        }
        
        public override int GetHashCode() => (this.FontSize, this.IconSize, this.Font, this.CustomFontFilePath).GetHashCode();
        
        public static bool operator ==(FontSettings lhs, FontSettings rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FontSettings lhs, FontSettings rhs) => !(lhs == rhs);
        
        public FontSettings Clone() => (FontSettings)MemberwiseClone();
    }
    
    [Name("Font Settings"), Description("Changing these requires a restart.")]
    public FontSettings fontSettings = new FontSettings();

    public enum Theme
    {
        Dark,
        Light,
        ImGuiClassic
    }
    [Name("Theme")]
    public Theme useTheme = Theme.Dark;
    
    
    [Name("Hide Field Descriptions"), Description("Hides the field descriptions (like these) in the editor panel.")]
    public bool HideFieldDescriptions = false;
    [Name("Hide Nested Hmx::Object Fields"), Description("Hides the \"Object Fields\" area if an object is nested in another. Typically these fields are redundant and only matter for the parent object.")]
    public bool HideNestedHMXObjectFields = true;
    
    /// <summary>
    /// The instance of Settings that is edited by the user.
    /// Use for things that can change at runtime.
    /// </summary>
    public static Settings Editing = new Settings();
    /// <summary>
    /// An internal copy of Settings that only contains the settings loaded from disk.
    /// Use for things that cannot change at runtime.
    /// </summary>
    public static Settings Loaded = new Settings();

    public float ScaledIconSize => Loaded.fontSettings.IconSize*Editing.UIScale;

    public override bool Equals(object? obj) => this.Equals(obj as Settings);
    
    public bool Equals(Settings? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Object.ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.GetType() != other.GetType())
        {
            return false;
        }

        return (this.UIScale == other.UIScale) &&
               (this.useTheme == other.useTheme) &&
               (this.HideFieldDescriptions == other.HideFieldDescriptions) &&
               (this.HideNestedHMXObjectFields == other.HideNestedHMXObjectFields) &&
               (this.fontSettings == other.fontSettings);
    }
    
    public static bool operator ==(Settings lhs, Settings rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }
    public static bool operator !=(Settings lhs, Settings rhs) => !(lhs == rhs);

    public Settings Clone()
    {
        var clone = (Settings)MemberwiseClone();
        clone.fontSettings = fontSettings.Clone();
        return clone;
    }
}