namespace AOEMods.Essence.Chunky.RRTex;

/// <summary>
/// Texture mip containing texture data for a single mip.
/// </summary>
public struct TextureMip
{
    /// <summary>
    /// Mip number.
    /// </summary>
    public int Mip { get; init; }

    /// <summary>
    /// Texture data of the mip.
    /// </summary>
    public byte[] Data { get; init; }

    /// <summary>
    /// Initializes a TextureMip from a mip number and texture data.
    /// </summary>
    /// <param name="mip">Mip number.</param>
    /// <param name="data">Texture data of the mip.</param>
    public TextureMip(int mip, byte[] data)
    {
        Mip = mip;
        Data = data;
    }
}