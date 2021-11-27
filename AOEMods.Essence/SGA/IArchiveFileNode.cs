namespace AOEMods.Essence.SGA
{
    public interface IArchiveFileNode : IArchiveNode
    {
        public string Extension => Path.GetExtension(Name);
        public IEnumerable<byte> GetData();
    }
}
