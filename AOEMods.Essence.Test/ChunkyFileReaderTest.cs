using AOEMods.Essence.Chunky.Core;
using System.IO;
using System.Linq;
using Xunit;

namespace AOEMods.Essence.Test;

public class ChunkyFileReaderTest
{
    private static readonly byte[] validChunkHeaderEmptyContent = new byte[]
    {
        0x44, 0x41, 0x54, 0x41, // Type (DATA)
        0x54, 0x45, 0x53, 0x54, // Name (TEST)
        0x0A, 0x00, 0x00, 0x00, // Version (10)
        0x00, 0x00, 0x00, 0x00, // Length (0)
        0x03, 0x00, 0x00, 0x00, // Path length (3)
        0x41, 0x42, 0x43 // Path (ABC)
    };

    private static readonly byte[] validChunkyFileHeader = new byte[]
    {
        0, 1, 2, 3, 4, 5, 6, 7, // Magic (16 bytes)
        8, 9, 10, 11, 12, 13, 14, 15,
        10, 0, 0, 0, // Version
        1, 0, 0, 0 // Platform
    };

    private static readonly byte[] validChunkyFileSingleData = validChunkyFileHeader.Concat(validChunkHeaderEmptyContent).ToArray();

    [Fact]
    public void TestReadChunkHeader_ValidChunkHeaderEmptyContent_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(validChunkHeaderEmptyContent));
        var header = reader.ReadChunkHeader();

        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);
    }

    [Fact]
    public void TestReadChunkHeaders_ValidChunkHeaderEmptyContent_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(validChunkHeaderEmptyContent));
        var headers = reader.ReadChunkHeaders();
        var header = headers.Single();
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);
    }

    [Fact]
    public void TestReadChunkHeaders_ValidChunkHeaderEmptyContentIterateTwice_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(validChunkHeaderEmptyContent));
        var headers = reader.ReadChunkHeaders();

        var header = headers.Single();
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);

        var header2 = headers.Single();
        Assert.Equal("DATA", header2.Type);
        Assert.Equal("TEST", header2.Name);
        Assert.Equal(10, header2.Version);
        Assert.Equal(0, header2.Length);
        Assert.Equal("ABC", header2.Path);
    }

    [Fact]
    public void TestReadChunkyFileHeader_ValidChunkyFileHeader_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(validChunkyFileHeader));
        var fileHeader = reader.ReadChunkyFileHeader();
        Assert.All(Enumerable.Range(0, 16), i => Assert.Equal(validChunkyFileHeader[i], fileHeader.Magic[i]));
        Assert.Equal(10, fileHeader.Version);
        Assert.Equal(1, fileHeader.Platform);
    }
}
