namespace AOEMods.Essence.SGA;

public interface IArchiveNode
{
    IArchiveNode? Parent { get; }
    string Name { get; set; }
    string FullName => Parent != null ? $"{Parent.FullName}\\{Name}" : Name;
    int Depth => Parent != null ? Parent.Depth + 1 : 0;
}
