using AOEMods.Essence.SGA.Graph;

namespace AOEMods.Essence.SGA;

/// <summary>
/// Table of contents of an SGA archive. The table of contents is not guaranteed
/// to be up to date (eg. after modifying the node graph).
/// The RebuildFromRootFolder function can be called to refresh it.
/// </summary>
public interface IArchiveToc
{
    /// <summary>
    /// All folders contained in the archive.
    /// </summary>
    IList<IArchiveFolderNode> Folders { get; }

    /// <summary>
    /// All files contained in the archive.
    /// </summary>
    IList<IArchiveFileNode> Files { get; }

    /// <summary>
    /// All files in the archive indexed by their full name.
    /// </summary>
    IReadOnlyDictionary<string, IArchiveFileNode> FilesByPath { get; }

    /// <summary>
    /// Root folder of the archive.
    /// </summary>
    IArchiveFolderNode RootFolder { get; }

    /// <summary>
    /// Name of the table of contents.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Alias of the table of contents.
    /// </summary>
    string Alias { get; }

    /// <summary>
    /// Rebuilds the table of contents from the actual files and folders
    /// contained in the archive graph.
    /// </summary>
    void RebuildFromRootFolder();
}
