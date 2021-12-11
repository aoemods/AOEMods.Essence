namespace AOEMods.Essence.Chunky.RRGeom;

/// <summary>
/// Geometry object containing a model's mesh and a material reference.
/// </summary>
/// <param name="VertexPositions">Vertex positions of the mesh.</param>
/// <param name="VertexTextureCoordinates">Vertex texture coordinates of the mesh.</param>
/// <param name="VertexNormals">Vertex normals of the mesh.</param>
/// <param name="Faces">Triangle faces indexing the other vertex arrays.</param>
/// <param name="MaterialName">Optional name of the model's material.</param>
public record GeometryObject(Half[,] VertexPositions, Half[,] VertexTextureCoordinates, float[,] VertexNormals, ushort[,] Faces, string? MaterialName);