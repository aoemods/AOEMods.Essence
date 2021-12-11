namespace AOEMods.Essence.SGA.Core;

public record ArchiveFileEntry(
    uint NameOffset, uint HashOffset, ulong DataOffset, uint CompressedLength,
    uint UncompressedSize, FileVerificationType VerificationType, FileStorageType StorageType,
    uint Crc
);