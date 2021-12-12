using AOEMods.Essence.Chunky.Core;
using System.IO;
using System.Linq;
using Xunit;

namespace AOEMods.Essence.Test;

public class ChunkyFileReaderTest
{
    [Fact]
    public void TestReadChunkHeader_ValidChunkHeaderEmptyContent_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(TestData.ValidChunkHeaderEmptyContent));
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
        ChunkyFileReader reader = new(new MemoryStream(TestData.ValidChunkHeaderEmptyContent));

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
        ChunkyFileReader reader = new(new MemoryStream(TestData.ValidChunkHeaderEmptyContent));
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
        ChunkyFileReader reader = new(new MemoryStream(TestData.ValidChunkyFileHeader));
        var fileHeader = reader.ReadChunkyFileHeader();
        Assert.All(Enumerable.Range(0, 16), i => Assert.Equal(TestData.ValidChunkyFileHeader[i], fileHeader.Magic[i]));
        Assert.Equal(10, fileHeader.Version);
        Assert.Equal(1, fileHeader.Platform);
    }

    [Fact]
    public void TestReadChunkyFileHeaderAndReadChunkHeaders_ValidChunkyFileSingleDataEmptyContent_ParsedCorrectly()
    {
        ChunkyFileReader reader = new(new MemoryStream(TestData.ValidChunkyFileSingleData));

        var fileHeader = reader.ReadChunkyFileHeader();
        Assert.All(Enumerable.Range(0, 16), i => Assert.Equal(TestData.ValidChunkyFileHeader[i], fileHeader.Magic[i]));
        Assert.Equal(10, fileHeader.Version);
        Assert.Equal(1, fileHeader.Platform);

        var headers = reader.ReadChunkHeaders();
        var header = headers.Single();
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);
    }
}
