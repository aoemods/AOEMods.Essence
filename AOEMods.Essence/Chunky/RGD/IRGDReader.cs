namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Reads Relic Game Data (RGD) files.
/// </summary>
public interface IRGDReader
{
    /// <summary>
    /// Reads a list of RGD nodes from a stream containing an RGD file.
    /// </summary>
    /// <param name="stream">Stream containing an RGD file.</param>
    /// <returns>RGD nodes read from the stream.</returns>
    static abstract IList<RGDNode> ReadRGD(Stream stream);
}
