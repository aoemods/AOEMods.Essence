using System.IO.Compression;

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

    public static IEnumerable<IArchiveNode> Gather(IArchiveNode node, Predicate<IArchiveNode> pred)
    {
        if (pred(node))
        {
            yield return node;
        }

        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                foreach (var childResult in Gather(childNode, pred))
                {
                    yield return childResult;
                }
            }
        }
    }

    public static IEnumerable<TNode> GatherOfType<TNode>(IArchiveNode node) where TNode : IArchiveNode
    {
        if (node is TNode archiveNode)
        {
            yield return archiveNode;
        }

        if (node is IArchiveFolderNode folderNode)
        {
            foreach (IArchiveNode childNode in folderNode.Children)
            {
                foreach (var childResult in GatherOfType<TNode>(childNode))
                {
                    yield return childResult;
                }
            }
        }
    }
}