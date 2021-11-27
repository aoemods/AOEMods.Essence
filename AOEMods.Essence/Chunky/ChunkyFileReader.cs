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

    private object ReadType(int type)
    {
        return type switch
        {
            0 => ReadSingle(),
            1 => ReadInt32(),
            2 => ReadBoolean(),
            3 => ReadCString(),
            100 => ReadChunkyList(),
            101 => ReadChunkyList(),
            _ => throw new Exception($"Unknown type {type}")
        };
    }

    private ChunkyList ReadChunkyList()
    {
        int length = ReadInt32();

        // Read table index
        var keyTypeAndDataIndex = new (ulong key, int Type, int index)[length];
        for (int i = 0; i < length; i++)
        {
            ulong key = ReadUInt64();
            int type = ReadInt32();
            int index = ReadInt32();
            keyTypeAndDataIndex[i] = (key, type, index);
        }

        // Read table row data
        long dataPosition = BaseStream.Position;

        ChunkyList kvs = new();
        foreach (var (key, type, index) in keyTypeAndDataIndex)
        {
            BaseStream.Position = dataPosition + index;
            kvs.Add(new KeyValueEntry(key, ReadType(type)));
        }

        return kvs;
    }

    public KeyValueDataChunk ReadKeyValueDataChunk(ChunkHeader header)
    {
        BaseStream.Position = header.DataPosition;

        int unknown = ReadInt32();
        var table = ReadChunkyList();

        return new KeyValueDataChunk(table);
    }

    public KeysDataChunk ReadKeysDataChunk(ChunkHeader header)
    {
        BaseStream.Position = header.DataPosition;

        Dictionary<string, ulong> stringKeys = new();

        int count = ReadInt32();

        for (int i = 0; i < count; i++)
        {
            ulong key = ReadUInt64();

            int stringLength = ReadInt32();
            string str = new string(ReadChars(stringLength));

            stringKeys.Add(str, key);
        }

        return new KeysDataChunk(stringKeys);
    }
}