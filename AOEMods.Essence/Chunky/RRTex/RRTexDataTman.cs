namespace AOEMods.Essence.Chunky.RRTex;

/// <summary>
/// RRTex DATA TMAN chunk containing information about the data contained in the DATA TDAT chunk.
/// </summary>
/// <param name="Unknown1">Unknown 4 byte value.</param>
/// <param name="Width">Width of the texture.</param>
/// <param name="Height">Height of the texture.</param>
/// <param name="Unknown2">Unknown 4 byte value.</param>
/// <param name="Unknown3">Unknown 4 byte value.</param>
/// <param name="TextureCompression">Texture compression used to compress the texture.</param>
/// <param name="MipCount">Number of mips contained in the DATA TDAT chunk.</param>
/// <param name="Unknown4">Unknown 4 byte value.</param>
/// <param name="MipTextureCount">Texture chunk counts for each mip.</param>
/// <param name="SizeCompressed">Size of the mip texture chunks in bytes as stored in the DATA TDAT chunk.</param>
/// <param name="SizeUncompressed">Size of the mip texture chunks in bytes when inflated.</param>
public record RRTexDataTman(
    int Unknown1, int Width, int Height, int Unknown2, int Unknown3, int TextureCompression,
    int MipCount, int Unknown4, IList<int> MipTextureCount, IList<IList<int>> SizeCompressed, IList<IList<int>> SizeUncompressed
);
