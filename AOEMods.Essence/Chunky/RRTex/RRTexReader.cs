using AOEMods.Essence.Chunky.Core;
using AOEMods.Essence.Chunky.Graph;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;

namespace AOEMods.Essence.Chunky.RRTex;

/// <summary>
/// Reads Relic Texture (RRTex) files.
/// </summary>
public class RRTexReader : IRRTexReader
{
    /// <summary>
    /// Reads TextureMips from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMips.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMips read from the stream.</returns>
    public static IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        var reader = new ChunkyFileReader(rrtexStream);
        var fileHeader = reader.ReadChunkyFileHeader();
        var chunkyFile = ChunkyFile.FromStream(rrtexStream);
        var dataNodes = chunkyFile.RootNodes.OfType<IChunkyDataNode>();

        var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
        var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

        var tman = ReadDataTman(reader, tmanNode.Header);
        var mips = ReadDataTdat(reader, tdatNode.Header, tman, outputFormat, textureType);
        foreach (var mip in mips)
        {
            yield return mip;
        }
    }

    /// <summary>
    /// Reads the last (usually largest) TextureMip from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMip.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMip read from the stream.</returns>
    public static TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        var reader = new ChunkyFileReader(rrtexStream);
        var chunkyFile = ChunkyFile.FromStream(rrtexStream);
        var dataNodes = ((IChunkyFolderNode)chunkyFile.RootNodes.Single(node => node.Header.Name == "TSET")).Children
            .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "TXTR").Children
            .OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "DXTC").Children
            .OfType<IChunkyDataNode>().ToArray();

        var tmanNode = dataNodes.First(node => node.Header.Name == "TMAN");
        var tdatNode = dataNodes.First(node => node.Header.Name == "TDAT");

        var tman = ReadDataTman(reader, tmanNode.Header);
        return ReadDataTdatLastMip(reader, tdatNode.Header, tman, outputFormat, textureType);
    }

    private static RRTexDataTman ReadDataTman(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        int unknown1 = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        int unknown2 = reader.ReadInt32();
        int unknown3 = reader.ReadInt32();
        int textureCompression = reader.ReadInt32();

        int mipCount = reader.ReadInt32();
        int unknown4 = reader.ReadInt32();

        int[] mipTextureCounts = new int[mipCount];
        for (int i = 0; i < mipCount; i++)
        {
            mipTextureCounts[i] = reader.ReadInt32();
        }

        int[][] sizeCompressed = new int[mipCount][];
        int[][] sizeUncompressed = new int[mipCount][];

        for (int i = 0; i < mipCount; i++)
        {
            int textureChunkCount = mipTextureCounts[i];
            sizeUncompressed[i] = new int[textureChunkCount];
            sizeCompressed[i] = new int[textureChunkCount];
            for (int j = 0; j < textureChunkCount; j++)
            {
                sizeUncompressed[i][j] = reader.ReadInt32();
                sizeCompressed[i][j] = reader.ReadInt32();
            }
        }

        return new(
            unknown1, width, height, unknown2, unknown3, textureCompression,
            mipCount, unknown4, mipTextureCounts, sizeCompressed, sizeUncompressed
        );
    }

    private static BcDecoder decoder = new();

    private static TextureMip? ReadMip(ChunkyFileReader reader, int mip, RRTexDataTman tman, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        MemoryStream data = new();
        int w = -1;
        int h = -1;

        void WriteData(BinaryReader dataReader, bool isFirstChunk)
        {
            int length = (int)dataReader.BaseStream.Length;
            if (isFirstChunk)
            {
                int mipLevel = dataReader.ReadInt32();
                int widthx = dataReader.ReadInt32();
                int heightx = dataReader.ReadInt32();
                w = Math.Max(widthx, 4);
                h = Math.Max(heightx, 4);
                int numPhysicalTexels = dataReader.ReadInt32();
                data.Write(dataReader.ReadBytes(length - 16));
            }
            else
            {
                data.Write(dataReader.ReadBytes(length));
            }
        }

        for (int j = 0; j < tman.MipTextureCount[mip]; j++)
        {
            long prePos = reader.BaseStream.Position;
            bool isFirstChunk = j == 0;

            if (tman.SizeCompressed[mip][j] != tman.SizeUncompressed[mip][j])
            {
                byte[] zlibHeader = reader.ReadBytes(2);

                DeflateStream deflateStream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress, true);
                MemoryStream inflatedStream = new MemoryStream();
                deflateStream.CopyTo(inflatedStream);

                inflatedStream.Position = 0;
                WriteData(new BinaryReader(inflatedStream), isFirstChunk);
            }
            else
            {
                WriteData(reader, isFirstChunk);
            }

            reader.BaseStream.Position = prePos + tman.SizeCompressed[mip][j];
        }

        var format = tman.TextureCompression switch
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
            return null;
        }

        data.Position = 0;
        Image<Rgba32> image = decoder.DecodeRawToImageRgba32(data, w, h, format);

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
        return new TextureMip(mip, outputStream.ToArray());
    }

    private static IEnumerable<TextureMip> ReadDataTdat(ChunkyFileReader reader, ChunkHeader header, RRTexDataTman tman, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        reader.BaseStream.Position = header.DataPosition;
        int unk = reader.ReadInt32();

        for (int i = 0; i < tman.MipCount; i++)
        {
            var mip = ReadMip(reader, i, tman, outputFormat, textureType);

            if (mip != null)
            {
                yield return mip.Value;
            }
        }
    }

    private static TextureMip? ReadDataTdatLastMip(ChunkyFileReader reader, ChunkHeader header, RRTexDataTman tman, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
    {
        reader.BaseStream.Position = header.DataPosition;
        int unk = reader.ReadInt32();

        reader.BaseStream.Position += tman.SizeCompressed
            .Take(tman.MipCount - 1)
            .Sum(sizes => sizes.Sum());

        return ReadMip(reader, tman.MipCount - 1, tman, outputFormat, textureType);
    }
}
