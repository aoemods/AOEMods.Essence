namespace AOEMods.Essence.Chunky.RRGeom;

public interface IRRGeomReader
{
    static abstract IEnumerable<GeometryObject> ReadRRGeom(Stream stream);
}
