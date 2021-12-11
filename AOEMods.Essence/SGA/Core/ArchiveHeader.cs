namespace AOEMods.Essence.SGA.Core;

/// <summary>
/// Header of an SGA archive.
/// </summary>
/// <param name="Magic">Magic value of an SGA archive. Should be "_ARCHIVE".</param>
/// <param name="Version">Archive version.</param>
/// <param name="Product">Product id.</param>
/// <param name="NiceName">Name of the archive</param>
/// <param name="HeaderBlobOffset">Offset where the archive's header blob starts.</param>
/// <param name="HeaderBlobLength">Size of the archive's header blob in bytes.</param>
/// <param name="DataOffset">Offset where the archive's data blob starts.</param>
/// <param name="DataBlobLength">Size of the archive's data blob in bytes.</param>
/// <param name="TocDataOffset">Offset relative to HeaderBlobOffset where the archive's table of contents data starts.</param>
/// <param name="TocDataCount">Number of tocs at the TocDataOffset.</param>
/// <param name="FolderDataOffset">Offset relative to HeaderBlobOffset where the archive's folder data starts.</param>
/// <param name="FolderDataCount">Number of folders at FolderDataOffset.</param>
/// <param name="FileDataOffset">Offset relative to HeaderBlobOffset where the archive's file data starts.</param>
/// <param name="FileDataCount">Number of files at FileDataOffset.</param>
/// <param name="StringOffset">Offset relative to HeaderBlobOffset where the archive's string data starts.</param>
/// <param name="StringLength">Size of the archive's string data in bytes.</param>
/// <param name="BlockSize">Block size of the archive.</param>
/// <param name="Signature">2048 bit (256 byte) signature of the archive. Probably using PKCS#1 in official archives.
/// Also validated in the game by XORing together 16 byte chunks and comparing against known values.</param>
public record ArchiveHeader(
    byte[] Magic,
    ushort Version, ushort Product, string NiceName,
    ulong HeaderBlobOffset, uint HeaderBlobLength,
    ulong DataOffset, ulong DataBlobLength,
    uint TocDataOffset, uint TocDataCount,
    uint FolderDataOffset, uint FolderDataCount,
    uint FileDataOffset, uint FileDataCount,
    uint StringOffset, uint StringLength,
    uint BlockSize, byte[] Signature
);
