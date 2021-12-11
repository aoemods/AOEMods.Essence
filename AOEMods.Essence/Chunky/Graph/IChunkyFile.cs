using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file.
/// </summary>
public interface IChunkyFile
{
    /// <summary>
    /// Root chunk nodes of the Relic Chunky file.
    /// </summary>
    IEnumerable<IChunkyNode> RootNodes { get; }

    /// <summary>
    /// Header of the Relic Chunky file.
    /// </summary>
    ChunkyFileHeader Header { get; }
}
