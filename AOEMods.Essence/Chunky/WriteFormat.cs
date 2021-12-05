using AOEMods.Essence.Chunky.RGD;

namespace AOEMods.Essence.Chunky
{
    public static class WriteFormat
    {
        public static void RGD(Stream stream, IList<RGDNode> nodes)
        {
            ChunkyFileWriter writer = new(stream);

            // Write chunky file header
            writer.Write(new ChunkyFileHeader(RGDUtil.Magic, 4, 1));

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
            RGDUtil.WriteKeyValueDataChunk(writer, new KeyValueDataChunk(nodes.Select(node => new KeyValueEntry(hashes[node.Key], node.Value)).ToArray()));
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
            RGDUtil.WriteKeysDataChunk(writer, new KeysDataChunk(hashes));
            postDataPosition = stream.Position;
            stream.Position = headerPosition;
            header = new("DATA", "KEYS", 3, (int)(postDataPosition - dataPosition), "", dataPosition);
            writer.Write(header);
            stream.Position = postDataPosition;
        }
    }
}
