namespace AOEMods.Essence.SGA;

public class Archive : IArchive
{
    public IArchiveNode? Parent { get; }
    public IList<IArchiveToc> Tocs { get; }
    public string Name { get; set; }

    public Archive(string name, IList<IArchiveToc> tocs)
    {
        Name = name;
        Tocs = tocs;
    }
}
