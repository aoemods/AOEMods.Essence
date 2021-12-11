using SixLabors.ImageSharp.Formats;

namespace AOEMods.Essence.Chunky.RRTex;

public interface IRRTexReader
{
    static abstract IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic);
    static abstract TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic);
}
