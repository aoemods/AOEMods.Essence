namespace AOEMods.Essence.SGA;

public class ArchiveToc : IArchiveToc
{
    public string Name { get; }
    public string Alias { get; }
    public IArchiveFolderNode RootFolder { get; }
    public IList<IArchiveFileNode> Files { get; }
    public IList<IArchiveFolderNode> Folders { get; }

    public ArchiveToc(string name, string alias, IArchiveFolderNode rootFolder)
    {
        Name = name;
        Alias = alias;
        RootFolder = rootFolder;

        var allNodes = ArchiveNodeHelper.EnumerateNodes(rootFolder);
        Files = allNodes.OfType<IArchiveFileNode>().ToArray();
        Folders = allNodes.OfType<IArchiveFolderNode>().ToArray();
    }
}
