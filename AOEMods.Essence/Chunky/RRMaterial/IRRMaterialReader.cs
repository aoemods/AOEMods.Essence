namespace AOEMods.Essence.Chunky.RRMaterial;

public interface IRRMaterialReader
{
    static abstract IEnumerable<Material> ReadRRMaterial(Stream stream, string? materialName = null);
}
