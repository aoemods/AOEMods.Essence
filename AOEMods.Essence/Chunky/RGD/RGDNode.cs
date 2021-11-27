namespace AOEMods.Essence.Chunky.RGD;

public class RGDNode
{
    public string Key { get; init; }
    public object Value { get; init; }

    public RGDNode(string key, object value)
    {
        Key = key;
        Value = value;
    }
}