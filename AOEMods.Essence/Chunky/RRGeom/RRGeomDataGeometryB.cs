namespace AOEMods.Essence.Chunky.RRGeom;

public record RRGeomDataGeometryBData(
    uint ElementCount, uint Unknown1, uint ElementDataLength, uint Unknown2,
    Half[,] VertexPositions
);

public record RRGeomDataGeometryBIndices(
    uint ElementCount, uint Unknown1, uint ElementDataLength, uint Unknown2,
    ushort[,] Faces
);