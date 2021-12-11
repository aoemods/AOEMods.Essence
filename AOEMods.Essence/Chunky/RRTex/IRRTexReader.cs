using SixLabors.ImageSharp.Formats;

namespace AOEMods.Essence.Chunky.RRTex;

/// <summary>
/// Reads Relic Texture (RRTex) files.
/// </summary>
public interface IRRTexReader
{
    /// <summary>
    /// Reads TextureMips from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMips.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMips read from the stream.</returns>
    static abstract IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic);

    /// <summary>
    /// Reads the last (usually largest) TextureMip from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMip.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMip read from the stream.</returns>
    static abstract TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic);
}
