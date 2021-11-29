namespace AOEMods.Essence.SGA;

public interface IArchiveToc
{
    public IList<IArchiveFolderNode> Folders { get; }
    public IList<IArchiveFileNode> Files { get; }
    IArchiveFolderNode RootFolder { get; }
    string Name { get; }
    string Alias { get; }
    public void RebuildFromRootFolder();
}
