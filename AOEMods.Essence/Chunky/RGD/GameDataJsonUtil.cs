using System.Text;

namespace AOEMods.Essence.Chunky.RGD;

public static class GameDataJsonUtil
{
    public static string GameDataToJson(IList<RGDNode> nodes)
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
            stringBuilder.Append("{\n");
            printIndent(depth + 1);
            stringBuilder.AppendFormat("\"key\": \"{0}\",\n", node.Key);
            printIndent(depth + 1);
            stringBuilder.Append("\"value\": ");

            if (node.Value is IList<RGDNode> childNodes)
            {
                stringBuilder.Append("[\n");

                for (int i = 0; i < childNodes.Count; i++)
                {
                    printValue(childNodes[i], depth + 2);
                    var childNode = childNodes[i];

                    if (i != childNodes.Count - 1)
                    {
                        stringBuilder.Append(",");
                    }

                    stringBuilder.Append("\n");
                }

                printIndent(depth + 1);
                stringBuilder.Append("]");
                stringBuilder.Append("\n");
            }
            else
            {
                stringBuilder.Append(node.Value switch
                {
                    bool b => b ? "true" : "false",
                    string s => $"\"{s.Replace("\\", "\\\\")}\"",
                    _ => node.Value,
                });

                stringBuilder.Append("\n");
            }

            printIndent(depth);
            stringBuilder.Append("}");
        }

        stringBuilder.Append("{\n");
        printIndent(1);
        stringBuilder.Append("\"data\": [\n");
        for (int i = 0; i < nodes.Count; i++)
        {
            printValue(nodes[i], 2);
            if (i != nodes.Count - 1)
            {
                stringBuilder.Append(",");
            }
            stringBuilder.Append("\n");
        }
        printIndent(1);
        stringBuilder.Append("]\n}");

        return stringBuilder.ToString();
    }
}
