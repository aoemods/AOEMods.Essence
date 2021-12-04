namespace AOEMods.Essence.Chunky;

public interface IChunkyDataNode : IChunkyNode
{
    IEnumerable<byte> GetData();
}
