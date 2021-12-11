using AOEMods.Essence.Chunky.Core;
using System.Text;

namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Reads Relic Game Data (RGD) files.
/// </summary>
public class RGDReader : IRGDReader
{
    /// <summary>
    /// Reads a list of RGD nodes from a stream containing an RGD file.
    /// </summary>
    /// <param name="stream">Stream containing an RGD file.</param>
    /// <returns>RGD nodes read from the stream.</returns>
    /// <exception cref="Exception">Thrown if zero or more than DATA KEYS or AEGD chunks are present.</exception>
    public static IList<RGDNode> ReadRGD(Stream stream)
    {
        using var reader = new ChunkyFileReader(stream, Encoding.UTF8, true);

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

        var keys = ReadKeysDataChunk(reader, keysHeaders[0]);
        var kvs = ReadKeyValueDataChunk(reader, kvsHeaders[0]);

        var keysInv = ReverseReadOnlyDictionary(keys.StringKeys);

        static RGDNode MakeNode(ulong key, object value, IReadOnlyDictionary<ulong, string> keysInv)
        {
            string keyStr = keysInv[key];

            if (value is RGDList table)
            {
                return new RGDNode(keyStr, table.Select(listItem => MakeNode(listItem.Key, listItem.Value, keysInv)).ToArray());
            }

            return new RGDNode(keyStr, value);
        }

        return kvs.KeyValues.Select(kv => MakeNode(kv.Key, kv.Value, keysInv)).ToArray();
    }

    private static IReadOnlyDictionary<TValue, TKey> ReverseReadOnlyDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source) where TValue : notnull
    {
        var dictionary = new Dictionary<TValue, TKey>();
        foreach (var entry in source)
        {
            if (!dictionary.ContainsKey(entry.Value))
                dictionary.Add(entry.Value, entry.Key);
        }
        return dictionary;
    }

    private static object ReadType(ChunkyFileReader reader, RGDDataType type)
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

    private static RGDList ReadChunkyList(ChunkyFileReader reader)
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

        RGDList kvs = new();
        foreach (var (key, type, index) in keyTypeAndDataIndex)
        {
            reader.BaseStream.Position = dataPosition + index;
            kvs.Add(new KeyValueEntry(key, ReadType(reader, type)));
        }

        return kvs;
    }

    private static KeyValueDataChunk ReadKeyValueDataChunk(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;

        int unknown = reader.ReadInt32();
        var table = ReadChunkyList(reader);

        return new KeyValueDataChunk(table);
    }

    private static KeysDataChunk ReadKeysDataChunk(ChunkyFileReader reader, ChunkHeader header)
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
}
