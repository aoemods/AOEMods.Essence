namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// File node of an SGA archive containing data.
/// </summary>
public interface IArchiveFileNode : IArchiveNode
{
    /// <summary>
    /// Extension of the file node including the period.
    /// </summary>
    public string Extension => Path.GetExtension(Name);

    /// <summary>
    /// Reads and returns the data of the file node.
    /// </summary>
    /// <returns>Data of the file.</returns>
    public IEnumerable<byte> GetData();
}
