namespace AOEMods.Essence.SGA;

public interface IArchive
{
    IList<IArchiveToc> Tocs { get; }
    public string Name { get; set; }
    public byte[] Signature { get; set; }
}
