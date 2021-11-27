using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMods.Essence.SGA;

public static class ArchiveReaderHelper
{
    public static IArchive DirectoryToArchive(string rootDirectoryPath, string archiveName)
    {
        IArchiveFileNode FilePathToNode(string filePath, IArchiveNode parent)
        {
            return new ArchiveStoredFileNode(Path.GetFileName(filePath), File.ReadAllBytes(filePath), parent);
        }

        IArchiveFolderNode DirectoryPathToNode(string directoryPath, IArchiveNode parent)
        {
            var folderNode = new ArchiveFolderNode(Path.GetRelativePath(rootDirectoryPath, directoryPath), parent: parent);

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

        IArchiveTocNode TocDirectoryPathToNode(string directoryPath)
        {
            var tocNode = new ArchiveTocNode(archiveName);

            foreach (string childDirectoryPath in Directory.GetDirectories(directoryPath))
            {
                tocNode.Children.Add(DirectoryPathToNode(childDirectoryPath, tocNode));
            }

            foreach (string childFilePath in Directory.GetFiles(directoryPath))
            {
                tocNode.Children.Add(FilePathToNode(childFilePath, tocNode));
            }

            return tocNode;
        }

        return new Archive(archiveName, new IArchiveTocNode[] { TocDirectoryPathToNode(rootDirectoryPath) });
    }
}
