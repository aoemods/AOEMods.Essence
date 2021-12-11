namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// List containing key value entries found in the DATA AEGD format of Relic Game Data (RGD) chunks.
/// </summary>
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