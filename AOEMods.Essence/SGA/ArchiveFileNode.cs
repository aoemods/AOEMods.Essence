using System.IO.Compression;
using System.Text;

namespace AOEMods.Essence.SGA;

public class ArchiveFileNode : IArchiveFileNode
{
    public IArchiveNode? Parent { get; }
    public string Name { get; }

    private Stream dataStream;
    private long dataPosition;
    private long dataLength;
    private long dataUncompressedLength;
    private FileStorageType storageType;

    public ArchiveFileNode(Stream dataStream, string name, long dataPosition, long dataLength, long dataUncompressedLength, FileStorageType storageType, IArchiveNode? parent = null)
    {
        Name = name;
        Parent = parent;
        this.dataStream = dataStream;
        this.dataPosition = dataPosition;
        this.dataLength = dataLength;
        this.dataUncompressedLength = dataUncompressedLength;
        this.storageType = storageType;
    }

    public IEnumerable<byte> GetData()
    {
        dataStream.Position = dataPosition;

        switch (storageType)
        {
            case FileStorageType.Store:
                BinaryReader reader = new BinaryReader(dataStream, Encoding.ASCII, true);
                return reader.ReadBytes((int)dataUncompressedLength);
            case FileStorageType.StreamCompress:
            case FileStorageType.BufferCompress:
                {
                    dataStream.Position += 2;
                    using var deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress, leaveOpen: true);
                    MemoryStream decoded = new((int)dataUncompressedLength);
                    deflateStream.CopyTo(decoded);

                    return decoded.ToArray();
                }
            default:
                throw new Exception($"Unknown storage type {storageType}");
        }
    }
}
