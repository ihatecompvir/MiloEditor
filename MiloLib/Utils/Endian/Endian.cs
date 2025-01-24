// originally taken from https://github.com/XboxChaos/Assembly, thanks for originally writing this code
// license in Utils/Endian/LICENSE.md

using MiloLib.Classes;

namespace MiloLib.Utils
{
    public enum Endian
    {
        [Name("Big Endian (RB1 and later)")]
        BigEndian, // MSB -> LSB
        [Name("Little Endian (GH2 and earlier)")]
        LittleEndian // LSB -> MSB
    }
}