namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Writes RGD nodes to a stream.
/// </summary>
public interface IRGDWriter
{
    /// <summary>
    /// Write RGD nodes to the given stream.
    /// </summary>
    /// <param name="stream">Stream to write RGD nodes to.</param>
    /// <param name="nodes">RGD nodes to write to the given stream.</param>
    static abstract void WriteRGD(Stream stream, IList<RGDNode> nodes);
}
