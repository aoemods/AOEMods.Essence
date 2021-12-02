namespace AOEMods.Essence.SGA;

public static class ArchiveReaderHelper
{
    public static IArchive DirectoryToArchive(string rootDirectoryPath, string archiveName)
    {
        IArchiveFileNode FilePathToNode(string filePath, IArchiveNode parent)
        {
            return new ArchiveStoredFileNode(Path.GetFileName(filePath),filePath, parent);
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
