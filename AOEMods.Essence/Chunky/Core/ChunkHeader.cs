namespace AOEMods.Essence.Chunky;

/// <summary>
/// Header of a single chunk of a Relic Chunky file.
/// </summary>
/// <param name="Type">Type of the header, usually DATA or FOLD.</param>
/// <param name="Name">Name of the chunk.</param>
/// <param name="Version">Version of the chunk.</param>
/// <param name="Length">Length of the data of the chunk in bytes.</param>
/// <param name="Path">Path of the chunk.</param>
/// <param name="DataPosition">Position of the data of the chunk in the stream.</param>
public record class ChunkHeader(string Type, string Name, int Version, int Length, string Path, long DataPosition);