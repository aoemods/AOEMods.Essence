namespace AOEMods.Essence.SGA.Core;

public record ArchiveTocEntry(string Alias, string Name, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex, uint FolderRootIndex);