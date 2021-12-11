namespace AOEMods.Essence.Chunky.RRGeom;

/// <summary>
/// RRGeom DATA GOHD chunk containing a list of strings.
/// </summary>
/// <param name="Strings">Strings of the chunk. The second string is usually the material name.</param>
/// <param name="Unknown">Unknown 1 byte value.</param>
public record RRGeomDataGeometryObjectHd(IList<string> Strings, byte Unknown);