namespace AOEMods.Essence.SGA.Graph
{
    public interface IArchiveFileNode : IArchiveNode
    {
        public string Extension => Path.GetExtension(Name);
        public IEnumerable<byte> GetData();
    }
}
