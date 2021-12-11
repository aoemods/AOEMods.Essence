namespace AOEMods.Essence.Chunky.RRGeom;

/// <summary>
/// Reads Relic Geometry (RRGeom) files.
/// </summary>
public interface IRRGeomReader
{
    /// <summary>
    /// Reads GeometryObjects from a stream containing an RRGeom file.
    /// </summary>
    /// <param name="stream">Stream containing an RRGeom file.</param>
    /// <returns>GeometryObjects read from the stream.</returns>
    static abstract IEnumerable<GeometryObject> ReadRRGeom(Stream stream);
}
