using AOEMods.Essence.SGA.Core;

namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// SGA archive containing archive nodes.
/// </summary>
public class Archive : IArchive
{
    /// <summary>
    /// Table of contents of the archive.
    /// </summary>
    public IList<IArchiveToc> Tocs { get; }

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
    /// Initializes an Archive from its name, table of contents and signature.
    /// </summary>
    /// <param name="name">Name of the archive.</param>
    /// <param name="tocs">Table of contents of the archive.</param>
    /// <param name="signature">Signature of the archive.</param>
    public Archive(string name, IList<IArchiveToc> tocs, byte[] signature)
    {
        Name = name;
        Tocs = tocs;
        Signature = signature;
    }

    public static Archive FromStream(Stream stream)
    {
        ArchiveReader reader = new(stream);

        ArchiveEntries archive = reader.ReadArchiveEntries();

        IArchiveToc FromTocEntry(ArchiveTocEntry tocEntry, IArchiveNode? parent)
        {
            var folderEntry = archive.Folders[(int)tocEntry.FolderRootIndex];

            stream.Position = (long)(archive.Header.Offset + archive.Header.StringOffset + folderEntry.NameOffset);
            string name = reader.ReadCString();
            int lastSeparatorIndex = name.LastIndexOfAny(new char[] { '/', '\\' });
            if (lastSeparatorIndex >= 0)
            {
                name = name.Substring(lastSeparatorIndex + 1);
            }

            var rootFolder = new ArchiveFolderNode(name, parent: parent);

            for (uint folderIndex = folderEntry.FolderStartIndex; folderIndex < folderEntry.FolderEndIndex; folderIndex++)
            {
                rootFolder.Children.Add(FromFolderEntry(archive.Folders[(int)folderIndex], rootFolder));
            }

            for (uint fileIndex = folderEntry.FileStartIndex; fileIndex < folderEntry.FileEndIndex; fileIndex++)
            {
                rootFolder.Children.Add(FromFileEntry(archive.Files[(int)fileIndex], rootFolder));
            }

            var toc = new ArchiveToc(tocEntry.Name, tocEntry.Alias, rootFolder);

            return toc;
        }

        IArchiveFolderNode FromFolderEntry(ArchiveFolderEntry folderEntry, IArchiveNode? parent)
        {
            stream.Position = (long)(archive.Header.Offset + archive.Header.StringOffset + folderEntry.NameOffset);
            string name = reader.ReadCString();
            int lastSeparatorIndex = name.LastIndexOfAny(new char[] { '/', '\\' });
            if (lastSeparatorIndex >= 0)
            {
                name = name.Substring(lastSeparatorIndex + 1);
            }

            var folderNode = new ArchiveFolderNode(name, parent: parent);

            for (uint folderIndex = folderEntry.FolderStartIndex; folderIndex < folderEntry.FolderEndIndex; folderIndex++)
            {
                folderNode.Children.Add(FromFolderEntry(archive.Folders[(int)folderIndex], folderNode));
            }

            for (uint fileIndex = folderEntry.FileStartIndex; fileIndex < folderEntry.FileEndIndex; fileIndex++)
            {
                folderNode.Children.Add(FromFileEntry(archive.Files[(int)fileIndex], folderNode));
            }

            return folderNode;
        }

        IArchiveFileNode FromFileEntry(ArchiveFileEntry fileEntry, IArchiveNode? parent)
        {
            stream.Position = (long)(archive.Header.Offset + archive.Header.StringOffset + fileEntry.NameOffset);
            string name = reader.ReadCString();

            return new ArchiveFileNode(
                stream, name, (long)(archive.Header.DataOffset + fileEntry.DataOffset),
                fileEntry.CompressedLength, fileEntry.UncompressedSize, fileEntry.StorageType, parent
            );
        }

        return new Archive(archive.Header.NiceName, archive.Tocs.Select(toc => FromTocEntry(toc, null)).ToArray(), archive.Header.Signature);
    }
}
