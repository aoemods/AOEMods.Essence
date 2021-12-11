namespace AOEMods.Essence.Chunky.Core;

public record class ChunkyFileHeader(byte[] Magic, int Version, int Platform);