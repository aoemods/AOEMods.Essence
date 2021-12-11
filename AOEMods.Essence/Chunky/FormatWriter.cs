using AOEMods.Essence.Chunky.RGD;

namespace AOEMods.Essence.Chunky;

public class FormatWriter : IRGDWriter
{
    public static void WriteRGD(Stream stream, IList<RGDNode> nodes)
        => RGDWriter.WriteRGD(stream, nodes);
}
