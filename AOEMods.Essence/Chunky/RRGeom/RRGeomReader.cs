using AOEMods.Essence.Chunky.Core;
using AOEMods.Essence.Chunky.Graph;
using System.Text;

namespace AOEMods.Essence.Chunky.RRGeom;

/// <summary>
/// Reads Relic Geometry (RRGeom) files.
/// </summary>
public class RRGeomReader : IRRGeomReader
{
    /// <summary>
    /// Reads GeometryObjects from a stream containing an RRGeom file.
    /// </summary>
    /// <param name="stream">Stream containing an RRGeom file.</param>
    /// <returns>GeometryObjects read from the stream.</returns>
    public static IEnumerable<GeometryObject> ReadRRGeom(Stream stream)
    {
        using var reader = new ChunkyFileReader(stream, Encoding.UTF8, true);

        var rootNode = (IChunkyFolderNode)ChunkyFile.FromStream(stream).RootNodes.Single();

        var rrgoNodes = rootNode.Children;

        var numberMeshesNode = rrgoNodes.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBME");
        var numberMeshes = ReadDataNumber(reader, numberMeshesNode.Header);

        foreach (var meshNode in rrgoNodes.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "MESH"))
        {
            var numberLodsNode = meshNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBLO");
            var numberLods = ReadDataNumber(reader, numberLodsNode.Header);

            foreach (var lodNode in meshNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "LOD "))
            {
                var numberGeometryObjectsNode = lodNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBGO");
                var numberGeometryObjects = ReadDataNumber(reader, numberGeometryObjectsNode.Header);

                foreach (var geometryNode in lodNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "GEOM"))
                {
                    var gohdNode = geometryNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "GOHD");
                    var gohd = ReadDataGeometryObjectHd(reader, gohdNode.Header);

                    var geobNodes = geometryNode.Children.OfType<IChunkyDataNode>().Where(node => node.Header.Name == "GEOB").ToArray();
                    var geobData = ReadDataGeometryBData(reader, geobNodes[0].Header);
                    var geobIndices = ReadDataGeometryBIndices(reader, geobNodes[1].Header);

                    yield return new GeometryObject(
                        geobData.VertexPositions, geobData.VertexTextureCoordinates,
                        geobData.VertexNormals, geobIndices.Faces,
                        gohd.Strings.Count >= 2 ? gohd.Strings[1] : null
                    );
                }
            }
        }
    }

    private static uint ReadDataNumber(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;
        return reader.ReadUInt32();
    }

    private static RRGeomDataGeometryObjectHd ReadDataGeometryObjectHd(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        List<string> names = new();
        while (reader.BaseStream.Position < header.DataPosition + header.Length - 1)
        {
            names.Add(Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())));
        }

        byte unknown = reader.ReadByte();

        return new RRGeomDataGeometryObjectHd(names, unknown);
    }

    private static RRGeomDataGeometryBData ReadDataGeometryBData(ChunkyFileReader reader, ChunkHeader header)
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

    private static RRGeomDataGeometryBIndices ReadDataGeometryBIndices(ChunkyFileReader reader, ChunkHeader header)
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

    private static RRGeomDataGeometryC ReadDataGeometryC(ChunkyFileReader reader, ChunkHeader header)
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
