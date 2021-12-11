using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file containing a f ile header and chunky nodes.
/// </summary>
public class ChunkyFile : IChunkyFile
{
    /// <summary>
    /// Nodes of the root chunks of the Relic Chunky file.
    /// </summary>
    public IEnumerable<IChunkyNode> RootNodes
    {
        get
        {
            stream.Position = dataPosition;
            return ChunkyNode.FromStream(stream, dataLength);
        }
    }

    /// <summary>
    /// Header of the Relic Chunky file.
    /// </summary>
    public ChunkyFileHeader Header
    {
        get;
    }

    private readonly Stream stream;
    private readonly long dataPosition;
    private readonly long dataLength;

    private ChunkyFile(ChunkyFileHeader header, Stream stream, long dataPosition, long dataLength)
    {
        Header = header;
        this.dataPosition = dataPosition;
        this.dataLength = dataLength;
        this.stream = stream;
    }

    /// <summary>
    /// Initializes a ChunkyFile from a stream. The stream should start with the Relic Chunky file header.
    /// </summary>
    /// <param name="stream">Stream to read the Relic Chunky file from.</param>
    /// <returns>ChunkyFile read from the passed stream.</returns>
    public static ChunkyFile FromStream(Stream stream)
    {
        ChunkyFileReader reader = new(stream);
        var fileHeader = reader.ReadChunkyFileHeader();
        return new ChunkyFile(fileHeader, stream, stream.Position, stream.Length - stream.Position);
    }
}
