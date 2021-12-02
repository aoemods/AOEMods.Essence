namespace AOEMods.Essence.SGA;

public class ArchiveMemoryFileNode : IArchiveFileNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; set; }
    private byte[] data;

    public ArchiveMemoryFileNode(string name, byte[] data, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;
        this.data = data;
    }

    public IEnumerable<byte> GetData()
    {
        return data;
    }
}
