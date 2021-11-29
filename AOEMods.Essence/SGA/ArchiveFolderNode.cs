namespace AOEMods.Essence.SGA;

public class ArchiveFolderNode : IArchiveFolderNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; set; }
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
