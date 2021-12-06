using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRTex;
using AOEMods.Essence.Chunky.RRGeom;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;
using System.Text;
using AOEMods.Essence.Chunky.RRMaterial;

namespace AOEMods.Essence.Chunky
{
    public static class ReadFormat
    {
        public static IEnumerable<Material> RRMaterial(Stream stream, string? materialName = null)
        {
            using var reader = new ChunkyFileReader(stream, Encoding.ASCII);

            var chunky = reader.ReadChunky();
            var materialNodes = chunky.RootNodes
                .OfType<IChunkyFolderNode>()
                .Where(node => node.Header.Name == "GMat");

            if (!string.IsNullOrEmpty(materialName))
            {
                materialNodes = materialNodes.Where(node => node.Header.Path == materialName);
            }

            foreach (var materialNode in materialNodes)
            {
                var matPNode = materialNode.Children.OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "MatP");
                var pbNodes = matPNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "\0\0Pb");
                foreach (var pbNode in pbNodes)
                {
                    var pbTextureNode = pbNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "PbTe");

                    reader.BaseStream.Position = pbTextureNode.Header.DataPosition;

                    uint textureCount = reader.ReadUInt32();
                    var textures = new Dictionary<string, string>((int)textureCount);

                    for (uint i = 0; i < textureCount; i++)
                    {
                        uint keyLength = reader.ReadUInt32();
                        string key = Encoding.UTF8.GetString(reader.ReadBytes((int)keyLength));
                        uint valueLength = reader.ReadUInt32();
                        string value = Encoding.UTF8.GetString(reader.ReadBytes((int)valueLength));
                        if (textures.ContainsKey(key) && textures[key] != value)
                        {
                            throw new Exception($"Textures already contains key {key} but does not match value {value}");
                        }
                        textures[key] = value;
                    }

                    yield return new Material(new RRMaterialPbTextures(textures));
                }
            }
        }

        public static IEnumerable<GeometryObject> RRGeom(Stream stream)
        {
            using var reader = new ChunkyFileReader(stream, Encoding.ASCII);
            var fileHeader = reader.ReadChunkyFileHeader();

            var rootNodes = reader.ReadNodes();
            var rootNode = (IChunkyFolderNode)rootNodes.Single();

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

        public static IList<RGDNode> RGD(string rgdPath)
        {
            return RGD(File.Open(rgdPath, FileMode.Open));
        }

        public static IList<RGDNode> RGD(Stream stream)
        {
            using var reader = new ChunkyFileReader(stream, Encoding.ASCII);

            reader.ReadChunkyFileHeader();
            var chunkHeaders = reader.ReadChunkHeaders().ToArray();

            ChunkHeader[] keysHeaders = chunkHeaders.Where(header => header.Type == "DATA" && header.Name == "KEYS").ToArray();
            ChunkHeader[] kvsHeaders = chunkHeaders.Where(header => header.Type == "DATA" && header.Name == "AEGD").ToArray();

            if (keysHeaders.Length == 0)
            {
                throw new Exception("No DATA KEYS chunk present");
            }

            if (keysHeaders.Length > 1)
            {
                throw new Exception("More than one DATA KEYS chunk present");
            }

            if (kvsHeaders.Length == 0)
            {
                throw new Exception("No DATA AEGD chunk present");
            }

            if (kvsHeaders.Length > 1)
            {
                throw new Exception("More than one DATA AEGD chunk present");
            }

            var keys = RGDUtil.ReadKeysDataChunk(reader, keysHeaders[0]);
            var kvs = RGDUtil.ReadKeyValueDataChunk(reader, kvsHeaders[0]);

            var keysInv = ChunkyUtil.ReverseReadOnlyDictionary(keys.StringKeys);

            static RGDNode MakeNode(ulong key, object value, IReadOnlyDictionary<ulong, string> keysInv)
            {
                string keyStr = keysInv[key];

                if (value is ChunkyList table)
                {
                    return new RGDNode(keyStr, table.Select(listItem => MakeNode(listItem.Key, listItem.Value, keysInv)).ToArray());
                }

                return new RGDNode(keyStr, value);
            }

            return kvs.KeyValues.Select(kv => MakeNode(kv.Key, kv.Value, keysInv)).ToArray();
        }

        public static IEnumerable<TextureMip> RRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        {
            var reader = new ChunkyFileReader(rrtexStream, Encoding.ASCII);
            var fileHeader = reader.ReadChunkyFileHeader();
            var chunkyFile = reader.ReadChunky();
            var dataNodes = chunkyFile.RootNodes.OfType<IChunkyDataNode>();

            var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
            var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

            var tman = RRTexUtil.ReadDataTman(reader, tmanNode.Header);
            var mips = RRTexUtil.ReadDataTdat(reader, tdatNode.Header, tman, outputFormat, textureType);
            foreach (var mip in mips)
            {
                yield return mip;
            }
        }

        public static TextureMip? RRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        {
            var reader = new ChunkyFileReader(rrtexStream, Encoding.ASCII);
            var chunkyFile = reader.ReadChunky();
            var dataNodes = ((IChunkyFolderNode)chunkyFile.RootNodes.Single(node => node.Header.Name == "TSET")).Children
                .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "TXTR").Children
                .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "DXTC").Children
                .OfType<IChunkyDataNode>().ToArray();

            var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
            var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

            var tman = RRTexUtil.ReadDataTman(reader, tmanNode.Header);
            return RRTexUtil.ReadDataTdatLastMip(reader, tdatNode.Header, tman, outputFormat, textureType);
        }
    }
}
