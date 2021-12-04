namespace AOEMods.Essence.Chunky;

public interface IChunkyFolderNode : IChunkyNode
{
    IEnumerable<IChunkyNode> Children { get; }
}
