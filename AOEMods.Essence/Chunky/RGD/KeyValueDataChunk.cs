namespace AOEMods.Essence.Chunky;


/// <summary>
/// Entry of a DATA AEGD chunk of a Relic Game Data (RGD) chunk.
/// </summary>
/// <param name="Key">Id of the entry. The id's string representation can be found in the DATA KEYS chunk.</param>
/// <param name="Value">Value of the entry. Can be one of RGDDataType.</param>
public record KeyValueEntry(ulong Key, object Value);

/// <summary>
/// DATA AEGD chunk of a Relic Game Data (RGD) chunk.
/// </summary>
/// <param name="KeyValues">List of values associated with ids. The ids' string representation can be found in the DATA KEYS chunk.</param>
public record KeyValueDataChunk(IList<KeyValueEntry> KeyValues);