namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file FOLD node containing more chunk nodes.
/// </summary>
public class ChunkyFolderNode : ChunkyNode, IChunkyFolderNode
{
    /// <summary>
    /// Child chunk nodes of the FOLD node.
    /// </summary>
    public IEnumerable<IChunkyNode> Children =>
        StreamEnumerableUtil.WithPosition(
            dataStream, Header.DataPosition, FromStream(dataStream, Header.Length)
        );

    /// <summary>
    /// Initializes a chunk FOLD node from a header and its data stream.
    /// </summary>
    /// <param name="header">Header of the FOLD node.</param>
    /// <param name="dataStream">Stream containing the content of the FOLD node.</param>
    public ChunkyFolderNode(ChunkHeader header, Stream dataStream)
        : base(header, dataStream)
    {
    }
}
