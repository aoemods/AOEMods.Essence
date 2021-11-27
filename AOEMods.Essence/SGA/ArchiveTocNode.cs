using System.Text;

namespace AOEMods.Essence.SGA;

public class ArchiveTocNode : IArchiveTocNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; }
    public IList<IArchiveNode> Children { get; }

    public ArchiveTocNode(string name, IList<IArchiveNode>? children = null, IArchiveNode? parent = null)
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
