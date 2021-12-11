using AOEMods.Essence.Chunky.Core;

namespace AOEMods.Essence.Chunky.RGD;

public class RGDReader : IRGDReader
{
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

    public static IList<RGDNode> ReadRGD(Stream stream)
    {
        using var reader = new ChunkyFileReader(stream);

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
}
