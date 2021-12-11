using System.Text;

namespace AOEMods.Essence.Chunky.Core;

/// <summary>
/// Extends BinaryReader to provide functions for reading Relic Chunky files.
/// </summary>
public class ChunkyFileReader : BinaryReader
{
    private Encoding encoding;

    public ChunkyFileReader(Stream input) : base(input)
    {
        encoding = Encoding.UTF8;
    }

    public ChunkyFileReader(Stream input, Encoding encoding) : base(input, encoding)
    {
        this.encoding = encoding;
    }

    public ChunkyFileReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
        this.encoding = encoding;
    }

    /// <summary>
    /// Reads a zero-terminated string from the stream and advances
    /// the stream's position.
    /// </summary>
    /// <returns>String that was read.</returns>
    public string ReadCString()
    {
        List<byte> chars = new();
        while (true)
        {
            byte b = ReadByte();
            if (b != 0)
            {
                chars.Add(b);
            }
            else
            {
                break;
            }
        }

        return encoding.GetString(chars.ToArray());
    }

    private IEnumerable<ChunkHeader> ReadChunkHeadersImpl(long? length = null)
    {
        long position = BaseStream.Position;
        length ??= BaseStream.Length - position;
        while (BaseStream.Position < position + length)
        {
            long oldPosition = BaseStream.Position;
            var chunkHeader = ReadChunkHeader();

            if (chunkHeader.Type != "DATA" && chunkHeader.Type != "FOLD")
            {
                BaseStream.Position = oldPosition;
                break;
            }

            yield return chunkHeader;

            BaseStream.Position = chunkHeader.DataPosition + chunkHeader.Length;
        }
    }

    /// <summary>
    /// Reads all chunk headers from the current stream position non-recursively and advances
    /// the stream's position.
    /// </summary>
    /// <param name="length">Optional length of how many bytes to read. If null, the entire rest of the stream is considered.</param>
    /// <returns>Chunk headers that were read from the stream.</returns>
    public IEnumerable<ChunkHeader> ReadChunkHeaders(long? length = null) => StreamEnumerableUtil.WithStreamPosition(BaseStream, ReadChunkHeadersImpl(length));

    /// <summary>
    /// Reads a single chunk header from the current stream position and advances
    /// the stream's position.
    /// </summary>
    /// <returns>Chunk header that was read from the stream.</returns>
    public ChunkHeader ReadChunkHeader()
    {
        return new ChunkHeader(
            encoding.GetString(ReadBytes(4)),
            encoding.GetString(ReadBytes(4)),
            ReadInt32(),
            ReadInt32(),
            encoding.GetString(ReadBytes(ReadInt32())).Replace("\0", ""),
            BaseStream.Position
        );
    }

    /// <summary>
    /// Reads the Relic Chunky file header at the current  stream position
    /// and advances the stream's position.
    /// </summary>
    /// <returns>Relic Chunky file header that was read.</returns>
    public ChunkyFileHeader ReadChunkyFileHeader()
    {
        return new ChunkyFileHeader(
            ReadBytes(16),
            ReadInt32(),
            ReadInt32()
        );
    }
}