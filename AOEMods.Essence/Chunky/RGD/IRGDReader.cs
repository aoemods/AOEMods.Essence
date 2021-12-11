namespace AOEMods.Essence.Chunky.RGD;

public interface IRGDReader
{
    static abstract IList<RGDNode> ReadRGD(Stream stream);
}
