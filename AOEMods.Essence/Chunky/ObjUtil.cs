using AOEMods.Essence.Chunky.RRGeom;
using System.Text;

namespace AOEMods.Essence.Chunky;

/// <summary>
/// Provides functions to convert rrgeom meshes to obj.
/// </summary>
public static class ObjUtil
{
    /// <summary>
    /// Encodes a geometry object as obj and writes it to a stream.
    /// </summary>
    /// <param name="stream">Stream to write obj to.</param>
    /// <param name="geometryObject">Geometry object to encode.</param>
    public static void WriteGeometryObjectAsObj(Stream stream, GeometryObject geometryObject)
    {
        var streamWriter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        var pos = geometryObject.VertexPositions;
        var faces = geometryObject.Faces;
        var texCoords = geometryObject.VertexTextureCoordinates;
        var normals = geometryObject.VertexNormals;

        for (int i = 0; i < geometryObject.VertexPositions.GetLength(0); i++)
        {
            streamWriter.Write($"v {pos[i, 0]} {pos[i, 1]} {pos[i, 2]}\n");
            streamWriter.Write($"vt {texCoords[i, 0]} {1 - (float)texCoords[i, 1]}\n");
            streamWriter.Write($"vn {normals[i, 0]} {normals[i, 1]} {normals[i, 2]}\n");
        }

        for (int i = 0; i < geometryObject.Faces.GetLength(0); i++)
        {
            int idx1 = 1 + faces[i, 0];
            int idx2 = 1 + faces[i, 1];
            int idx3 = 1 + faces[i, 2];
            streamWriter.Write($"f {idx1}/{idx1}/{idx1} {idx2}/{idx2}/{idx2} {idx3}/{idx3}/{idx3}\n");
        }
    }
}
