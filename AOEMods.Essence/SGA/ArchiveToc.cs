namespace AOEMods.Essence.SGA;

public class ArchiveToc : IArchiveToc
{
    public string Name { get; }
    public string Alias { get; }
    public IArchiveFolderNode RootFolder { get; }
    public IList<IArchiveFileNode> Files { get; private set; }
    public IList<IArchiveFolderNode> Folders { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ArchiveToc(string name, string alias, IArchiveFolderNode rootFolder)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Name = name;
        Alias = alias;
        RootFolder = rootFolder;

        RebuildFromRootFolder();
    }

    public void RebuildFromRootFolder()
    {
        var allNodes = (new[] { RootFolder }).Concat(ArchiveNodeHelper.EnumerateChildren(RootFolder));
        Files = allNodes.OfType<IArchiveFileNode>().ToArray();
        Folders = allNodes.OfType<IArchiveFolderNode>().ToArray();
    }
}
