namespace AOEMods.Essence.SGA;

public record ArchiveEntries(ArchiveHeader Header, IList<ArchiveTocEntry> Tocs, IList<ArchiveFolderEntry> Folders, IList<ArchiveFileEntry> Files);