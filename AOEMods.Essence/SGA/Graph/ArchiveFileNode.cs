using AOEMods.Essence.SGA.Core;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// File node of an SGA archive containing data read from a stream as stored in an SGA file.
/// </summary>
public class ArchiveFileNode : IArchiveFileNode
{
    /// <summary>
    /// Parent of the node.
    /// </summary>
    public IArchiveNode? Parent { get; }

    /// <summary>
    /// Name of the node.
    /// </summary>
    public string Name { get; set; }

    private readonly Stream dataStream;
    private readonly long dataPosition;
    private readonly long dataLength;
    private readonly long dataUncompressedLength;
    private readonly FileStorageType storageType;

    /// <summary>
    /// Initializes an ArchiveFileNode from values as usually stored in an SGA file.
    /// </summary>
    /// <param name="dataStream">Stream to read the node's data from.</param>
    /// <param name="name">Name of the node.</param>
    /// <param name="dataPosition">Position of the data in the stream.</param>
    /// <param name="dataLength">Size of the data in bytes in the stream.</param>
    /// <param name="dataUncompressedLength">Size of the data when uncompressed.</param>
    /// <param name="storageType">How the data is stored in the stream.</param>
    /// <param name="parent">Parent of the node.</param>
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

    /// <summary>
    /// Reads and returns the data of the file node from the stream.
    /// </summary>
    /// <returns>Data of the file.</returns>
    public IEnumerable<byte> GetData()
    {
        dataStream.Position = dataPosition;

        switch (storageType)
        {
            case FileStorageType.Store:
                BinaryReader reader = new BinaryReader(dataStream, Encoding.UTF8, true);
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
            case FileStorageType.StreamCompressBrotli:
            case FileStorageType.BufferCompressBrotli:
                {
                    using var brotliStream = new BrotliStream(dataStream, CompressionMode.Decompress, leaveOpen: true);
                    MemoryStream decoded = new((int)dataUncompressedLength);
                    brotliStream.CopyTo(decoded);

                    return decoded.ToArray();
                }
            default:
                throw new NotImplementedException($"Unknown storage type {storageType}");
        }
    }
}
