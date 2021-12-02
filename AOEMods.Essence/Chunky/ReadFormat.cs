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

namespace AOEMods.Essence.Chunky
{
    public static class ReadFormat
    {
        public static IEnumerable<GeometryObject> RRGeom(Stream stream)
        {
            using var reader = new ChunkyFileReader(stream, Encoding.ASCII);
            var fileHeader = reader.ReadChunkyFileHeader();

            var rootHeaders = reader.ReadChunkHeaders();
            var rootHeader = rootHeaders.Single();

            var foldRrgoHeaders = ChunkyUtil.EnumerateChunkHeaders(reader, rootHeader);

            var dataNbmeHeader = foldRrgoHeaders.Single(header => header.Type == "DATA" && header.Name == "NBME");
            var numberMeshes = RRGeomUtil.ReadDataNumber(reader, dataNbmeHeader);

            foreach (var foldMeshHeader in foldRrgoHeaders.Where(header => header.Type == "FOLD" && header.Name == "MESH"))
            {
                var foldMeshHeaders = ChunkyUtil.EnumerateChunkHeaders(reader, foldMeshHeader);
                
                var dataNbloHeader = foldMeshHeaders.Single(header => header.Type == "DATA" && header.Name == "NBLO");
                var numberLods = RRGeomUtil.ReadDataNumber(reader, dataNbloHeader);

                foreach (var foldLodHeader in foldMeshHeaders.Where(header => header.Type == "FOLD" && header.Name == "LOD "))
                {
                    var foldLodHeaders = ChunkyUtil.EnumerateChunkHeaders(reader, foldLodHeader);

                    var dataNbgoHeader = foldLodHeaders.Single(header => header.Type == "DATA" && header.Name == "NBGO");
                    var numberGeometryObjects = RRGeomUtil.ReadDataNumber(reader, dataNbloHeader);

                    foreach (var foldGeomHeader in foldLodHeaders.Where(header => header.Type == "FOLD" && header.Name == "GEOM"))
                    {
                        var foldGeomHeaders = ChunkyUtil.EnumerateChunkHeaders(reader, foldGeomHeader);

                        var dataGohdHeader = foldGeomHeaders.Single(header => header.Type == "DATA" && header.Name == "GOHD");
                        var dataGohd = RRGeomUtil.ReadDataGeometryObjectHd(reader, dataGohdHeader);

                        var geobHeaders = foldGeomHeaders.Where(header => header.Type == "DATA" && header.Name == "GEOB").ToArray();
                        var geobData = RRGeomUtil.ReadDataGeometryBData(reader, geobHeaders[0]);
                        var geobIndices = RRGeomUtil.ReadDataGeometryBIndices(reader, geobHeaders[1]);

                        yield return new GeometryObject(geobData.VertexPositions, geobIndices.Faces);
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

        public static IEnumerable<TextureMip> RRTex(Stream rrtexStream, IImageFormat outputFormat)
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
