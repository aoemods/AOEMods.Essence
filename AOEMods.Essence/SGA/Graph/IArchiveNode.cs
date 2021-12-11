namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Node of an SGA archive.
/// </summary>
public interface IArchiveNode
{
    /// <summary>
    /// Parent of the node.
    /// </summary>
    IArchiveNode? Parent { get; }

    /// <summary>
    /// Name of the node.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Full name / path of the node.
    /// </summary>
    string FullName => Parent != null && Parent.Name != "" ? $"{Parent.FullName}\\{Name}" : Name;

    /// <summary>
    /// Depth of the node. The root node has depth 0.
    /// </summary>
    int Depth => Parent != null ? Parent.Depth + 1 : 0;
}
