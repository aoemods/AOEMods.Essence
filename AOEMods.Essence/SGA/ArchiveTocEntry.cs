namespace AOEMods.Essence.SGA;

public record ArchiveTocEntry(string Alias, string Name, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex, uint FolderRootIndex) : IArchiveEntry;