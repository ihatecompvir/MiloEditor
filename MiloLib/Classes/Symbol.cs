using MiloLib.Utils;
using System.Text;
using System;

public class Symbol
{
    private uint length;
    private string chars;

    public string value => chars;

    public Symbol(uint length, string chars)
    {
        this.length = length;
        this.chars = chars;
    }

    public static implicit operator Symbol(string input)
    {
        return new Symbol((uint)input.Length, input);
    }

    public static implicit operator string(Symbol lengthString)
    {
        return lengthString.chars;
    }

    public override string ToString()
    {
        if (String.IsNullOrEmpty(chars))
        {
            return "N/A";
        }
        else return chars;
    }

    public static Symbol Read(EndianReader reader)
    {
        uint length = reader.ReadUInt32();

        if (length > 512)
            throw new InvalidDataException($"Symbol length too high: {length}");

        byte[] bytes = reader.ReadBlock((int)length);
        string value = Encoding.Latin1.GetString(bytes);

        return new Symbol(length, value);
    }

    public static void Write(EndianWriter writer, Symbol lengthString)
    {
        byte[] bytes = Encoding.Latin1.GetBytes(lengthString.chars);
        writer.WriteUInt32((uint)bytes.Length);
        writer.WriteBlock(bytes);
    }
}
