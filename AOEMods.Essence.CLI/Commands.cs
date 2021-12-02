using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRGeom;
using AOEMods.Essence.SGA;
using Microsoft.Extensions.FileSystemGlobbing;
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
        var archiveWriter = new ArchiveWriter(outFile);
        archiveWriter.Write(archive);

        return 0;
    }

    public static int SgaUnpack(SgaUnpackOptions options)
    {
        ArchiveReader archiveReader = new ArchiveReader(File.OpenRead(options.InputPath), Encoding.ASCII);
        var arch = archiveReader.ReadArchive();

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
                string directoryName = Path.GetDirectoryName(outputPath);

                foreach (var texture in ReadFormat.RRTex(rrtexStream, format))
                {
                    File.WriteAllBytes(Path.Join(directoryName, $"{texture.Mip}_{fileName}"), texture.Data);
                }
            }
            else
            {
                try
                {
                    var texture = ReadFormat.RRTex(rrtexStream, format).First();
                    File.WriteAllBytes(outputPath, texture.Data);
                }
                catch (InvalidOperationException ex)
                {
                    if (options.Verbose)
                    {
                        Console.WriteLine("No textures could be decoded for {0}: {1}", path, ex);
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

    public static int RGDDecode(RGDDecodeOptions options)
    {
        static void ConvertFile(string path, string outputPath)
        {
            using var rgdStream = File.Open(path, FileMode.Open, FileAccess.Read);
            var nodes = ReadFormat.RGD(rgdStream);
            string json = GameDataJsonUtil.GameDataToJson(nodes);

            File.WriteAllText(outputPath, json);
        }

        if (options.Batch)
        {
            BatchConvert(options.InputPath, options.OutputPath, "**/*.rgd", ".json", ConvertFile);
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
            string directoryName = Path.GetDirectoryName(outputPath);

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            int objectIndex = 0;
            foreach (var geometryObject in ReadFormat.RRGeom(stream))
            {
                // Write .obj file
                using var fileStream = File.Open(
                    Path.Join(directoryName, $"{objectIndex}_{fileName}"),
                    FileMode.Create
                );

                RRGeomUtil.WriteGeometryObject(fileStream, geometryObject);

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
}
