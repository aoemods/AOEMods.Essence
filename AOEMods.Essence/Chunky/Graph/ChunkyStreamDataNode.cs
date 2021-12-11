using System.Text;

namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file DATA chunk node with data read from a stream when requested.
/// </summary>
public class ChunkyStreamDataNode : ChunkyNode, IChunkyDataNode
{
    /// <summary>
    /// Initializes a ChunkyStreamDataNode from the chunk's header and a data stream.
    /// </summary>
    /// <param name="header">Chunk header of the chunk.</param>
    /// <param name="dataStream">Stream containing the chunk's data.</param>
    public ChunkyStreamDataNode(ChunkHeader header, Stream dataStream)
        : base(header, dataStream)
    {
    }

    /// <summary>
    /// Gets the data of the DATA chunk from the stream.
    /// </summary>
    /// <returns>Data of the DATA chunk.</returns>
    public IEnumerable<byte> GetData()
    {
        BinaryReader reader = new BinaryReader(dataStream, Encoding.ASCII, true);
        reader.BaseStream.Position = Header.DataPosition;
        return reader.ReadBytes(Header.Length);
    }
}
