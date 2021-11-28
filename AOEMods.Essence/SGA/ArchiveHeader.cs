namespace AOEMods.Essence.SGA;

public record ArchiveHeader(
    byte[] Magic,
    ushort Version, ushort Product, string NiceName,
    ulong Offset, uint HeaderBlobLength,
    ulong DataOffset, ulong DataBlobLength,
    uint TocDataOffset, uint TocDataCount,
    uint FolderDataOffset, uint FolderDataCount,
    uint FileDataOffset, uint FileDataCount,
    uint StringOffset, uint StringLength,
    uint BlockSize, byte[] Signature
);
