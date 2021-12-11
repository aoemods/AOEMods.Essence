namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file chunk node.
/// </summary>
public interface IChunkyNode
{
    /// <summary>
    /// Header of the chunk node.
    /// </summary>
    ChunkHeader Header { get; }
}
