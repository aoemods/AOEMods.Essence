namespace AOEMods.Essence.SGA;

public static class ArchiveNodeHelper
{
    public static void TraverseBreadthFirst(IArchiveNode node, Action<IArchiveNode> visit)
    {
        visit(node);

        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                TraverseBreadthFirst(childNode, visit);
            }
        }
    }

    public static IEnumerable<TResult> TraverseBreadthFirstWithResult<TResult>(IArchiveNode node, Func<IArchiveNode, TResult> visit)
    {
        yield return visit(node);

        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                foreach (var childResult in TraverseBreadthFirstWithResult(childNode, visit))
                {
                    yield return childResult;
                }
            }
        }
    }

    public static IEnumerable<IArchiveNode> EnumerateNodes(IArchiveNode node)
    {
        yield return node;

        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                foreach (var childResult in EnumerateNodes(childNode))
                {
                    yield return childResult;
                }
            }
        }
    }
}