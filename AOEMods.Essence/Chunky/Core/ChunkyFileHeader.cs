namespace AOEMods.Essence.Chunky.Core;

/// <summary>
/// Header of the Relic Chunky format.
/// </summary>
/// <param name="Magic">Magic value of the chunky file.</param>
/// <param name="Version">Version of the chunky file.</param>
/// <param name="Platform">Platform of the chunky file.</param>
public record class ChunkyFileHeader(byte[] Magic, int Version, int Platform);