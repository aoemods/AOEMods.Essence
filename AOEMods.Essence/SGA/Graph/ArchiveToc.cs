namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Table of contents of an SGA archive. The table of contents is not guaranteed
/// to be up to date (eg. after modifying the node graph).
/// The RebuildFromRootFolder function can be called to refresh it.
/// </summary>
public class ArchiveToc : IArchiveToc
{
    /// <summary>
    /// Name of the table of contents.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Alias of the table of contents.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Root folder of the archive.
    /// </summary>
    public IArchiveFolderNode RootFolder { get; }

    /// <summary>
    /// All files contained in the archive.
    /// </summary>
    public IList<IArchiveFileNode> Files { get; private set; }

    /// <summary>
    /// All folders contained in the archive.
    /// </summary>
    public IList<IArchiveFolderNode> Folders { get; private set; }

    /// <summary>
    /// All files in the archive indexed by their full name.
    /// </summary>
    public IReadOnlyDictionary<string, IArchiveFileNode> FilesByPath => filesByPath;

    private Dictionary<string, IArchiveFileNode> filesByPath;

    /// <summary>
    /// Initializes an archive table of contents from its name, alias and root folder node.
    /// </summary>
    /// <param name="name">Name of the table of contents.</param>
    /// <param name="alias">Alias of the table of contents.</param>
    /// <param name="rootFolder">Root folder of the table of contents.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ArchiveToc(string name, string alias, IArchiveFolderNode rootFolder)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Name = name;
        Alias = alias;
        RootFolder = rootFolder;

        RebuildFromRootFolder();
    }

    /// <summary>
    /// Rebuilds the table of contents from the actual files and folders
    /// contained in the archive graph.
    /// </summary>
    public void RebuildFromRootFolder()
    {
        var allNodes = (new[] { RootFolder }).Concat(ArchiveNodeHelper.EnumerateChildren(RootFolder));
        Files = allNodes.OfType<IArchiveFileNode>().ToArray();
        Folders = allNodes.OfType<IArchiveFolderNode>().ToArray();
        filesByPath = Files.DistinctBy(file => file.FullName).ToDictionary(file => file.FullName);
    }
}
