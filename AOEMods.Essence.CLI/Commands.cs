using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRTex;
using AOEMods.Essence.SGA.Core;
using AOEMods.Essence.SGA.Graph;
using Microsoft.Extensions.FileSystemGlobbing;
using SharpGLTF.Materials;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using System.Text;

namespace AOEMods.Essence.CLI;

public static class Commands
{
    public static int SgaPack(SgaPackOptions options)
    {
        var archive = ArchiveReaderHelper.DirectoryToArchive(options.InputPath, options.ArchiveName);

        using var outFile = File.Open(options.OutputPath, FileMode.Create, FileAccess.Write);
        ArchiveWriterHelper.WriteArchiveToStream(outFile, archive);

        return 0;
    }

    public static int SgaUnpack(SgaUnpackOptions options)
    {
        using var archiveFile = File.OpenRead(options.InputPath);
        var arch = Archive.FromStream(archiveFile);

        var fileNodes = ArchiveNodeHelper.EnumerateChildren(arch.Tocs[0].RootFolder).OfType<IArchiveFileNode>();

        foreach (var fileNode in fileNodes)
        {
            string fullName = fileNode.FullName;
            if (options.Verbose)
            {
                Console.WriteLine(fullName);
            }
            var outputFilePath = Path.Join(options.OutputPath, fullName.Replace('\\', '/'));
            if (options.Verbose)
            {
                Console.WriteLine("Writing {0} to {1}", fileNode.FullName, outputFilePath);
            }
            var outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (outputDirectory != null)
            {
                Directory.CreateDirectory(outputDirectory);
            }
            File.WriteAllBytes(outputFilePath, fileNode.GetData().ToArray());
        }

        return 0;
    }

