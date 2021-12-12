using System.Text;

namespace AOEMods.Essence.Chunky.Core;

/// <summary>
/// Extends BinaryWriter to provide extensions for writing Relic Chunky files.
/// </summary>
public class ChunkyFileWriter : BinaryWriter
{
    private readonly Encoding encoding;

    public ChunkyFileWriter(Stream output) : base(output)
    {
        encoding = Encoding.UTF8;
    }

    public ChunkyFileWriter(Stream output, Encoding encoding) : base(output, encoding)
    {
        this.encoding = encoding;
    }

    public ChunkyFileWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
    {
        this.encoding = encoding;
    }

    /// <summary>
    /// Writes a zero-terminated string to the underlying stream.
    /// </summary>
    /// <param name="s">String to write to the underlying stream.</param>
    public void WriteCString(string s)
    {
        Write(encoding.GetBytes(s));
        Write((byte)0);
    }

    /// <summary>
    /// Writes a chunk header to the underlying stream.
    /// </summary>
    /// <param name="header">Chunk header to write to the underlying stream.</param>
    public void Write(ChunkHeader header)
    {
        Write(encoding.GetBytes(header.Type));
        Write(encoding.GetBytes(header.Name));
        Write(header.Version);
        Write(header.Length);
        Write(header.Path.Length);
        Write(encoding.GetBytes(header.Path));
    }

    /// <summary>
    /// Writes a Relic Chunky file header to the underlying stream.
    /// </summary>
    /// <param name="header">Relic Chunky file header to write to the underlying stream.</param>
    public void Write(ChunkyFileHeader header)
    {
        Write(header.Magic);
        Write(header.Version);
        Write(header.Platform);
    }
}
