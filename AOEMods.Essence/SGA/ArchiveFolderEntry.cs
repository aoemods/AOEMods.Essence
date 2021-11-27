namespace AOEMods.Essence.SGA;

public record ArchiveFolderEntry(uint NameOffset, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex) : IArchiveEntry;