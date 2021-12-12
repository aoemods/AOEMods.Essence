using AOEMods.Essence.Chunky.Graph;
using System.IO;
using System.Linq;
using Xunit;

namespace AOEMods.Essence.Test;

public class ChunkyFileTest
{
    [Fact]
    public void TestFromStream_ValidChunkyFileSingleDataEmptyContent_RootNodeHeaderParsedCorrectly()
    {
        var chunky = ChunkyFile.FromStream(new MemoryStream(TestData.ValidChunkyFileSingleData));

        var header = chunky.RootNodes.Single().Header;
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);
    }

    [Fact]
    public void TestFromStream_ValidChunkyFileSingleDataEmptyContentReadRootNodeAndHeaderTwice_RootNodeHeaderParsedCorrectly()
    {
        var chunky = ChunkyFile.FromStream(new MemoryStream(TestData.ValidChunkyFileSingleData));

        var header = chunky.RootNodes.Single().Header;
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);

        var header2 = chunky.RootNodes.Single().Header;
        Assert.Equal("DATA", header2.Type);
        Assert.Equal("TEST", header2.Name);
        Assert.Equal(10, header2.Version);
        Assert.Equal(0, header2.Length);
        Assert.Equal("ABC", header2.Path);
    }

    [Fact]
    public void TestFromStream_ValidChunkyFileSingleDataEmptyContent_FileHeaderParsedCorrectly()
    {
        var chunky = ChunkyFile.FromStream(new MemoryStream(TestData.ValidChunkyFileSingleData));
        var fileHeader = chunky.Header;
        Assert.All(Enumerable.Range(0, 16), i => Assert.Equal(TestData.ValidChunkyFileHeader[i], fileHeader.Magic[i]));
        Assert.Equal(10, fileHeader.Version);
        Assert.Equal(1, fileHeader.Platform);
    }

    [Fact]
    public void TestReadFromStream_ValidChunkyFileSingleDataEmptyContent_FileAndRootNodeHeaderParsedCorrectly()
    {
        var chunky = ChunkyFile.FromStream(new MemoryStream(TestData.ValidChunkyFileSingleData));
        var fileHeader = chunky.Header;

        Assert.All(Enumerable.Range(0, 16), i => Assert.Equal(TestData.ValidChunkyFileHeader[i], fileHeader.Magic[i]));
        Assert.Equal(10, fileHeader.Version);
        Assert.Equal(1, fileHeader.Platform);

        var header = chunky.RootNodes.Single().Header;
        Assert.Equal("DATA", header.Type);
        Assert.Equal("TEST", header.Name);
        Assert.Equal(10, header.Version);
        Assert.Equal(0, header.Length);
        Assert.Equal("ABC", header.Path);
    }
}
