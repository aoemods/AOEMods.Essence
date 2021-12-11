namespace AOEMods.Essence.Chunky.Graph;

public interface IChunkyDataNode : IChunkyNode
{
    IEnumerable<byte> GetData();
}
