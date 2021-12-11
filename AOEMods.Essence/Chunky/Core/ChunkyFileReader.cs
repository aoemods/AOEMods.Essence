using AOEMods.Essence.Chunky.Graph;
using System.Collections;
using System.Text;

namespace AOEMods.Essence.Chunky.Core;
public class ChunkyFileReader : BinaryReader
{
    public ChunkyFileReader(Stream input) : base(input)
    {
    }

    public ChunkyFileReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public ChunkyFileReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }

    public string ReadCString()
    {
        List<byte> chars = new();
        while (true)
        {
            byte b = ReadByte();
            if (b != 0)
            {
                chars.Add(b);
            }
            else
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(chars.ToArray());
    }

    private IEnumerable<ChunkHeader> ReadChunkHeadersImpl(long? length = null)
    {
        long position = BaseStream.Position;
        length ??= BaseStream.Length - position;
        while (BaseStream.Position < position + length)
        {
            long oldPosition = BaseStream.Position;
            var chunkHeader = ReadChunkHeader();

            if (chunkHeader.Type != "DATA" && chunkHeader.Type != "FOLD")
            {
                BaseStream.Position = oldPosition;
                break;
            }

            yield return chunkHeader;

            BaseStream.Position = chunkHeader.DataPosition + chunkHeader.Length;
        }
    }

    public IEnumerable<ChunkHeader> ReadChunkHeaders(long? length = null) => StreamEnumerableUtil.WithStreamPosition(BaseStream, ReadChunkHeadersImpl(length));

    public ChunkHeader ReadChunkHeader()
    {
        return new ChunkHeader(
            Encoding.UTF8.GetString(ReadBytes(4)),
            Encoding.UTF8.GetString(ReadBytes(4)),
            ReadInt32(),
            ReadInt32(),
            Encoding.UTF8.GetString(ReadBytes(ReadInt32())).Replace("\0", ""),
            BaseStream.Position
        );
    }

    public ChunkyFileHeader ReadChunkyFileHeader()
    {
        return new ChunkyFileHeader(
            ReadBytes(16),
            ReadInt32(),
            ReadInt32()
        );
    }

    public IEnumerable<IChunkyNode> ReadNodes()
    {
        return ReadNodes(BaseStream.Length - BaseStream.Position);
    }

    public IEnumerable<IChunkyNode> ReadNodes(long length) => StreamEnumerableUtil.WithStreamPosition(BaseStream, ReadNodesImpl(length));

    private IEnumerable<IChunkyNode> ReadNodesImpl(long length)
    {
        foreach (var header in ReadChunkHeaders(length))
        {
            switch (header.Type)
            {
                case "FOLD":
                    yield return new ChunkyFolderNode(header, BaseStream);
                    break;
                case "DATA":
                    yield return new ChunkyStreamDataNode(header, BaseStream);
                    break;
                default:
                    throw new Exception($"Unknown chunk type {header.Type}");
            }
        }
    }
}