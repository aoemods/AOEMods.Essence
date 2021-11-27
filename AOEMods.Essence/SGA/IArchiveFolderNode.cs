namespace AOEMods.Essence.SGA;

public interface IArchiveFolderNode : IArchiveNode
{
    IList<IArchiveNode> Children { get; }
}
