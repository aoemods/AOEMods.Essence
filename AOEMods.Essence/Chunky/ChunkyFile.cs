namespace AOEMods.Essence.Chunky;

public class ChunkyFile : IChunkyFile
{
    public IEnumerable<IChunkyNode> RootNodes
    {
        get
        {
            var reader = new ChunkyFileReader(stream);
            stream.Position = dataPosition;
            foreach (var node in reader.ReadNodes(dataLength))
            {
                yield return node;
            }
        }
    }

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
}
