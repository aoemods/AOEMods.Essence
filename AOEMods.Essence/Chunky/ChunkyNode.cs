namespace AOEMods.Essence.Chunky;

public abstract class ChunkyNode : IChunkyNode
{
    public ChunkHeader Header
    {
        get;
    }

    protected readonly Stream dataStream;

    public ChunkyNode(ChunkHeader header, Stream dataStream)
    {
        Header = header;
        this.dataStream = dataStream;
    }
}
