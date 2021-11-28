using System.Text;

namespace AOEMods.Essence.SGA;

public class ArchiveWriter : BinaryWriter
{
    private static readonly byte[] Magic = new byte[] { 95, 65, 82, 67, 72, 73, 86, 69 };
    private const ushort Version = 10;
    private const ushort Product = 0;
    private const uint BlockSize = 262144;
    private const ulong HeaderSize = 428UL;
    private const uint HeaderExtraSize = 44;

    public ArchiveWriter(Stream output) : base(output)
    {
    }

    public ArchiveWriter(Stream output, Encoding encoding) : base(output, encoding)
    {
    }

    public ArchiveWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
    {
    }

    protected ArchiveWriter()
    {
    }

    private MemoryStream archiveData = new();
    private MemoryStream archiveStrings = new();

    public long AddString(string s)
    {
        long stringOffset = archiveStrings.Length;
        archiveStrings.Write(Encoding.ASCII.GetBytes(s));
        archiveStrings.WriteByte(0);
        return stringOffset;
    }

    public long AddData(byte[] data)
    {
        long offset = archiveData.Length;
        archiveData.Write(data);
        return offset;
    }

    public void WriteStrings()
    {
        Write(archiveStrings.ToArray());
    }

    public void WriteData()
    {
        Write(archiveData.ToArray());
    }

    public void WriteFixedString(string s, int charCount, int charSize)
    {
        var nameBytes = charSize switch
        {
            1 => Encoding.ASCII.GetBytes(s),
            2 => Encoding.Unicode.GetBytes(s),
            _ => throw new NotImplementedException()
        };
        Write(nameBytes);
        Write(new byte[charCount * charSize - nameBytes.Length]);
    }

    public void Write(ArchiveHeader header)
    {
        Write(header.Magic); // 0
        Write(header.Version); // 8
        Write(header.Product); // 10
        WriteFixedString(header.NiceName, 64, 2); // 12
        Write(header.Offset); // 140 header blob offset
        Write(header.HeaderBlobLength); // 148 num2 / header blob length
        Write(header.DataOffset); // 152 data blob offset
        Write(header.DataBlobLength); // 160 data blob length
        Write(1U); // 168 unk2 == 1

        Write(header.Signature);
        if (header.Offset != (ulong)BaseStream.Position)
        {
            throw new Exception("Header length or offset incorrect.");
        }

        Write(new byte[header.Offset - (ulong)BaseStream.Position]);

        Write(header.TocDataOffset); // 256
        Write(header.TocDataCount); // 260
        Write(header.FolderDataOffset); // 264
        Write(header.FolderDataCount); // 268
        Write(header.FileDataOffset); // 272
        Write(header.FileDataCount); // 276
        Write(header.StringOffset); // 280
        Write(header.StringLength); // 284

        Write(0U); // 288 file hash offset
        Write(0U); // 292 file hash length

        Write(header.BlockSize); // 296
    }

    public void Write(ArchiveTocEntry tocEntry)
    {
        WriteFixedString(tocEntry.Alias, 64, 1);
        WriteFixedString(tocEntry.Name, 64, 1);
        Write(tocEntry.FolderStartIndex);
        Write(tocEntry.FolderEndIndex);
        Write(tocEntry.FileStartIndex);
        Write(tocEntry.FileEndIndex);
        Write(tocEntry.FolderRootIndex);
    }

    public void Write(ArchiveFolderEntry folderEntry)
    {
        Write(folderEntry.NameOffset);
        Write(folderEntry.FolderStartIndex);
        Write(folderEntry.FolderEndIndex);
        Write(folderEntry.FileStartIndex);
        Write(folderEntry.FileEndIndex);
    }

    public void Write(ArchiveFileEntry fileEntry)
    {
        Write(fileEntry.NameOffset);
        Write(fileEntry.HashOffset);
        Write(fileEntry.DataOffset);
        Write(fileEntry.CompressedLength);
        Write(fileEntry.UncompressedSize);
        Write((byte)fileEntry.VerificationType);
        Write((byte)fileEntry.StorageType);
        Write(fileEntry.Crc);
    }

    public void Write(IArchive archive)
    {
        var toc = archive.Tocs[0];
        var rootFolder = toc.RootFolder;
        var fileNodes = toc.Files;
        var folderNodes = toc.Folders;

        MemoryStream contentStream = new();
        ArchiveWriter writer = new(contentStream, Encoding.ASCII);

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
                dirDirectories.Length == 0 ? 0 : ((uint)folderNodes.IndexOf(dirDirectories.Last()) + 1),
                dirFiles.Length == 0 ? 0 : (uint)fileNodes.IndexOf(dirFiles.First()),
                dirFiles.Length == 0 ? 0 : ((uint)fileNodes.IndexOf(dirFiles.Last()) + 1)
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
        Write(new ArchiveHeader(
            Magic, Version, Product, archive.Name,
            HeaderSize, (uint)headerBlobLength,
            HeaderSize + (ulong)dataOffset + HeaderExtraSize, (ulong)dataBlobLength,
            (uint)tocEntryOffset + HeaderExtraSize, 1,
            (uint)folderEntryOffset + HeaderExtraSize, (uint)folderNodes.Count,
            (uint)fileEntryOffset + HeaderExtraSize, (uint)fileNodes.Count,
            (uint)stringsOffset + HeaderExtraSize, (uint)stringLength,
            BlockSize, archive.Signature
        ));

        Write(contentStream.ToArray());
    }
}
