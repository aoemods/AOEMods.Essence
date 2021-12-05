using System.Security;
using System.Text;

namespace AOEMods.Essence.Chunky.RGD;

public static class GameDataXmlUtil
{
    public static string GameDataToXml(IList<RGDNode> nodes)
    {
        StringBuilder stringBuilder = new StringBuilder();

        void printIndent(int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                stringBuilder.Append("  ");
            }
        }

        void printValue(RGDNode node, int depth)
        {
            printIndent(depth);
            stringBuilder.AppendFormat("<RGDNode Key=\"{0}\" Type=\"{1}\"", SecurityElement.Escape(node.Key), SecurityElement.Escape(node.Value switch
            {
                float => RGDDataType.Float.ToString(),
                int => RGDDataType.Int.ToString(),
                bool => RGDDataType.Boolean.ToString(),
                string => RGDDataType.CString.ToString(),
                RGDNode[] => RGDDataType.List.ToString(),
                _ => throw new NotSupportedException()
            }));
            
            if (node.Value is IList<RGDNode> childNodes)
            {
                stringBuilder.Append(">\n");
                for (int i = 0; i < childNodes.Count; i++)
                {
                    printValue(childNodes[i], depth + 1);
                    stringBuilder.Append("\n");
                }

                printIndent(depth);
                stringBuilder.Append("</RGDNode>");
            }
            else
            {
                stringBuilder.AppendFormat(" Value=\"{0}\" />", node.Value is string stringValue ? SecurityElement.Escape(stringValue) : node.Value);
            }
        }

        stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        stringBuilder.Append("<Root>\n");

        for (int i = 0; i < nodes.Count; i++)
        {
            printValue(nodes[i], 1);
            stringBuilder.Append("\n");
        }
        stringBuilder.Append("</Root>");

        return stringBuilder.ToString();
    }
}
