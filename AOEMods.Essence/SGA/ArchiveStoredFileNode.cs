using System.Text;

namespace AOEMods.Essence.SGA;

public class ArchiveStoredFileNode : IArchiveFileNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; set; }
    private string filePath;

    public ArchiveStoredFileNode(string name, string filePath, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;
        this.filePath = filePath;
    }

    public IEnumerable<byte> GetData()
    {
        return File.ReadAllBytes(filePath);
    }
}
