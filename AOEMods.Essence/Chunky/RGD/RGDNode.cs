namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Key-value node found in DATA AEGD chunks of Relic Game Data (RGD) files.
/// </summary>
public class RGDNode
{
    /// <summary>
    /// String key of the node. The associated id can be found in DATA KEYS chunks.
    /// </summary>
    public string Key { get; init; }

    /// <summary>
    /// Value of the node. Can be one of RGDDataType.
    /// </summary>
    public object Value { get; init; }

    /// <summary>
    /// Initializes a key-value node from the key and value.
    /// </summary>
    /// <param name="key">String key of the node. The associated id can be found in DATA KEYS chunks.</param>
    /// <param name="value">Value of the node. Can be one of RGDDataType.</param>
    public RGDNode(string key, object value)
    {
        Key = key;
        Value = value;
    }
}