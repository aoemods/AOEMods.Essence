using System.Text;

namespace AOEMods.Essence.Chunky;
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

    public IEnumerable<ChunkHeader> ReadChunkHeaders(long length)
    {
        long startPosition = BaseStream.Position;
        while (BaseStream.Position < startPosition + length)
        {
            var chunkHeader = ReadChunkHeader();

            yield return chunkHeader;

            BaseStream.Position = chunkHeader.DataPosition + chunkHeader.Length;
        }
    }

    public IEnumerable<ChunkHeader> ReadChunkHeaders()
    {
        return ReadChunkHeaders(BaseStream.Length - BaseStream.Position);
    }

    public ChunkHeader ReadChunkHeader()
    {
        return new ChunkHeader(
            new string(ReadChars(4)),
            new string(ReadChars(4)),
            ReadInt32(),
            ReadInt32(),
            Encoding.ASCII.GetString(ReadBytes(ReadInt32())),
            BaseStream.Position
        );
    }

    public ChunkyFileHeader ReadChunkyFileHeader()
    {
        return new ChunkyFileHeader(
            ReadChars(16),
            ReadInt32(),
            ReadInt32()
        );
    }

    public ChunkyFile ReadChunky()
    {
        var fileHeader = ReadChunkyFileHeader();
        return new ChunkyFile(fileHeader, BaseStream, BaseStream.Position, BaseStream.Length - BaseStream.Position);
    }

    public string ReadCString()
    {
        List<char> chars = new List<char>();
        while (true)
        {
            char c = ReadChar();
            if (c != 0)
            {
                chars.Add(c);
            }
            else
            {
                break;
            }
        }

        return new string(chars.ToArray());
    }

    public IEnumerable<IChunkyNode> ReadNodes()
    {
        return ReadNodes(BaseStream.Length - BaseStream.Position);
    }

    public IEnumerable<IChunkyNode> ReadNodes(long length)
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