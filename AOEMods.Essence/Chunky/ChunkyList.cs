namespace AOEMods.Essence.Chunky;
public class ChunkyList : List<KeyValueEntry>
{
    public ChunkyList()
    {
    }

    public ChunkyList(IEnumerable<KeyValueEntry> collection) : base(collection)
    {
    }

    public ChunkyList(int capacity) : base(capacity)
    {
    }
}