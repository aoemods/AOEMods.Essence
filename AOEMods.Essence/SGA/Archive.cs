namespace AOEMods.Essence.SGA;

public class Archive : IArchive
{
    public IArchiveNode? Parent { get; }
    public IList<IArchiveTocNode> Tocs { get; }
    public string Name { get; set; }

    public Archive(string name, IList<IArchiveTocNode> tocs)
    {
        Name = name;
        Tocs = tocs;
    }
}
