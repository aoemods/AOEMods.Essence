namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Describes how a file is stored within an SGA archive.
/// </summary>
public enum FileStorageType : byte
{
    /// <summary>
    /// Stored plainly.
    /// </summary>
    Store,

    /// <summary>
    /// Stored compressed.
    /// </summary>
    StreamCompress,

    /// <summary>
    /// Stored compressed.
    /// </summary>
    BufferCompress,

    /// <summary>
    /// Stored compressed.
    /// </summary>
    StreamCompressBrotli,

    /// <summary>
    /// Stored compressed.
    /// </summary>
    BufferCompressBrotli
}
