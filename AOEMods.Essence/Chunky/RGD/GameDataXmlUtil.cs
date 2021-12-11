using System.Security;
using System.Text;
using System.Xml.Linq;

namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Provides functions for encoding and decoding Relic Game Data (RGD) with XML.
/// </summary>
public static class GameDataXmlUtil
{
    /// <summary>
    /// Encodes RGD nodes as an XML string.
    /// </summary>
    /// <param name="nodes">List of RGD nodes to encode with XML.</param>
    /// <returns>XML string encoding the given RGD nodes.</returns>
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

    /// <summary>
    /// Decodes an XML string to RGD nodes.
    /// </summary>
    /// <param name="xml">XML string encoding the RGD nodes.</param>
    /// <returns>RGD nodes the XML string is encoding.</returns>
    /// <exception cref="NotImplementedException">Thrown if an unknown data type is read for a node in the encoded XML.</exception>
    public static RGDNode[] XmlToGameData(string xml)
    {
        var doc = XDocument.Parse(xml);

        RGDNode XmlElementToRgdNode(XElement element)
        {
            RGDDataType type = Enum.Parse<RGDDataType>(element.Attribute("Type").Value);
            object value = type switch
            {
                RGDDataType.Float => float.Parse(element.Attribute("Value").Value),
                RGDDataType.Int => int.Parse(element.Attribute("Value").Value),
                RGDDataType.Boolean => bool.Parse(element.Attribute("Value").Value),
                RGDDataType.CString => element.Attribute("Value").Value,
                RGDDataType.List or RGDDataType.List2 => element.Elements().Select(XmlElementToRgdNode).ToArray(),
                _ => throw new NotImplementedException()
            };
            return new RGDNode(element.Attribute("Key").Value, value);
        }

        var rootElements = doc.Root.Elements();

        return rootElements.Select(XmlElementToRgdNode).ToArray();
    }
}
