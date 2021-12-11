using AOEMods.Essence.SGA.Graph;

namespace AOEMods.Essence.SGA;

public interface IArchiveFolderNode : IArchiveNode
{
    IList<IArchiveNode> Children { get; }
}
