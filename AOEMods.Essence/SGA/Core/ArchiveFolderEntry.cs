namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Folder entry of an SGA archive.
/// </summary>
/// <param name="NameOffset">Offset of the folder's name in the SGA archive's string blob.</param>
/// <param name="FolderStartIndex">Index of the first child folder.</param>
/// <param name="FolderEndIndex">Index past the last child folder.</param>
/// <param name="FileStartIndex">Index of the first child file.</param>
/// <param name="FileEndIndex">Index past the last child file.</param>
public record ArchiveFolderEntry(uint NameOffset, uint FolderStartIndex, uint FolderEndIndex, uint FileStartIndex, uint FileEndIndex);