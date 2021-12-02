using System.Text;

namespace AOEMods.Essence.Chunky.RRGeom;

public static class RRGeomUtil
{
    public static void WriteGeometryObject(Stream stream, GeometryObject geometryObject)
    {
        var streamWriter = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

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

    public static uint ReadDataNumber(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;
        return reader.ReadUInt32();
    }

    public static RRGeomDataGeometryObjectHd ReadDataGeometryObjectHd(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        List<string> names = new();
        while (reader.BaseStream.Position < header.DataPosition + header.Length - 1)
        {
            names.Add(Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32())));
        }

        byte unknown = reader.ReadByte();

        return new RRGeomDataGeometryObjectHd(names, unknown);
    }

    public static RRGeomDataGeometryBData ReadDataGeometryBData(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        uint elementCount = reader.ReadUInt32();
        uint unknown1 = reader.ReadUInt32();
        uint elementDataLength = reader.ReadUInt32();
        uint unknown2 = reader.ReadUInt32();

        var vertexPositions = new Half[elementCount, 3];
        var vertexNormals = new float[elementCount, 3];
        var vertexUvs = new Half[elementCount, 2];

        for (int i = 0; i < elementCount; i++)
        {
            long startPos = reader.BaseStream.Position;

            // Position at offset 0, 6 halfs
            vertexPositions[i, 0] = reader.ReadHalf();
            vertexPositions[i, 1] = reader.ReadHalf();
            vertexPositions[i, 2] = reader.ReadHalf();

            // Normals at offset 8, 3 signed bytes
            // Not sure if we should normalize or not, length is close to 1 already
            reader.BaseStream.Position += 2;
            vertexNormals[i, 0] = (float)reader.ReadSByte() / 128;
            vertexNormals[i, 1] = (float)reader.ReadSByte() / 128;
            vertexNormals[i, 2] = (float)reader.ReadSByte() / 128;

            // Texture coordinates at offset 12, 2 halfs
            reader.BaseStream.Position += 1;
            vertexUvs[i, 0] = reader.ReadHalf();
            vertexUvs[i, 1] = reader.ReadHalf();

            reader.BaseStream.Position = startPos + elementDataLength;
        }

        return new RRGeomDataGeometryBData(elementCount, unknown1, elementDataLength, unknown2, vertexPositions, vertexNormals, vertexUvs);
    }

    public static RRGeomDataGeometryBIndices ReadDataGeometryBIndices(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        uint elementCount = reader.ReadUInt32();
        uint unknown1 = reader.ReadUInt32();
        uint elementDataLength = reader.ReadUInt32();
        uint unknown2 = reader.ReadUInt32();

        var faces = new ushort[elementCount / 3, 3];

        for (int i = 0; i < elementCount / 3; i++)
        {
            faces[i, 0] = reader.ReadUInt16();
            faces[i, 1] = reader.ReadUInt16();
            faces[i, 2] = reader.ReadUInt16();

            reader.BaseStream.Position += 3 * elementDataLength - 6;
        }

        return new RRGeomDataGeometryBIndices(elementCount, unknown1, elementDataLength, unknown2, faces);
    }

    public static RRGeomDataGeometryC ReadDataGeometryC(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        var unknown = new int[header.Length / 4];
        for (int i = 0; i < unknown.Length; i++)
        {
            unknown[i] = reader.ReadInt32();
        }

        return new RRGeomDataGeometryC(unknown);
    }
}
