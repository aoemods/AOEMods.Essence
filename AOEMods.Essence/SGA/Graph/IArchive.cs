namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// SGA archive containing archive nodes.
/// </summary>
public interface IArchive
{
    /// <summary>
    /// Table of contents of the archive.
    /// </summary>
    IList<IArchiveToc> Tocs { get; }

    /// <summary>
    /// Name of the archive.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 2048 bit (256 byte) signature of the archive. Probably using PKCS#1 in official archives.
    /// Also validated in the game by XORing together 16 byte chunks and comparing against
    /// known values.
    /// </summary>
    public byte[] Signature { get; set; }

    /// <summary>
    /// Finds a file with the given path in the table of contents.
    /// Note that the table of contents is not guaranteed to be up to date
    /// and might need refreshing when the node graph was modified.
    /// </summary>
    /// <param name="path">Path of the file to find.</param>
    /// <returns>File node if found or null if not found.</returns>
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
