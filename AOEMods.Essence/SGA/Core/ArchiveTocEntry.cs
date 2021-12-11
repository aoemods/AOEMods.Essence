namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Table of contents entry of an SGA archive.
/// </summary>
/// <param name="Alias">Alias of the table of contents.</param>
/// <param name="Name">Name of the table of contents.</param>
/// <param name="FolderStartIndex">Index of the first child folder.</param>
/// <param name="FolderEndIndex">Index past the last child folder.</param>
/// <param name="FileStartIndex">Index of the first child file.</param>
/// <param name="FileEndIndex">Index past the last child file.</param>
/// <param name="FolderRootIndex">Index of the root folder.</param>
public record ArchiveTocEntry(string Alias, string Name, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex, uint FolderRootIndex);