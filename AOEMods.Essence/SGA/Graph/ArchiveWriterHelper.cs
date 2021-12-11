using AOEMods.Essence.SGA.Core;

namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Helper functions for writing archives.
/// </summary>
public static class ArchiveWriterHelper
{
    private static readonly byte[] Magic = new byte[] { 95, 65, 82, 67, 72, 73, 86, 69 };
    private const ushort Version = 10;
    private const ushort Product = 0;
    private const uint BlockSize = 262144;
    private const ulong HeaderSize = 428UL;
    private const uint HeaderExtraSize = 44;

    /// <summary>
    /// Writes an archive to a stream and advances the stream's position.
    /// </summary>
    /// <param name="stream">Stream to write the archive to.</param>
    /// <param name="archive">Archive to write to the stream.</param>
    public static void WriteArchiveToStream(Stream stream, IArchive archive)
    {
        ArchiveWriter archiveWriter = new(stream);

        var toc = archive.Tocs[0];
        toc.RebuildFromRootFolder();

        var rootFolder = toc.RootFolder;
        var fileNodes = toc.Files;
        var folderNodes = toc.Folders;

        MemoryStream contentStream = new();
        ArchiveWriter writer = new(contentStream);

        var fileDataOffsets = fileNodes.Select(fileNode =>
        {
            var fileData = fileNode.GetData().ToArray();
            return (writer.AddData(fileData), fileData.Length);
        }).ToArray();

        var directoryNameOffsets = folderNodes.Select(node => writer.AddString(node.FullName)).ToArray();
        var fileNameOffsets = fileNodes.Select(file => writer.AddString(file.Name)).ToArray();

        // Write files
        long fileEntryOffset = writer.BaseStream.Position;
        for (int i = 0; i < fileNodes.Count; i++)
        {
            ulong fileDataOffset = (ulong)fileDataOffsets[i].Item1;
            uint fileDataLength = (uint)fileDataOffsets[i].Item2;
            uint fileNameOffset = (uint)fileNameOffsets[i];

            writer.Write(new ArchiveFileEntry(
                fileNameOffset, 0, fileDataOffset,
                fileDataLength, fileDataLength,
                0, 0, 0
            ));
        }

        // Write folders
        long folderEntryOffset = writer.BaseStream.Position;
        for (int i = 0; i < folderNodes.Count; i++)
        {
            var dir = folderNodes[i];
            var dirFiles = dir.Children.OfType<IArchiveFileNode>().ToArray();
            var dirDirectories = dir.Children.OfType<IArchiveFolderNode>().ToArray();
            uint dirNameOffset = (uint)directoryNameOffsets[i];

            writer.Write(new ArchiveFolderEntry(
                dirNameOffset,
                dirDirectories.Length == 0 ? 0 : (uint)folderNodes.IndexOf(dirDirectories.First()),
                dirDirectories.Length == 0 ? 0 : (uint)folderNodes.IndexOf(dirDirectories.Last()) + 1,
                dirFiles.Length == 0 ? 0 : (uint)fileNodes.IndexOf(dirFiles.First()),
                dirFiles.Length == 0 ? 0 : (uint)fileNodes.IndexOf(dirFiles.Last()) + 1
            ));
        }

        // Write toc
        long tocEntryOffset = writer.BaseStream.Position;

        writer.Write(new ArchiveTocEntry(
            toc.Alias, toc.Name,
            0, (uint)toc.Files.Count,
            0, (uint)toc.Folders.Count,
            (uint)folderNodes.IndexOf(toc.RootFolder)
        ));

        long stringsOffset = writer.BaseStream.Position;
        writer.WriteStrings();
        long stringLength = writer.BaseStream.Position - stringsOffset;

        long headerBlobLength = writer.BaseStream.Position + 44;

        long dataOffset = writer.BaseStream.Position;
        writer.WriteData();
        long dataBlobLength = writer.BaseStream.Position - dataOffset;

        // Write the actual file

        // Write header
        archiveWriter.Write(new ArchiveHeader(
            Magic, Version, Product, archive.Name,
            HeaderSize, (uint)headerBlobLength,
            HeaderSize + (ulong)dataOffset + HeaderExtraSize, (ulong)dataBlobLength,
            (uint)tocEntryOffset + HeaderExtraSize, 1,
            (uint)folderEntryOffset + HeaderExtraSize, (uint)folderNodes.Count,
            (uint)fileEntryOffset + HeaderExtraSize, (uint)fileNodes.Count,
            (uint)stringsOffset + HeaderExtraSize, (uint)stringLength,
            BlockSize, archive.Signature
        ));

        archiveWriter.Write(contentStream.ToArray());
    }
}
