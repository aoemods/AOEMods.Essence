namespace AOEMods.Essence.SGA.Graph;

/// <summary>
/// Helper functions for archive nodes.
/// </summary>
public static class ArchiveNodeHelper
{
    /// <summary>
    /// Enumerates all child nodes of an archive node recursively.
    /// </summary>
    /// <param name="node">Node whose children to enumerate recursively.</param>
    /// <returns>Enumerable for all child nodes of the archive node.</returns>
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