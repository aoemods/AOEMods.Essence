namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Data type of a DATA AEGD chunk entry.
/// </summary>
public enum RGDDataType : int
{
    /// <summary>
    /// 32 bit float.
    /// </summary>
    Float = 0,
    /// <summary>
    /// 32 bit integer.
    /// </summary>
    Int = 1,
    /// <summary>
    /// 32 bit boolean.
    /// </summary>
    Boolean = 2,
    /// <summary>
    /// Zero-terminated string.
    /// </summary>
    CString = 3,
    /// <summary>
    /// List of more data.
    /// </summary>
    List = 100,
    /// <summary>
    /// List of more data.
    /// </summary>
    List2 = 101,
}
