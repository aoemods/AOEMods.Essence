namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Entries of an SGA archive.
/// </summary>
/// <param name="Header">Header of the archive.</param>
/// <param name="Tocs">Table of contents entries of the archive.</param>
/// <param name="Folders">Folder entries of the archive.</param>
/// <param name="Files">File entries of the archive.</param>
public record ArchiveEntries(ArchiveHeader Header, IList<ArchiveTocEntry> Tocs, IList<ArchiveFolderEntry> Folders, IList<ArchiveFileEntry> Files);