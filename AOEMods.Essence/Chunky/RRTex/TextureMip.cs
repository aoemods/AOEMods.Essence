namespace AOEMods.Essence.Chunky.RRTex;

public struct TextureMip
{
    public int Mip { get; init; }
    public byte[] Data { get; init; }

    public TextureMip(int mip, byte[] data)
    {
        Mip = mip;
        Data = data;
    }
}