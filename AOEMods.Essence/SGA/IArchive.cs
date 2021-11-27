namespace AOEMods.Essence.SGA;

public interface IArchive
{
    IList<IArchiveTocNode> Tocs { get; }
    public string Name { get; }
}
