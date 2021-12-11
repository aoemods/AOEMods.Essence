using System.Text;

namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Extends BinaryWriter to provide functions for writing SGA archive files.
/// </summary>
public class ArchiveWriter : BinaryWriter
{
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

    /// <summary>
    /// Adds a string to the archive's string blob. Use WriteStrings to write the
    /// string blob to the stream.
    /// </summary>
    /// <param name="s">String to add to the archive's string blob.</param>
    /// <returns>Offset within the string blob where the added string starts.</returns>
    public long AddString(string s)
    {
        long stringOffset = archiveStrings.Length;
        archiveStrings.Write(Encoding.ASCII.GetBytes(s));
        archiveStrings.WriteByte(0);
        return stringOffset;
    }

    /// <summary>
    /// Adds a string to the archive's data blob. Use WriteData to write the
    /// data blob to the stream.
    /// </summary>
    /// <param name="data">Data to add to the archive's data blob.</param>
    /// <returns>Offset within the data blob where the added data starts.</returns>
    public long AddData(byte[] data)
    {
        long offset = archiveData.Length;
        archiveData.Write(data);
        return offset;
    }

    /// <summary>
    /// Writes the string blob previously created using AddString
    /// to the stream and advances the stream's position.
    /// </summary>
    public void WriteStrings()
    {
        Write(archiveStrings.ToArray());
    }

    /// <summary>
    /// Writes the data blob previously created using AddData
    /// to the stream and advances the stream's position.
    /// </summary>
    public void WriteData()
    {
        Write(archiveData.ToArray());
    }

    private void WriteFixedString(string s, int charCount, int charSize)
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

    /// <summary>
    /// Writes an archive header to the stream and advances the stream's position.
    /// </summary>
    /// <param name="header">Archive header to write to the stream.</param>
    /// <exception cref="Exception">Thrown if the header length or offset is inconsistent.</exception>
    public void Write(ArchiveHeader header)
    {
        Write(header.Magic); // 0
        Write(header.Version); // 8
        Write(header.Product); // 10
        WriteFixedString(header.NiceName, 64, 2); // 12
        Write(header.HeaderBlobOffset); // 140 header blob offset
        Write(header.HeaderBlobLength); // 148 num2 / header blob length
        Write(header.DataOffset); // 152 data blob offset
        Write(header.DataBlobLength); // 160 data blob length
        Write(1U); // 168 unk2 == 1

        Write(header.Signature);
        if (header.HeaderBlobOffset != (ulong)BaseStream.Position)
        {
            throw new Exception("Header length or offset incorrect.");
        }

        Write(new byte[header.HeaderBlobOffset - (ulong)BaseStream.Position]);

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

    /// <summary>
    /// Writes an archive table of contents entry to the stream and advances the stream's position.
    /// </summary>
    /// <param name="tocEntry">Archive table of contents entry to write to the stream.</param>
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

    /// <summary>
    /// Writes an archive folder entry to the stream and advances the stream's position.
    /// </summary>
    /// <param name="folderEntry">Archive folder entry to write to the stream.</param>
    public void Write(ArchiveFolderEntry folderEntry)
    {
        Write(folderEntry.NameOffset);
        Write(folderEntry.FolderStartIndex);
        Write(folderEntry.FolderEndIndex);
        Write(folderEntry.FileStartIndex);
        Write(folderEntry.FileEndIndex);
    }

    /// <summary>
    /// Writes an archive file entry to the stream and advances the stream's position.
    /// </summary>
    /// <param name="fileEntry">Archive file entry to write to the stream.</param>
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
}
