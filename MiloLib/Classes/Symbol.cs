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
        if(String.IsNullOrEmpty(chars)) {
            return "N/A";
        }
        else return chars;
    }

    // TODO: move the register provider to a more global place, this is shit

    public static Symbol Read(EndianReader reader)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        uint length = reader.ReadUInt32();

        if (length > 512)
        {
            throw new InvalidDataException($"Symbol length is too high: {length}");
        }

        // use Latin1 encoding to support extended characters
        string value = reader.ReadBytesWithEncoding((int)length, Encoding.Latin1);
        return new Symbol(length, value);
    }

    public static void Write(EndianWriter writer, Symbol lengthString)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] bytes = Encoding.Latin1.GetBytes(lengthString.chars);
        writer.WriteUInt32((uint)bytes.Length);
        writer.WriteBlock(bytes);
    }
}
