namespace AOEMods.Essence.Chunky.RGD;

public interface IRGDWriter
{
    static abstract void WriteRGD(Stream stream, IList<RGDNode> nodes);
}
