namespace AOEMods.Essence.SGA;

public class ArchiveStoredFileNode : IArchiveFileNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; }
    private byte[] data;

    public ArchiveStoredFileNode(string name, byte[] data, IArchiveNode? parent = null)
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
