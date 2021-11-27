using CommandLine;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using System.Text;
using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.CLI;
using AOEMods.Essence.SGA;

int SgaPack(SgaPackOptions options)
{
    var archive = ArchiveReaderHelper.DirectoryToArchive(options.InputPath, options.ArchiveName);

    using var outFile = File.Open(options.OutputPath, FileMode.Create, FileAccess.Write);
    var archiveWriter = new ArchiveWriter(outFile);
    archiveWriter.Write(archive);

    return 0;
}

int SgaUnpack(SgaUnpackOptions options)
{
    ArchiveReader archiveReader = new ArchiveReader(File.OpenRead(@"D:\Microsoft.Cardinal_5.0.7274.0_x64__8wekyb3d8bbwe\cardinal\archives\attrib.sga"), Encoding.ASCII);
    var arch = archiveReader.ReadArchive();

    var fileNodes = ArchiveNodeHelper.GatherOfType<IArchiveFileNode>(arch.Tocs[0]);

    foreach (var fileNode in fileNodes)
    {
        string fullName = fileNode.FullName;
        if (options.Verbose)
        {
            Console.WriteLine(fullName);
        }
        var outputFilePath = Path.Join(options.OutputPath, fullName);
        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (outputDirectory != null)
        {
            Directory.CreateDirectory(outputDirectory);
        }
        File.WriteAllBytes(outputFilePath, fileNode.GetData().ToArray());
    }

    return 0;
}

int RRTexDecode(RRTexDecodeOptions options)
{
    using var rrtexStream = File.Open(options.InputPath, FileMode.Open, FileAccess.Read);

    string extension = Path.GetExtension(options.OutputPath);

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
        string fileName = Path.GetFileName(options.OutputPath);
        string directoryName = Path.GetDirectoryName(options.OutputPath);

        foreach (var texture in ReadFormat.RRTex(rrtexStream, format))
        {
            File.WriteAllBytes(Path.Join(directoryName, $"{texture.Mip}_{fileName}"), texture.Data);
        }
    }
    else
    {
        var texture = ReadFormat.RRTex(rrtexStream, format).First();
        File.WriteAllBytes(options.OutputPath, texture.Data);
    }

    return 0;
}

int RGDDecode(RGDDecodeOptions options)
{
    using var rgdStream = File.Open(options.InputPath, FileMode.Open, FileAccess.Read);

    var nodes = ReadFormat.RGD(rgdStream);

    string json = AOEMods.Essence.Chunky.RGD.GameDataJsonUtil.GameDataToJson(nodes);

    File.WriteAllText(options.OutputPath, json);

    return 0;
}

int OnError(IEnumerable<Error> errors)
{
    return 1;
}

return Parser.Default.ParseArguments<SgaPackOptions, SgaUnpackOptions, RRTexDecodeOptions, RGDDecodeOptions>(args)
    .MapResult<SgaPackOptions, SgaUnpackOptions, RRTexDecodeOptions, RGDDecodeOptions, int>(
        SgaPack,
        SgaUnpack,
        RRTexDecode,
        RGDDecode,
        OnError
    );