    private static void BatchConvert(string inputDirectoryPath, string outputDirectoryPath, string pattern, string outputExtension, Action<string, string> convertFile)
    {
        Matcher matcher = new();
        matcher.AddInclude(pattern);
        var inputPaths = matcher.GetResultsInFullPath(inputDirectoryPath).ToArray();
        Console.WriteLine("Found {0} files matching {1}", inputPaths.Length, inputDirectoryPath);
        using (var progress = new ProgressBar())
        {
            int processed = 0;
            foreach (var inputPath in inputPaths)
            {
                var relativePath = Path.GetRelativePath(inputDirectoryPath, inputPath);
                var outRelativePath = Path.ChangeExtension(relativePath, outputExtension);
                var outputPath = Path.Combine(outputDirectoryPath, outRelativePath);
                var directoryName = Path.GetDirectoryName(outputPath);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                }

                convertFile(inputPath, outputPath);
                processed++;
                progress.Report((double)processed / inputPaths.Length);
            }
        }
    }

    public static int RRTexDecode(RRTexDecodeOptions options)
    {
        void ConvertFile(string path, string outputPath)
        {
            using var rrtexStream = File.Open(path, FileMode.Open, FileAccess.Read);

            string extension = Path.GetExtension(outputPath);

            IImageFormat format = extension switch
            {
                ".png" => PngFormat.Instance,
                ".jpg" or ".jpeg" => JpegFormat.Instance,
                ".bmp" => BmpFormat.Instance,
                ".tga" => TgaFormat.Instance,
                ".gif" => GifFormat.Instance,
                _ => throw new NotSupportedException($"Output extension {extension} not supported")
            };

            if (options.AllMips)
            {
                string fileName = Path.GetFileName(outputPath);
                string? directoryName = Path.GetDirectoryName(outputPath);

                foreach (var texture in FormatReader.ReadRRTex(rrtexStream, format))
                {
                    File.WriteAllBytes(Path.Join(directoryName, $"{texture.Mip}_{fileName}"), texture.Data);
                }
            }
            else
            {
                try
                {
                    var texture = FormatReader.ReadRRTexLastMip(rrtexStream, format);
                    if (texture != null)
                    {
                        File.WriteAllBytes(outputPath, texture.Value.Data);
                    }
                    else if(options.Verbose)
                    {
                        Console.WriteLine("No textures could be decoded for {0}", path);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (options.Verbose)
                    {
                        Console.WriteLine("Failed to decode textures for {0}: {1}", path, ex);
                    }
                }
            }
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.rrtex", ".png", ConvertFile);
        }
        else
        {
            ConvertFile(options.InputPath, options.OutputPath);
        }

        return 0;
    }

    public static int RGDEncode(RGDEncodeOptions options)
    {
        Func<string, RGDNode[]> loadNodes = options.Format switch
        {
            "xml" => GameDataXmlUtil.XmlToGameData,
            _ => throw new NotSupportedException($"Unsupported inpout format {options.Format}, only xml is supported")
        };

        void ConvertFile(string path, string outputPath)
        {
            var nodes = loadNodes(File.ReadAllText(path));

            using var outStream = File.Open(outputPath, FileMode.Create, FileAccess.Write);
            FormatWriter.WriteRGD(outStream, nodes);
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.xml", ".rgd", ConvertFile);
        }
        else
        {
            ConvertFile(options.InputPath, options.OutputPath);
        }


        return 0;
    }

    public static int RGDDecode(RGDDecodeOptions options)
    {
        Func<IList<RGDNode>, string> conversionFunc = options.Format switch
        {
            "json" => GameDataJsonUtil.GameDataToJson,
            "xml" => GameDataXmlUtil.GameDataToXml,
            _ => throw new NotSupportedException($"Unsupported output format {options.Format}, supported values are json and xml.")
        };

        void ConvertFile(string path, string outputPath)
        {
            using var rgdStream = File.Open(path, FileMode.Open, FileAccess.Read);
            var nodes = FormatReader.ReadRGD(rgdStream);

            string converted = conversionFunc(nodes);
            File.WriteAllText(outputPath, converted);
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.rgd", $".{options.Format}", ConvertFile);
        }
        else
        {
            ConvertFile(options.InputPath, options.OutputPath);
        }

        return 0;
    }

    public static int RRGeomDecode(RRGeomDecodeOptions options)
    {
        static void ConvertFile(string path, string outputPath)
        {
            string fileName = Path.GetFileName(outputPath);
            string? directoryName = Path.GetDirectoryName(outputPath);

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            int objectIndex = 0;
            foreach (var geometryObject in FormatReader.ReadRRGeom(stream))
            {
                // Write .obj file
                using var fileStream = File.Open(
                    Path.Join(directoryName, $"{objectIndex}_{fileName}"),
                    FileMode.Create
                );

                ObjUtil.WriteGeometryObjectAsObj(fileStream, geometryObject);

                objectIndex++;
            }
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.rrgeom", ".obj", ConvertFile);
        }
        else
        {
            ConvertFile(options.InputPath, options.OutputPath);
        }

        return 0;
    }

    public static int ModelExport(ModelExportOptions options)
    {
        string? materialDirectory = string.IsNullOrEmpty(options.MaterialInputPath) ?
            (Path.GetDirectoryName(options.InputPath)) :
            options.MaterialInputPath;

        string? textureDirectory = string.IsNullOrEmpty(options.TextureInputPath) ?
            materialDirectory :
            options.TextureInputPath;

        bool TryFindMaterialPath(string path, out string materialPath)
        {
            materialPath = Path.Combine(materialDirectory ?? "", Path.ChangeExtension(Path.GetRelativePath(options.InputPath, path), ".rrmaterial"));
            if (File.Exists(materialPath))
            {
                return true;
            }

            materialPath = Path.ChangeExtension(path, ".rrmaterial");
            if (File.Exists(materialPath))
            {
                return true;
            }

            return false;
        }

        void ConvertFile(string path, string outputPath)
        {
            MaterialBuilder material;
            if (options.WithMaterial)
            {
                if (!TryFindMaterialPath(path, out var materialPath))
                {
                    material = MaterialBuilder.CreateDefault();
                    if (options.Verbose)
                    {
                        Console.WriteLine("Could not find material file at {0} for {1}", materialPath, path);
                    }
                }
                else
                {
                    using var materialStream = File.OpenRead(materialPath);
                    var materials = FormatReader.ReadRRMaterial(materialStream).ToArray();
                    var texturePaths = materials.First(mat => mat.LodTextures.Textures.ContainsKey("albedoTexture") || mat.LodTextures.Textures.ContainsKey("albedoTex")).LodTextures.Textures;

                    var albedoPath = Path.Combine(textureDirectory ?? "", texturePaths.ContainsKey("albedoTexture") ? texturePaths["albedoTexture"] : texturePaths["albedoTex"]);
                    using var albedoStream = File.OpenRead(albedoPath);
                    var albedoTexture = FormatReader.ReadRRTexLastMip(albedoStream, PngFormat.Instance);

                    TextureMip? normalTexture = null;
                    TextureMip? metalTexture = null;

                    var normalPath = Path.Combine(textureDirectory ?? "", texturePaths["normalMap"]);
                    if (File.Exists(normalPath))
                    {
                        using var normalStream = File.OpenRead(normalPath);
                        normalTexture = FormatReader.ReadRRTexLastMip(normalStream, PngFormat.Instance, RRTexType.NormalMap);
                        normalStream.Position = 0;
                        metalTexture = FormatReader.ReadRRTexLastMip(normalStream, PngFormat.Instance, RRTexType.Metal);
                    }

                    material = GltfUtil.MaterialFromTextures(albedoTexture, normalTexture, metalTexture);
                }
            }
            else
            {
                material = MaterialBuilder.CreateDefault();
            }

            string fileName = Path.GetFileName(outputPath);
            string? directoryName = Path.GetDirectoryName(outputPath);

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            int objectIndex = 0;
            foreach (var geometryObject in FormatReader.ReadRRGeom(stream))
            {
                var model = GltfUtil.GeometryObjectToModel(geometryObject, material);

                using var fileStream = File.Open(
                    Path.Join(directoryName, $"{objectIndex}_{fileName}"),
                    FileMode.Create
                );
                model.WriteGLB(fileStream);

                objectIndex++;
            }
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.rrgeom", ".glb", ConvertFile);
        }
        else
        {
            ConvertFile(options.InputPath, options.OutputPath);
        }

        return 0;
    }
}
