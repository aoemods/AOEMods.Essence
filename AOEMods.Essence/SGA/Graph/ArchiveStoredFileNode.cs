namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// File node of an SGA archive containing a file path to read its data from.
/// </summary>
public class ArchiveStoredFileNode : IArchiveFileNode
{
    /// <summary>
    /// Parent of the node.
    /// </summary>
    public IArchiveNode? Parent { get; }

    /// <summary>
    /// Name of the node.
    /// </summary>
    public string Name { get; set; }

    private string filePath;

    public ArchiveStoredFileNode(string name, string filePath, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;
        this.filePath = filePath;
    }

    /// <summary>
    /// Reads and returns the data of the file node from its file path.
    /// </summary>
    /// <returns>Data of the file.</returns>
    public IEnumerable<byte> GetData()
    {
        return File.ReadAllBytes(filePath);
    }
}
