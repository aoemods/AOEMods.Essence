using AOEMods.Essence.SGA.Core;
using System.IO;
using System.Text;
using Xunit;

namespace AOEMods.Essence.Test;

public class ArchiveReaderTest
{
    private Stream OpenTestArchive()
    {
        return File.OpenRead(Path.Combine("TestFiles", "TestArchive.sga"));
    }

    [Fact]
    public void TestReadArchiveEntries_ArchiveWithHelloWorldTextFile_CorrectFile()
    {
        using var testArchiveStream = OpenTestArchive();

        ArchiveReader reader = new(testArchiveStream);
        var archiveEntries = reader.ReadArchiveEntries();

        Assert.Equal(1, archiveEntries.Files.Count);

        Assert.Equal(11u, archiveEntries.Files[0].CompressedLength);
        Assert.Equal(11u, archiveEntries.Files[0].UncompressedSize);
        Assert.Equal(FileStorageType.Store, archiveEntries.Files[0].StorageType);
        Assert.Equal(FileVerificationType.None, archiveEntries.Files[0].VerificationType);

        // File name is "hello.txt"
        reader.BaseStream.Position = (long)(
            archiveEntries.Header.HeaderBlobOffset +
            archiveEntries.Header.StringOffset +
            archiveEntries.Files[0].NameOffset
        );
        var fileName = reader.ReadCString();
        Assert.Equal("hello.txt", fileName);

        // File content is "hello world"
        reader.BaseStream.Position = (long)(archiveEntries.Header.DataOffset + archiveEntries.Files[0].DataOffset);
        var fileContent = reader.ReadBytes((int)archiveEntries.Files[0].CompressedLength);
        string fileContentString = Encoding.UTF8.GetString(fileContent);
        Assert.Equal("hello world", fileContentString);
    }

    [Fact]
    public void TestReadArchiveEntries_ArchiveWithHelloWorldTextFile_CorrectNumberOfTocsFilesFolders()
    {
        using var testArchiveStream = OpenTestArchive();

        ArchiveReader reader = new(testArchiveStream);
        var archiveEntries = reader.ReadArchiveEntries();

        Assert.Equal(1, archiveEntries.Tocs.Count);
        Assert.Equal(1, archiveEntries.Files.Count);
        Assert.Equal(1, archiveEntries.Folders.Count);
    }

    [Fact]
    public void TestReadArchiveEntries_ArchiveWithHelloWorldTextFile_CorrectRootFolder()
    {
        using var testArchiveStream = OpenTestArchive();

        ArchiveReader reader = new(testArchiveStream);
        var archiveEntries = reader.ReadArchiveEntries();

        // One folder, the root folder, in archive
        Assert.Equal(1, archiveEntries.Folders.Count);

        // Folder name is empty string
        reader.BaseStream.Position = (long)(
            archiveEntries.Header.HeaderBlobOffset +
            archiveEntries.Header.StringOffset +
            archiveEntries.Folders[0].NameOffset
        );
        string rootFolderName = reader.ReadCString();
        Assert.Equal("", rootFolderName);

        // One file in folder at index 0
        Assert.Equal(0u, archiveEntries.Folders[0].FileStartIndex);
        Assert.Equal(1u, archiveEntries.Folders[0].FileEndIndex);

        // No folders in folder
        Assert.Equal(0u, archiveEntries.Folders[0].FolderStartIndex);
        Assert.Equal(0u, archiveEntries.Folders[0].FolderEndIndex);
    }
}
