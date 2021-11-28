namespace AOEMods.Essence.SGA;

public static class ArchiveNodeHelper
{
    public static IEnumerable<IArchiveNode> EnumerateChildren(IArchiveNode node)
    {
        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                yield return childNode;
            }

            foreach (IArchiveNode childNode in folderNode.Children)
            {
                foreach (var childResult in EnumerateChildren(childNode))
                {
                    yield return childResult;
                }
            }
        }
    }
}