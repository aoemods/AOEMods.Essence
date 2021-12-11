namespace AOEMods.Essence.Chunky.Graph;

public interface IChunkyFolderNode : IChunkyNode
{
    IEnumerable<IChunkyNode> Children { get; }
}
