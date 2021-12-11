namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file FOLD node containing more chunk nodes.
/// </summary>
public interface IChunkyFolderNode : IChunkyNode
{
    /// <summary>
    /// Chunk nodes contained in this FOLD node.
    /// </summary>
    IEnumerable<IChunkyNode> Children { get; }
}
