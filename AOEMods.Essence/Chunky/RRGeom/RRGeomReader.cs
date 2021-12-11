using AOEMods.Essence.Chunky.Core;
using AOEMods.Essence.Chunky.Graph;

namespace AOEMods.Essence.Chunky.RRGeom;

public class RRGeomReader : IRRGeomReader
{
    public static IEnumerable<GeometryObject> ReadRRGeom(Stream stream)
    {
        using var reader = new ChunkyFileReader(stream);

        var rootNode = (IChunkyFolderNode)ChunkyFile.FromStream(stream).RootNodes.Single();

        var rrgoNodes = rootNode.Children;

        var numberMeshesNode = rrgoNodes.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBME");
        var numberMeshes = RRGeomUtil.ReadDataNumber(reader, numberMeshesNode.Header);

        foreach (var meshNode in rrgoNodes.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "MESH"))
        {
            var numberLodsNode = meshNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBLO");
            var numberLods = RRGeomUtil.ReadDataNumber(reader, numberLodsNode.Header);

            foreach (var lodNode in meshNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "LOD "))
            {
                var numberGeometryObjectsNode = lodNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "NBGO");
                var numberGeometryObjects = RRGeomUtil.ReadDataNumber(reader, numberGeometryObjectsNode.Header);

                foreach (var geometryNode in lodNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "GEOM"))
                {
                    var gohdNode = geometryNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "GOHD");
                    var gohd = RRGeomUtil.ReadDataGeometryObjectHd(reader, gohdNode.Header);

                    var geobNodes = geometryNode.Children.OfType<IChunkyDataNode>().Where(node => node.Header.Name == "GEOB").ToArray();
                    var geobData = RRGeomUtil.ReadDataGeometryBData(reader, geobNodes[0].Header);
                    var geobIndices = RRGeomUtil.ReadDataGeometryBIndices(reader, geobNodes[1].Header);

                    yield return new GeometryObject(
                        geobData.VertexPositions, geobData.VertexTextureCoordinates,
                        geobData.VertexNormals, geobIndices.Faces,
                        gohd.Names.Count >= 2 ? gohd.Names[1] : null
                    );
                }
            }
        }
    }
}
