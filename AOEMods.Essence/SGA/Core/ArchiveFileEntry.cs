namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// File entry of an SGA archive.
/// </summary>
/// <param name="NameOffset">Offset of the file's name in the SGA archive's string blob.</param>
/// <param name="HashOffset">Offset of the file's hash in the SGA archive's hash blob.</param>
/// <param name="DataOffset">Offset of the file's data in the SGA archive's data blob.</param>
/// <param name="CompressedLength">Size of the file in bytes as stored in the SGA archive.</param>
/// <param name="UncompressedSize">Size of the file in bytes when uncompressed.</param>
/// <param name="VerificationType">How the file should be verified when loaded.</param>
/// <param name="StorageType">How the file's data is stored.</param>
/// <param name="Crc">Crc32 checksum of the file's data.</param>
public record ArchiveFileEntry(
    uint NameOffset, uint HashOffset, ulong DataOffset, uint CompressedLength,
    uint UncompressedSize, FileVerificationType VerificationType, FileStorageType StorageType,
    uint Crc
);