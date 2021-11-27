namespace AOEMods.Essence.Chunky;

public record class ChunkyFileHeader(char[] Magic, int Version, int Platform);