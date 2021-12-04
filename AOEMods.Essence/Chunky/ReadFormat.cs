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
        public static IEnumerable<Material> RRMaterial(Stream stream)
        {
            using var reader = new ChunkyFileReader(stream, Encoding.ASCII);

            var chunky = reader.ReadChunky();
            var materialNodes = chunky.RootNodes.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "GMat");

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

                        yield return new GeometryObject(geobData.VertexPositions, geobData.VertexTextureCoordinates, geobData.VertexNormals, geobIndices.Faces);
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

            var keys = reader.ReadKeysDataChunk(keysHeaders[0]);
            var kvs = reader.ReadKeyValueDataChunk(kvsHeaders[0]);

            var keysInv = ChunkyUtil.ReverseReadOnlyDictionary(keys.StringKeys);

            static RGDNode makeNode(ulong key, object value, IReadOnlyDictionary<ulong, string> keysInv)
            {
                string keyStr = keysInv[key];

                if (value is ChunkyList table)
                {
                    return new RGDNode(keyStr, table.Select(listItem => makeNode(listItem.Key, listItem.Value, keysInv)).ToArray());
                }

                return new RGDNode(keyStr, value);
            }

            return kvs.KeyValues.Select(kv => makeNode(kv.Key, kv.Value, keysInv)).ToArray();
        }

        public enum RRTexType
        {
            Generic,
            NormalMap,
            Metal,
            Pattern,
        };

        public static IEnumerable<TextureMip> RRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        {
            var reader = new ChunkyFileReader(rrtexStream, Encoding.ASCII);
            var fileHeader = reader.ReadChunkyFileHeader();

            int mipCount = -1;
            int width = 0;
            int height = 0;
            int[] mipTextureCounts = null;
            List<int> sizeCompressed = null;
            List<int> sizeUncompressed = null;
            int textureCompression = -1;

            IEnumerable<TextureMip> ReadFolderRecursive(long position, long length)
            {
                reader.BaseStream.Position = position;
                foreach (var header in reader.ReadChunkHeaders(length))
                {
                    long pos = reader.BaseStream.Position;
                    reader.BaseStream.Position = header.DataPosition;

                    if (header.Name == "TMAN")
                    {
                        int unknown1 = reader.ReadInt32();
                        width = reader.ReadInt32();
                        height = reader.ReadInt32();
                        int unknown2 = reader.ReadInt32();
                        int unknown3 = reader.ReadInt32();
                        textureCompression = reader.ReadInt32();

                        mipCount = reader.ReadInt32();
                        int unknown5 = reader.ReadInt32();

                        mipTextureCounts = new int[mipCount];
                        for (int i = 0; i < mipCount; i++)
                        {
                            mipTextureCounts[i] = reader.ReadInt32();
                        }

                        sizeCompressed = new();
                        sizeUncompressed = new();

                        for (int i = 0; i < mipCount; i++)
                        {
                            for (int j = 0; j < mipTextureCounts[i]; j++)
                            {
                                sizeUncompressed.Add(reader.ReadInt32());
                                sizeCompressed.Add(reader.ReadInt32());
                            }
                        }
                    }

                    if (header.Name == "TDAT")
                    {
                        int count = 0;
                        int unk = reader.ReadInt32();
                        for (int i = 0; i < mipCount; i++)
                        {
                            List<byte> data = new();
                            int w = -1;
                            int h = -1;

                            for (int j = 0; j < mipTextureCounts[i]; j++)
                            {
                                long prePos = reader.BaseStream.Position;

                                if (sizeCompressed[count] != sizeUncompressed[count])
                                {
                                    byte[] zlibHeader = reader.ReadBytes(2);

                                    DeflateStream deflateStream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress, true);
                                    MemoryStream inflatedStream = new MemoryStream();
                                    deflateStream.CopyTo(inflatedStream);
                                    int length2 = (int)inflatedStream.Length;

                                    BinaryReader dataReader = new BinaryReader(inflatedStream);
                                    dataReader.BaseStream.Position = 0;

                                    if (j == 0)
                                    {
                                        int mipLevel = dataReader.ReadInt32();
                                        int widthx = dataReader.ReadInt32();
                                        int heightx = dataReader.ReadInt32();
                                        w = Math.Max(widthx, 4);
                                        h = Math.Max(heightx, 4);
                                        int numPhysicalTexels = dataReader.ReadInt32();
                                        data.AddRange(dataReader.ReadBytes(length2 - 16));
                                    }
                                    else
                                    {
                                        data.AddRange(dataReader.ReadBytes(length2));
                                    }
                                }
                                else
                                {
                                    int length2 = sizeUncompressed[count];

                                    if (j == 0)
                                    {
                                        int mipLevel = reader.ReadInt32();
                                        int widthx = reader.ReadInt32();
                                        int heightx = reader.ReadInt32();
                                        w = Math.Max(widthx, 4);
                                        h = Math.Max(heightx, 4);
                                        int numPhysicalTexels = reader.ReadInt32();
                                        data.AddRange(reader.ReadBytes(length2 - 16));
                                    }
                                    else
                                    {
                                        data.AddRange(reader.ReadBytes(length2));
                                    }
                                }

                                reader.BaseStream.Position = prePos + sizeCompressed[count];

                                count++;
                            }

                            var decoder = new BcDecoder();

                            var format = textureCompression switch
                            {
                                2 => CompressionFormat.R,
                                18 => CompressionFormat.Bc1WithAlpha,
                                19 => CompressionFormat.Bc1,
                                22 => CompressionFormat.Bc3,
                                28 => CompressionFormat.Bc7,
                                _ => CompressionFormat.Unknown
                            };

                            if (format == CompressionFormat.Unknown)
                            {
                                continue;
                            }

                            Image<Rgba32> image = decoder.DecodeRawToImageRgba32(data.ToArray(), w, h, format);

                            switch (textureType)
                            {
                                case RRTexType.NormalMap:
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        for (int y = 0; y < image.Height; y++)
                                        {
                                            image[x, y] = new Rgba32(image[x, y].A, image[x, y].G, image[x, y].B, 255);
                                        }
                                    }
                                    break;
                                case RRTexType.Metal:
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        for (int y = 0; y < image.Height; y++)
                                        {
                                            image[x, y] = new Rgba32(image[x, y].R, 127, 0, 255);
                                        }
                                    }
                                    break;
                                case RRTexType.Pattern:
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        for (int y = 0; y < image.Height; y++)
                                        {
                                            image[x, y] = new Rgba32(image[x, y].R, 0, 0, 255);
                                        }
                                    }
                                    break;
                            }

                            MemoryStream outputStream = new();
                            image.Save(outputStream, outputFormat);
                            yield return new TextureMip(i, outputStream.ToArray());
                        }
                    }

                    reader.BaseStream.Position = pos;

                    if (header.Type == "FOLD")
                    {
                        foreach (var result in ReadFolderRecursive(header.DataPosition, header.Length))
                        {
                            yield return result;
                        }
                    }
                }
            }

            foreach (var result in ReadFolderRecursive(reader.BaseStream.Position, reader.BaseStream.Length - reader.BaseStream.Position))
            {
                yield return result;
            }
        }
    }
}
