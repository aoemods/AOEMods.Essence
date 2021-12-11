namespace AOEMods.Essence.Chunky;

/// <summary>
/// DATA KEYS chunk of a Relic Game Data (RGD) file.
/// </summary>
/// <param name="StringKeys">Keys and their associated ids that are referenced in the DATA AEGD chunk.</param>
public record class KeysDataChunk(IReadOnlyDictionary<string, ulong> StringKeys);