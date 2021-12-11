namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Describes how a file is verified when it's loaded.
/// </summary>
public enum FileVerificationType : byte
{
    /// <summary>
    /// No verification.
    /// </summary>
    None,
    CRC,
    CRCBlocks,
    MD5Blocks,
    SHA1Blocks
}
