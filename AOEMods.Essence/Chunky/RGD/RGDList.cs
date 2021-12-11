namespace AOEMods.Essence.Chunky;
public class RGDList : List<KeyValueEntry>
{
    public RGDList()
    {
    }

    public RGDList(IEnumerable<KeyValueEntry> collection) : base(collection)
    {
    }

    public RGDList(int capacity) : base(capacity)
    {
    }
}