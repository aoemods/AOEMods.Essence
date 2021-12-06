namespace AOEMods.Essence.Chunky.RRTex;

public record RRTexDataTman(
    int Unknown1, int Width, int Height, int Unknown2, int Unknown3, int TextureCompression,
    int MipCount, int Unknown4, IList<int> MipTextureCount, IList<IList<int>> SizeCompressed, IList<IList<int>> SizeUncompressed
);
