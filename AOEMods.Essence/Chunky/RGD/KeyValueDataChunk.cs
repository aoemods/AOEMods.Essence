namespace AOEMods.Essence.Chunky;

public record KeyValueEntry(ulong Key, object Value);
public record KeyValueDataChunk(IList<KeyValueEntry> KeyValues);