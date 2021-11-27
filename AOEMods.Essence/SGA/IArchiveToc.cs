namespace AOEMods.Essence.SGA;

public interface IArchiveToc
{
    IList<IArchiveFolderNode> Folders { get; }
    IList<IArchiveFileNode> Files { get; }
    IArchiveFolderNode RootFolder { get; }
    string Name { get; }
    string Alias { get; }
}
