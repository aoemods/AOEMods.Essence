using AOEMods.Essence.Chunky.Core;
using AOEMods.Essence.Chunky.Graph;
using System.Text;

namespace AOEMods.Essence.Chunky.RRMaterial;

public class RRMaterialReader : IRRMaterialReader
{
    public static IEnumerable<Material> ReadRRMaterial(Stream stream, string? materialName = null)
    {
        using var reader = new ChunkyFileReader(stream);

        var chunky = ChunkyFile.FromStream(stream);
        var materialNodes = chunky.RootNodes
            .OfType<IChunkyFolderNode>()
            .Where(node => node.Header.Name == "GMat");

        if (!string.IsNullOrEmpty(materialName))
        {
            materialNodes = materialNodes.Where(node => node.Header.Path == materialName);
        }

        foreach (var materialNode in materialNodes)
        {
            var matPNode = materialNode.Children.OfType<IChunkyFolderNode>().Single(node => node.Header.Name == "MatP");
            var pbNodes = matPNode.Children.OfType<IChunkyFolderNode>().Where(node => node.Header.Name == "\0\0Pb");
            foreach (var pbNode in pbNodes)
            {
                var pbTextureNode = pbNode.Children.OfType<IChunkyDataNode>().Single(node => node.Header.Name == "PbTe");

                reader.BaseStream.Position = pbTextureNode.Header.DataPosition;

                uint textureCount = reader.ReadUInt32();
                var textures = new Dictionary<string, string>((int)textureCount);

                for (uint i = 0; i < textureCount; i++)
                {
                    uint keyLength = reader.ReadUInt32();
                    string key = Encoding.UTF8.GetString(reader.ReadBytes((int)keyLength));
                    uint valueLength = reader.ReadUInt32();
                    string value = Encoding.UTF8.GetString(reader.ReadBytes((int)valueLength));
                    if (textures.ContainsKey(key) && textures[key] != value)
                    {
                        throw new Exception($"Textures already contains key {key} but does not match value {value}");
                    }
                    textures[key] = value;
                }

                yield return new Material(new RRMaterialPbTextures(textures));
            }
        }
    }
}
