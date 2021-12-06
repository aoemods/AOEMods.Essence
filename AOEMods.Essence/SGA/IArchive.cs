namespace AOEMods.Essence.SGA;

public interface IArchive
{
    IList<IArchiveToc> Tocs { get; }
    public string Name { get; set; }
    public byte[] Signature { get; set; }
    public IArchiveFileNode? FindFile(string path)
    {
        string sanitizedPath = path.Replace('/', '\\');
        foreach (var toc in Tocs)
        {
            if (toc.FilesByPath.TryGetValue(sanitizedPath, out var file))
            {
                return file;
            }
        }

        return null;
    }
}
