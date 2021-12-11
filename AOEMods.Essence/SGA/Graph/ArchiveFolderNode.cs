namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Folder node of an SGA archive containing child nodes.
/// </summary>
public class ArchiveFolderNode : IArchiveFolderNode
{
    /// <summary>
    /// Parent of the node.
    /// </summary>
    public IArchiveNode? Parent { get; }

    /// <summary>
    /// Name of the node.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Child nodes of the folder node.
    /// </summary>
    public IList<IArchiveNode> Children { get; }

    public ArchiveFolderNode(string name, IList<IArchiveNode>? children = null, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;

        if (children != null)
        {
            Children = children;
        }
        else
        {
            Children = new List<IArchiveNode>();
        }
    }
}
