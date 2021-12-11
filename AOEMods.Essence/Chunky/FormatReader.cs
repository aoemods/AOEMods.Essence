using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRGeom;
using AOEMods.Essence.Chunky.RRMaterial;
using AOEMods.Essence.Chunky.RRTex;
using SixLabors.ImageSharp.Formats;

namespace AOEMods.Essence.Chunky;

/// <summary>
/// Provides functions to read different Relic Chunky formats.
/// </summary>
public class FormatReader : IRGDReader, IRRGeomReader, IRRMaterialReader, IRRTexReader
{
    /// <summary>
    /// Reads a list of RGD nodes from a stream containing an RGD file.
    /// </summary>
    /// <param name="stream">Stream containing an RGD file.</param>
    /// <returns>RGD nodes read from the stream.</returns>
    /// <exception cref="Exception">Thrown if zero or more than DATA KEYS or AEGD chunks are present.</exception>
    public static IList<RGDNode> ReadRGD(Stream stream)
        => RGDReader.ReadRGD(stream);

    /// <summary>
    /// Reads GeometryObjects from a stream containing an RRGeom file.
    /// </summary>
    /// <param name="stream">Stream containing an RRGeom file.</param>
    /// <returns>GeometryObjects read from the stream.</returns>
    public static IEnumerable<GeometryObject> ReadRRGeom(Stream stream)
        => RRGeomReader.ReadRRGeom(stream);

    /// <summary>
    /// Reads Materials from a stream containing an RRMaterial file.
    /// </summary>
    /// <param name="stream">Stream containing an RRMaterial file.</param>
    /// <returns>Materials read from the stream.</returns>
    public static IEnumerable<Material> ReadRRMaterial(Stream stream, string? materialName = null)
        => RRMaterialReader.ReadRRMaterial(stream, materialName);

    /// <summary>
    /// Reads TextureMips from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMips.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMips read from the stream.</returns>
    public static IEnumerable<TextureMip> ReadRRTex(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        => RRTexReader.ReadRRTex(rrtexStream, outputFormat, textureType);

    /// <summary>
    /// Reads the last (usually largest) TextureMip from a stream containing an RRTex file.
    /// </summary>
    /// <param name="rrtexStream">Stream containing an RRTex file.</param>
    /// <param name="outputFormat">Image format of the output TextureMip.</param>
    /// <param name="textureType">Type of the texture to read.</param>
    /// <returns>TextureMip read from the stream.</returns>
    public static TextureMip? ReadRRTexLastMip(Stream rrtexStream, IImageFormat outputFormat, RRTexType textureType = RRTexType.Generic)
        => RRTexReader.ReadRRTexLastMip(rrtexStream, outputFormat, textureType);
}
