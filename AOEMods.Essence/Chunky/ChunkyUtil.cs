namespace AOEMods.Essence.Chunky;

public static class ChunkyUtil
{
    public static IReadOnlyDictionary<TValue, TKey> ReverseReadOnlyDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source) where TValue : notnull
    {
        var dictionary = new Dictionary<TValue, TKey>();
        foreach (var entry in source)
        {
            if (!dictionary.ContainsKey(entry.Value))
                dictionary.Add(entry.Value, entry.Key);
        }
        return dictionary;
    }

    public static IEnumerable<ChunkHeader> EnumerateChunkHeaders(ChunkyFileReader reader, ChunkHeader header)
    {
        reader.BaseStream.Position = header.DataPosition;
        foreach (var childHeader in reader.ReadChunkHeaders(header.Length))
        {
            yield return childHeader;
        }
    }
}