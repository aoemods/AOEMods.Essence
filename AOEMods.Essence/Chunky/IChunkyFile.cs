namespace AOEMods.Essence.Chunky;

public interface IChunkyFile
{
    IEnumerable<IChunkyNode> RootNodes { get; }
    ChunkyFileHeader Header { get; }
}
