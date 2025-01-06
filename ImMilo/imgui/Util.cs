using ImGuiNET;

namespace ImMilo.imgui;

public class Util
{
    public static unsafe bool InputUInt(string label, ref uint value)
    {
        fixed (uint* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U32, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputShort(string label, ref short value)
    {
        fixed (short* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.S16, (IntPtr)ptr);
        }
    }

    public static unsafe bool InputUShort(string label, ref ushort value)
    {
        fixed (ushort* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U16, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputLong(string label, ref long value)
    {
        fixed (long* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.S64, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputULong(string label, ref ulong value)
    {
        fixed (ulong* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U64, (IntPtr)ptr);
        }
    }
    
    public static unsafe bool InputByte(string label, ref byte value)
    {
        fixed (byte* ptr = &value)
        {
            return ImGui.InputScalar(label, ImGuiDataType.U8, (IntPtr)ptr);
        }
    }
    
}