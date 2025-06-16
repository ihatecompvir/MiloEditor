using MiloLib.Utils;
using System.Text;

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
        return chars;
    }

    public static Symbol Read(EndianReader reader)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        uint length = reader.ReadUInt32();

        if (length > 512)
        {
            throw new InvalidDataException($"Symbol length is too high: {length}");
        }

        // use windows-1252 encoding to support extended characters
        string value = reader.ReadBytesWithEncoding((int)length, Encoding.GetEncoding("Windows-1252"));
        return new Symbol(length, value);
    }

    public static void Write(EndianWriter writer, Symbol lengthString)
    {
        byte[] bytes = Encoding.GetEncoding("Windows-1252").GetBytes(lengthString.chars);
        writer.WriteUInt32((uint)bytes.Length);
        writer.WriteBlock(bytes);
    }
}
