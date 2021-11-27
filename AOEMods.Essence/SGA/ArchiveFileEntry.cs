namespace AOEMods.Essence.SGA;

public record ArchiveFileEntry(
    uint NameOffset, uint HashOffset, ulong DataOffset, uint CompressedLength,
    uint UncompressedSize, FileVerificationType VerificationType, FileStorageType StorageType,
    uint Crc
) : IArchiveEntry;