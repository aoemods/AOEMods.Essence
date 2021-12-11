using System.Text;

namespace AOEMods.Essence.SGA.Core;

public class ArchiveReader : BinaryReader
{
    public ArchiveReader(Stream input) : base(input)
    {
    }

    public ArchiveReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public ArchiveReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }

    public string ReadCString()
    {
        List<char> chars = new List<char>();
        while (true)
        {
            char c = ReadChar();
            if (c != 0)
            {
                chars.Add(c);
            }
            else
            {
                break;
            }
        }

        return new string(chars.ToArray());
    }

    private string ReadFixedString(int charCount, int charSize)
    {
        byte[] array = ReadBytes(charCount * charSize);
        if (array == null || array.Length != charCount * charSize)
        {
            throw new ApplicationException($"File length is not sufficient for fixed string of length {charCount * charSize}.");
        }
        for (int i = 0; i < charCount; i++)
        {
            bool flag = true;
            for (int j = 0; j < charSize; j++)
            {
                if (array[i * charSize + j] != 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                charCount = i;
            }
        }
        return charSize switch
        {
            1 => Encoding.UTF8.GetString(array, 0, charCount * charSize),
            2 => Encoding.Unicode.GetString(array, 0, charCount * charSize),
            _ => throw new ArgumentOutOfRangeException("charSize"),
        };
    }

    public ArchiveHeader ReadHeader()
    {
        byte[] magic = ReadBytes(8);
        ushort version = ReadUInt16();
        ushort product = ReadUInt16();
        string niceName = ReadFixedString(64, 2);

        ulong blobOffset = ReadUInt64(); // header blob offset
        uint blobLength = ReadUInt32(); // head blob length
        ulong dataOffset = ReadUInt64(); // data blob offset
        ulong dataBlobLength = ReadUInt64(); // data blob length

        uint unknown2 = ReadUInt32(); // always 1?

        byte[] signature = ReadBytes(256);

        BaseStream.Seek((long)blobOffset, SeekOrigin.Begin);

        uint tocDataOffset = ReadUInt32();
        uint tocDataCount = ReadUInt32();
        uint folderDataOffset = ReadUInt32();
        uint folderDataCount = ReadUInt32();
        uint fileDataOffset = ReadUInt32();
        uint fileDataCount = ReadUInt32();
        uint stringOffset = ReadUInt32();
        uint stringLength = ReadUInt32();

        uint fileHashOffset = ReadUInt32();
        uint fileHashLength = ReadUInt32();

        uint blockSize = ReadUInt32();

        return new ArchiveHeader(
            magic, version, product, niceName,
            blobOffset, blobLength,
            dataOffset, dataBlobLength,
            tocDataOffset, tocDataCount,
            folderDataOffset, folderDataCount,
            fileDataOffset, fileDataCount,
            stringOffset, stringLength,
            blockSize, signature
        );
    }

    public ArchiveTocEntry ReadTocEntry()
    {
        string alias = ReadFixedString(64, 1);
        string name = ReadFixedString(64, 1);
        uint folderStartIndex = ReadUInt32();
        uint folderEndIndex = ReadUInt32();
        uint fileStartIndex = ReadUInt32();
        uint fileEndIndex = ReadUInt32();
        uint folderRootIndex = ReadUInt32();

        return new ArchiveTocEntry(alias, name, folderStartIndex, folderEndIndex, fileStartIndex, fileEndIndex, folderRootIndex);
    }

    public ArchiveFolderEntry ReadFolderEntry()
    {
        uint nameOffset = ReadUInt32();
        uint folderStartIndex = ReadUInt32();
        uint folderEndIndex = ReadUInt32();
        uint fileStartIndex = ReadUInt32();
        uint fileEndIndex = ReadUInt32();

        return new ArchiveFolderEntry(nameOffset, folderStartIndex, folderEndIndex, fileStartIndex, fileEndIndex);
    }

    public ArchiveFileEntry ReadFileEntry()
    {
        uint nameOffset = ReadUInt32();
        uint hashOffset = ReadUInt32();
        ulong dataOffset = ReadUInt64();
        uint compressedLength = ReadUInt32();
        uint uncompressedLength = ReadUInt32();
        byte verificationType = ReadByte();
        byte storageType = ReadByte();
        uint crc = ReadUInt32();

        return new ArchiveFileEntry(
            nameOffset, hashOffset, dataOffset, compressedLength,
            uncompressedLength, (FileVerificationType)verificationType,
            (FileStorageType)storageType, crc
        );
    }

    public ArchiveEntries ReadArchiveEntries()
    {
        var header = ReadHeader();

        var tocs = new ArchiveTocEntry[header.TocDataCount];
        var folders = new ArchiveFolderEntry[header.FolderDataCount];
        var files = new ArchiveFileEntry[header.FileDataCount];

        BaseStream.Seek((long)(header.Offset + header.TocDataOffset), SeekOrigin.Begin);
        for (int i = 0; i < header.TocDataCount; i++)
        {
            tocs[i] = ReadTocEntry();
        }

        BaseStream.Seek((long)(header.Offset + header.FolderDataOffset), SeekOrigin.Begin);
        for (int i = 0; i < header.FolderDataCount; i++)
        {
            folders[i] = ReadFolderEntry();
        }

        BaseStream.Seek((long)(header.Offset + header.FileDataOffset), SeekOrigin.Begin);
        for (int i = 0; i < header.FileDataCount; i++)
        {
            files[i] = ReadFileEntry();
        }

        return new ArchiveEntries(header, tocs, folders, files);
    }
}
