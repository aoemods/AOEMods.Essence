using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRGeom;
using AOEMods.Essence.Chunky.RRMaterial;
using AOEMods.Essence.Chunky.RRTex;
using SixLabors.ImageSharp.Formats;

namespace AOEMods.Essence.Chunky;

public class FormatReader : IRGDReader, IRRGeomReader, IRRMaterialReader, IRRTexReader
{
    public static IList<RGDNode> ReadRGD(Stream stream)
        => RGDReader.ReadRGD(stream);

    public static IEnumerable<GeometryObject> ReadRRGeom(Stream stream)
        => RRGeomReader.ReadRRGeom(stream);

    public static IEnumerable<Material> ReadRRMaterial(Stream stream, string? materialName = null)
        => RRMaterialReader.ReadRRMaterial(stream, materialName);

    public static IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        => RRTexReader.ReadRRTex(rrtexStream, outputFormat, textureType);

    public static TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        => RRTexReader.ReadRRTexLastMip(rrtexStream, outputFormat, textureType);
}
