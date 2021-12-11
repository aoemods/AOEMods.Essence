using AOEMods.Essence.SGA.Graph;

namespace AOEMods.Essence.SGA;

/// <summary>
/// Folder node of an SGA archive containing child nodes.
/// </summary>
public interface IArchiveFolderNode : IArchiveNode
{
    /// <summary>
    /// Child nodes of the folder node.
    /// </summary>
    IList<IArchiveNode> Children { get; }
}
