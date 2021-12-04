using System.Text;

namespace AOEMods.Essence.Chunky;

public class ChunkyFileNode : ChunkyNode, IChunkyDataNode
{
    public ChunkyFileNode(ChunkHeader header, Stream dataStream)
        : base(header, dataStream)
    {
    }

    public IEnumerable<byte> GetData()
    {
        BinaryReader reader = new BinaryReader(dataStream, Encoding.ASCII, true);
        reader.BaseStream.Position = Header.DataPosition;
        return reader.ReadBytes(Header.Length);
    }
}
