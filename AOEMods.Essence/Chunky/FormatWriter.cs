using AOEMods.Essence.Chunky.RGD;

namespace AOEMods.Essence.Chunky;

/// <summary>
/// Provides functions to read different Relic Chunky formats.
/// </summary>
public class FormatWriter : IRGDWriter
{
    /// <summary>
    /// Writes an RGD file to a stream given a list of RGD root nodes.
    /// </summary>
    /// <param name="stream">Stream to write the RGD file to.</param>
    /// <param name="nodes">RGD root nodes from which to write the RGD file.</param>
    public static void WriteRGD(Stream stream, IList<RGDNode> nodes)
        => RGDWriter.WriteRGD(stream, nodes);
}
