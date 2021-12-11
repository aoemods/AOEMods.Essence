namespace AOEMods.Essence.Chunky.RRGeom;

/// <summary>
/// RRGeom first DATA GEOB chunk containing geometry mesh data.
/// </summary>
/// <param name="ElementCount">Number of elements in the data</param>
/// <param name="Unknown1">Unknown 4 byte value.</param>
/// <param name="ElementDataLength">Length in bytes of a single element.</param>
/// <param name="Unknown2">Unknown 4 byte value.</param>
/// <param name="VertexPositions">Vertex positions contained in the chunk.</param>
/// <param name="VertexNormals">Vertex normals contained in the chunk.</param>
/// <param name="VertexTextureCoordinates">Vertex texture coordinates contained in the chunk.</param>
public record RRGeomDataGeometryBData(
    uint ElementCount, uint Unknown1, uint ElementDataLength, uint Unknown2,
    Half[,] VertexPositions, float[,] VertexNormals, Half[,] VertexTextureCoordinates
);

/// <summary>
/// RRGeom second DATA GEOB chunk containing triangle faces indexing the first DATA GEOB chunk's vertices.
/// </summary>
/// <param name="ElementCount">Number of elements in the data.</param>
/// <param name="Unknown1">Unknown 4 byte value.</param>
/// <param name="ElementDataLength">Length in bytes of a single element.</param>
/// <param name="Unknown2">Unknown 4 byte value.</param>
/// <param name="Faces">Triangle faces indexing the first DATA GEOB chunk's vertices.</param>
public record RRGeomDataGeometryBIndices(
    uint ElementCount, uint Unknown1, uint ElementDataLength, uint Unknown2,
    ushort[,] Faces
);