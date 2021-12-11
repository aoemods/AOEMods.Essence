namespace AOEMods.Essence.SGA.Core;

public record ArchiveEntries(ArchiveHeader Header, IList<ArchiveTocEntry> Tocs, IList<ArchiveFolderEntry> Folders, IList<ArchiveFileEntry> Files);