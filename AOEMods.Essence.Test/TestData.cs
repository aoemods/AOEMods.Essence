using System.Linq;

namespace AOEMods.Essence.Test;

public static class TestData
{
    public static byte[] ValidChunkHeaderEmptyContent { get; } = new byte[]
    {
        0x44, 0x41, 0x54, 0x41, // Type (DATA)
        0x54, 0x45, 0x53, 0x54, // Name (TEST)
        0x0A, 0x00, 0x00, 0x00, // Version (10)
        0x00, 0x00, 0x00, 0x00, // Length (0)
        0x03, 0x00, 0x00, 0x00, // Path length (3)
        0x41, 0x42, 0x43 // Path (ABC)
    };

    public static byte[] ValidChunkyFileHeader { get; } = new byte[]
    {
        0, 1, 2, 3, 4, 5, 6, 7, // Magic (16 bytes)
        8, 9, 10, 11, 12, 13, 14, 15,
        10, 0, 0, 0, // Version
        1, 0, 0, 0 // Platform
    };

    public static byte[] ValidChunkyFileSingleData { get; } = ValidChunkyFileHeader.Concat(ValidChunkHeaderEmptyContent).ToArray();
}
