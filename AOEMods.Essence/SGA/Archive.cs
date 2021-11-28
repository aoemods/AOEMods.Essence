namespace AOEMods.Essence.SGA;

public class Archive : IArchive
{
    public IArchiveNode? Parent { get; }
    public IList<IArchiveToc> Tocs { get; }
    public string Name { get; set; }
    public byte[] Signature { get; set; }

    public Archive(string name, IList<IArchiveToc> tocs, byte[] signature)
    {
        Name = name;
        Tocs = tocs;
        Signature = signature;
    }
}
