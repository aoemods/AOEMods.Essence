using AOEMods.Essence.Chunky.Core;
using System.Text;

namespace AOEMods.Essence.Chunky.RGD;

/// <summary>
/// Writes Relic Game Data (RGD) files.
/// </summary>
public class RGDWriter : IRGDWriter
{
    /// <summary>
    /// Writes an RGD file to a stream given a list of RGD root nodes.
    /// </summary>
    /// <param name="stream">Stream to write the RGD file to.</param>
    /// <param name="nodes">RGD root nodes from which to write the RGD file.</param>
    public static void WriteRGD(Stream stream, IList<RGDNode> nodes)
    {
        ChunkyFileWriter writer = new(stream);

        // Write chunky file header
        writer.Write(new ChunkyFileHeader(ChunkyConstants.Magic, 4, 1));

        Dictionary<string, ulong> hashes = new();

        void AddNodeHash(RGDNode node)
        {
            ulong keyHash = (ulong)((long)node.Key.GetHashCode() + int.MaxValue);
            hashes[node.Key] = keyHash;

            if (node.Value is RGDNode[] childNodes)
            {
                foreach (var childNode in childNodes)
                {
                    AddNodeHash(childNode);
                }
            }
        }

        foreach (var node in nodes)
        {
            AddNodeHash(node);
        }

        // Write AEGD
        ChunkHeader header = new("DATA", "AEGD", 3, 0, "", 0);
        var headerPosition = stream.Position;
        writer.Write(header);
        var dataPosition = stream.Position;
        WriteKeyValueDataChunk(writer, new KeyValueDataChunk(nodes.Select(node => new KeyValueEntry(hashes[node.Key], node.Value)).ToArray()));
        var postDataPosition = stream.Position;
        stream.Position = headerPosition;
        header = new("DATA", "AEGD", 3, (int)(postDataPosition - dataPosition), "", dataPosition);
        writer.Write(header);
        stream.Position = postDataPosition;

        // Write KEYS
        header = new("DATA", "KEYS", 3, 0, "", 0);
        headerPosition = stream.Position;
        writer.Write(header);
        dataPosition = stream.Position;
        WriteKeysDataChunk(writer, new KeysDataChunk(hashes));
        postDataPosition = stream.Position;
        stream.Position = headerPosition;
        header = new("DATA", "KEYS", 3, (int)(postDataPosition - dataPosition), "", dataPosition);
        writer.Write(header);
        stream.Position = postDataPosition;
    }

    private static void WriteChunkyList(ChunkyFileWriter writer, IList<KeyValueEntry> list)
    {
        // int count
        // * for count
        // - ulong key
        // - int type
        // - int index (indexes data below)
        // * for count
        // - type[i] data
        MemoryStream dataStream = new();
        ChunkyFileWriter dataWriter = new(dataStream);

        writer.Write(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            writer.Write(list[i].Key);
            switch (list[i].Value)
            {
                case float dataFloat:
                    writer.Write((int)RGDDataType.Float);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataFloat);
                    break;
                case int dataInt:
                    writer.Write((int)RGDDataType.Int);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataInt);
                    break;
                case bool dataBool:
                    writer.Write((int)RGDDataType.Boolean);
                    writer.Write((int)dataStream.Position);
                    dataWriter.Write(dataBool);
                    break;
                case string dataString:
                    writer.Write((int)RGDDataType.CString);
                    writer.Write((int)dataStream.Position);
                    dataWriter.WriteCString(dataString);
                    break;
                case RGDList dataList:
                    writer.Write((int)RGDDataType.List);
                    writer.Write((int)dataStream.Position);
                    WriteChunkyList(dataWriter, dataList);
                    break;
                case RGDNode[] nodes:
                    writer.Write((int)RGDDataType.List);
                    writer.Write((int)dataStream.Position);
                    WriteChunkyList(dataWriter, nodes.Select(node => new KeyValueEntry((ulong)((long)node.Key.GetHashCode() + int.MaxValue), node.Value)).ToArray());
                    break;
                default:
                    throw new NotImplementedException($"Unknown type {list[i].Value}");
            }
        }

        writer.Write(dataStream.ToArray());
    }

    private static void WriteKeyValueDataChunk(ChunkyFileWriter writer, KeyValueDataChunk chunk)
    {
        writer.Write(0); // unknown
        WriteChunkyList(writer, chunk.KeyValues);
    }

    private static void WriteKeysDataChunk(ChunkyFileWriter writer, KeysDataChunk chunk)
    {
        // int count
        // * for count
        // - ulong keyHash
        // - int stringLength
        // - string key

        writer.Write(chunk.StringKeys.Count);
        foreach (var (key, keyHash) in chunk.StringKeys)
        {
            writer.Write(keyHash);
            byte[] keyData = Encoding.UTF8.GetBytes(key);
            writer.Write(keyData.Length);
            writer.Write(keyData);
        }
    }
}
