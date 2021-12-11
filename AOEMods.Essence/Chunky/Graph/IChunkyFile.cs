using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.Graph;

public interface IChunkyFile
{
    IEnumerable<IChunkyNode> RootNodes { get; }
    ChunkyFileHeader Header { get; }
}
