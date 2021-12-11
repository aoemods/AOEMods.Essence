using AOEMods.Essence.Chunky.Core;
using AOEMods.Essence.Chunky.Graph;
using SixLabors.ImageSharp.Formats;

namespace AOEMods.Essence.Chunky.RRTex;

public class RRTexReader : IRRTexReader
{
    public static IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        var reader = new ChunkyFileReader(rrtexStream);
        var fileHeader = reader.ReadChunkyFileHeader();
        var chunkyFile = ChunkyFile.FromStream(rrtexStream);
        var dataNodes = chunkyFile.RootNodes.OfType<IChunkyDataNode>();

        var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
        var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

        var tman = RRTexUtil.ReadDataTman(reader, tmanNode.Header);
        var mips = RRTexUtil.ReadDataTdat(reader, tdatNode.Header, tman, outputFormat, textureType);
        foreach (var mip in mips)
        {
            yield return mip;
        }
    }

    public static TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        var reader = new ChunkyFileReader(rrtexStream);
        var chunkyFile = ChunkyFile.FromStream(rrtexStream);
        var dataNodes = ((IChunkyFolderNode)chunkyFile.RootNodes.Single(node => node.Header.Name == "TSET")).Children
            .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "TXTR").Children
            .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "DXTC").Children
            .OfType<IChunkyDataNode>().ToArray();

        var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
        var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

        var tman = RRTexUtil.ReadDataTman(reader, tmanNode.Header);
        return RRTexUtil.ReadDataTdatLastMip(reader, tdatNode.Header, tman, outputFormat, textureType);
    }
}
