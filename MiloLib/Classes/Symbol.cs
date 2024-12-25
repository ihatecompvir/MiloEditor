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
        uint length = reader.ReadUInt32();

        // sanity check on length, if this is something really high we are probably reading garbage
        // as there are no symbols of this length
        if (length > 512)
        {
            throw new InvalidDataException($"Symbol length is too high: {length}");
        }

        string value = reader.ReadUTF8((int)length);
        return new Symbol(length, value);
    }

    public static void Write(EndianWriter writer, Symbol lengthString)
    {
        writer.WriteUInt32(lengthString.length);

        writer.WriteBlock(Encoding.UTF8.GetBytes(lengthString.chars));
    }
}
