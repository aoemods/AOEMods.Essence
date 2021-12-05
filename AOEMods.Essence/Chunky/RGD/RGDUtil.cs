using System.Text;

namespace AOEMods.Essence.Chunky.RGD;

public static class RGDUtil
{
    public static readonly char[] Magic = new[] { 'R', 'e', 'l', 'i', 'c', ' ', 'C', 'h', 'u', 'n', 'k', 'y', '\x0D', '\x0A', '\x1A', '\0' };

    public static object ReadType(ChunkyFileReader reader, RGDDataType type)
    {
        return type switch
        {
            RGDDataType.Float => reader.ReadSingle(),
            RGDDataType.Int => reader.ReadInt32(),
            RGDDataType.Boolean => reader.ReadBoolean(),
            RGDDataType.CString => reader.ReadCString(),
            RGDDataType.List or RGDDataType.List2 => ReadChunkyList(reader),
            _ => throw new Exception($"Unknown type {type}")
        };
    }

    public static ChunkyList ReadChunkyList(ChunkyFileReader reader)
    {
        int length = reader.ReadInt32();

        // Read table index
        var keyTypeAndDataIndex = new (ulong key, RGDDataType Type, int index)[length];
        for (int i = 0; i < length; i++)
        {
            ulong key = reader.ReadUInt64();
            RGDDataType type = (RGDDataType)reader.ReadInt32();
            int index = reader.ReadInt32();
            keyTypeAndDataIndex[i] = (key, type, index);
        }

        // Read table row data
        long dataPosition = reader.BaseStream.Position;

        ChunkyList kvs = new();
        foreach (var (key, type, index) in keyTypeAndDataIndex)
        {
            reader.BaseStream.Position = dataPosition + index;
            kvs.Add(new KeyValueEntry(key, ReadType(reader, type)));
        }

        return kvs;
    }

    public static void WriteChunkyList(ChunkyFileWriter writer, IList<KeyValueEntry> list)
    {
        // int count
        // * for count
        // - ulong key
        // - int type
        // - int index (indexes data below)
        // * for count
        // - type[i] data
        MemoryStream dataStream = new();
        ChunkyFileWriter dataWriter = new(dataStream);

        writer.Write(list.Count);
        for (int i = 0; i < list.Count;i++)
        {
            writer.Write(list[i].Key);
            switch (list[i].Value)
            {
                case float dataFloat:
                    writer.Write((int)RGDDataType.Float);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataFloat);
                    break;
                case int dataInt:
                    writer.Write((int)RGDDataType.Int);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataInt);
                    break;
                case bool dataBool:
                    writer.Write((int)RGDDataType.Boolean);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataBool);
                    break;
                case string dataString:
                    writer.Write((int)RGDDataType.CString);
                    writer.Write((int)dataStream.Position);
                    dataWriter.WriteCString(dataString);
                    break;
                case ChunkyList dataList:
                    writer.Write((int)RGDDataType.List);
                    writer.Write((int)dataStream.Position);
                    WriteChunkyList(dataWriter, dataList);
                    break;
                case RGDNode[] nodes:
                    writer.Write((int)RGDDataType.List);
                    writer.Write((int)dataStream.Position);
                    WriteChunkyList(dataWriter, nodes.Select(node => new KeyValueEntry((ulong)((long)node.Key.GetHashCode() + int.MaxValue), node.Value)).ToArray());
                    break;
                default:
                    throw new Exception($"Unknown type {list[i].Value}");
            }
        }

        writer.Write(dataStream.ToArray());
    }

    public static KeyValueDataChunk ReadKeyValueDataChunk(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        int unknown = reader.ReadInt32();
        var table = ReadChunkyList(reader);

        return new KeyValueDataChunk(table);
    }

    public static void WriteKeyValueDataChunk(ChunkyFileWriter writer, KeyValueDataChunk chunk)
    {
        writer.Write(0); // unknown
        WriteChunkyList(writer, chunk.KeyValues);
    }

    public static KeysDataChunk ReadKeysDataChunk(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        Dictionary<string, ulong> stringKeys = new();

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            ulong key = reader.ReadUInt64();

            int stringLength = reader.ReadInt32();
            string str = new string(reader.ReadChars(stringLength));

            stringKeys.Add(str, key);
        }

        return new KeysDataChunk(stringKeys);
    }

    public static void WriteKeysDataChunk(ChunkyFileWriter writer, KeysDataChunk chunk)
    {
        // int count
        // * for count
        // - ulong keyHash
        // - int stringLength
        // - string key

        writer.Write(chunk.StringKeys.Count);
        foreach (var (key, keyHash) in chunk.StringKeys)
        {
            writer.Write(keyHash);
            byte[] keyData = Encoding.UTF8.GetBytes(key);
            writer.Write(keyData.Length);
            writer.Write(keyData);
        }
    }
}
