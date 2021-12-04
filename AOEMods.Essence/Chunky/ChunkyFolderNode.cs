using System.Text;

namespace AOEMods.Essence.Chunky;

public class ChunkyFolderNode : ChunkyNode, IChunkyFolderNode
{
    public IEnumerable<IChunkyNode> Children
    {
        get
        {
            ChunkyFileReader reader = new(dataStream, Encoding.ASCII, true);
            reader.BaseStream.Position = Header.DataPosition;
            foreach (var node in reader.ReadNodes(Header.Length))
            {
                yield return node;
            }
        }
    }

    public ChunkyFolderNode(ChunkHeader header, Stream dataStream)
        : base(header, dataStream)
    {
    }
}
