using System.Text;

namespace AOEMods.Essence.Chunky.RRGeom;

public static class RRGeomUtil
{
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

        for (int i = 0; i < elementCount; i++)
        {
            vertexPositions[i, 0] = reader.ReadHalf();
            vertexPositions[i, 1] = reader.ReadHalf();
            vertexPositions[i, 2] = reader.ReadHalf();

            reader.BaseStream.Position += elementDataLength - 6;
        }

        return new RRGeomDataGeometryBData(elementCount, unknown1, elementDataLength, unknown2, vertexPositions);
    }

    public static RRGeomDataGeometryBIndices ReadDataGeometryBIndices(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        uint elementCount = reader.ReadUInt32();
        uint unknown1 = reader.ReadUInt32();
        uint elementDataLength = reader.ReadUInt32();
        uint unknown2 = reader.ReadUInt32();

        var faces = new ushort[elementCount, 3];

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
