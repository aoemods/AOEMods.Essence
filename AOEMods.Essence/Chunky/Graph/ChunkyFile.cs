using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.Graph;

public class ChunkyFile : IChunkyFile
{
    public IEnumerable<IChunkyNode> RootNodes => StreamEnumerableUtil.WithPosition(stream, dataPosition, new ChunkyFileReader(stream).ReadNodes(dataLength));

    public ChunkyFileHeader Header
    {
        get;
    }

    private readonly Stream stream;
    private long dataPosition;
    private long dataLength;

    public ChunkyFile(ChunkyFileHeader header, Stream stream, long dataPosition, long dataLength)
    {
        Header = header;
        this.dataPosition = dataPosition;
        this.dataLength = dataLength;
        this.stream = stream;
    }

    public static ChunkyFile FromStream(Stream stream)
    {
        ChunkyFileReader reader = new(stream);
        var fileHeader = reader.ReadChunkyFileHeader();
        return new ChunkyFile(fileHeader, stream, stream.Position, stream.Length - stream.Position);
    }
}
