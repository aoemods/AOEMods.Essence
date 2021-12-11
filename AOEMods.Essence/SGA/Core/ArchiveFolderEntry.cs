namespace AOEMods.Essence.SGA.Core;

public record ArchiveFolderEntry(uint NameOffset, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex);