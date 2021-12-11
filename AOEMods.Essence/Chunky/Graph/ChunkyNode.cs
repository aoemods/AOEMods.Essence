using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Base class for Relic Chunky nodes.
/// </summary>
public abstract class ChunkyNode : IChunkyNode
{
    /// <summary>
    /// Header of the chunk.
    /// </summary>
    public ChunkHeader Header
    {
        get;
    }

    protected readonly Stream dataStream;

    /// <summary>
    /// Initializes a ChunkyNode from a chunk header and a stream containing its contents.
    /// </summary>
    /// <param name="header">Chunk header of the chunk.</param>
    /// <param name="dataStream">Stream containing the chunk's contents.</param>
    protected ChunkyNode(ChunkHeader header, Stream dataStream)
    {
        Header = header;
        this.dataStream = dataStream;
    }

    /// <summary>
    /// Reads all chunk nodes from a stream. Reads until the end of the stream.
    /// </summary>
    /// <param name="stream">Stream to read chunk nodes from.</param>
    /// <returns>Chunk nodes that were read from the stream.</returns>
    public static IEnumerable<IChunkyNode> FromStream(Stream stream)
        => FromStream(stream, stream.Length - stream.Position);

    /// <summary>
    /// Reads all chunk nodes from a stream. Reads until a given number of bytes were read.
    /// </summary>
    /// <param name="stream">Stream to read chunk nodes from.</param>
    /// <param name="length">Length of the chunks in the stream in bytes.</param>
    /// <returns>Chunk nodes that were read from the stream.</returns>
    public static IEnumerable<IChunkyNode> FromStream(Stream stream, long length)
        => StreamEnumerableUtil.WithStreamPosition(stream, FromStreamImpl(stream, length));

    private static IEnumerable<IChunkyNode> FromStreamImpl(Stream stream, long length)
    {
        var reader = new ChunkyFileReader(stream);
        foreach (var header in reader.ReadChunkHeaders(length))
        {
            switch (header.Type)
            {
                case "FOLD":
                    yield return new ChunkyFolderNode(header, stream);
                    break;
                case "DATA":
                    yield return new ChunkyStreamDataNode(header, stream);
                    break;
                default:
                    throw new Exception($"Unknown chunk type {header.Type}");
            }
        }
    }
}
