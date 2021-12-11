namespace AOEMods.Essence.Chunky.Graph;

/// <summary>
/// Relic Chunky file DATA node containing a blob of data.
/// </summary>
public interface IChunkyDataNode : IChunkyNode
{
    /// <summary>
    /// Reads the DATA chunk's data blob.
    /// </summary>
    /// <returns>Data chunk's data blob.</returns>
    IEnumerable<byte> GetData();
}
