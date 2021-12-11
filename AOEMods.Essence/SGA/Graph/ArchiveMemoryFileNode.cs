namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// File node of an SGA archive containing data that is stored in memory.
/// </summary>
public class ArchiveMemoryFileNode : IArchiveFileNode
{
    /// <summary>
    /// Parent of the node.
    /// </summary>
    public IArchiveNode? Parent { get; }

    /// <summary>
    /// Name of the node.
    /// </summary>
    public string Name { get; set; }

    private byte[] data;

    /// <summary>
    /// Initializes an ArchiveMemoryFileNode from an array of bytes.
    /// </summary>
    /// <param name="name">Name of the node.</param>
    /// <param name="data">Data of the file node.</param>
    /// <param name="parent">Parent of the node.</param>
    public ArchiveMemoryFileNode(string name, byte[] data, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;
        this.data = data;
    }

    /// <summary>
    /// Reads and returns the in-memory data of the file node.
    /// </summary>
    /// <returns>Data of the file.</returns>
    public IEnumerable<byte> GetData()
    {
        return data;
    }
}
