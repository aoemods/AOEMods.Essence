namespace AOEMods.Essence.Chunky.RRMaterial;

/// <summary>
/// Reads Relic Material (RRMaterial) files.
/// </summary>
public interface IRRMaterialReader
{
    /// <summary>
    /// Reads Materials from a stream containing an RRMaterial file.
    /// </summary>
    /// <param name="stream">Stream containing an RRMaterial file.</param>
    /// <returns>Materials read from the stream.</returns>
    static abstract IEnumerable<Material> ReadRRMaterial(Stream stream, string? materialName = null);
}
