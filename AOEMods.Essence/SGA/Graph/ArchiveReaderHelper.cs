namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Helper functions for reading archives.
/// </summary>
public static class ArchiveReaderHelper
{
    /// <summary>
    /// Creates an archive from a directory.
    /// </summary>
    /// <param name="rootDirectoryPath">Directory path to create archive from.</param>
    /// <param name="archiveName">Name of the archive to create.</param>
    /// <returns>Archive created from the given directory.</returns>
    public static IArchive DirectoryToArchive(string rootDirectoryPath, string archiveName)
    {
        IArchiveFileNode FilePathToNode(string filePath, IArchiveNode parent)
        {
            return new ArchiveStoredFileNode(Path.GetFileName(filePath), filePath, parent);
        }

        IArchiveFolderNode DirectoryPathToNode(string directoryPath, IArchiveNode? parent)
        {
            var folderNode = new ArchiveFolderNode(
                rootDirectoryPath == directoryPath ? "" : new DirectoryInfo(directoryPath).Name,
                parent: parent
            );

            foreach (string childDirectoryPath in Directory.GetDirectories(directoryPath))
            {
                folderNode.Children.Add(DirectoryPathToNode(childDirectoryPath, folderNode));
            }

            foreach (string childFilePath in Directory.GetFiles(directoryPath))
            {
                folderNode.Children.Add(FilePathToNode(childFilePath, folderNode));
            }

            return folderNode;
        }

        var rootFolder = DirectoryPathToNode(rootDirectoryPath, null);
        var toc = new ArchiveToc(archiveName, archiveName, rootFolder);

        return new Archive(archiveName, new IArchiveToc[] { toc }, new byte[256]);
    }
}
