using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.SGA;
using Microsoft.Win32;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Linq;

namespace AOEMods.Essence.Editor;


public interface IExportRRTexOptions
{
    public bool ConvertRRTex { get; set; }
    public bool ExportAllMips { get; set; }
    public IImageFormat RRTexFormat { get; set; }
}

public interface IExportRgdOptions
{
    public bool ConvertRgd { get; set; }
}

public interface IExportArchiveNodeOptions : IExportRRTexOptions, IExportRgdOptions
{
    public string OutputDirectoryPath
    {
        get;
    }
}

public class ExportArchiveNodeOptions : IExportArchiveNodeOptions
{
    public string OutputDirectoryPath { get; set; }
    public bool ConvertRRTex { get => rrtexOptions.ConvertRRTex; set => rrtexOptions.ConvertRRTex = value; }
    public bool ExportAllMips { get => rrtexOptions.ExportAllMips; set => rrtexOptions.ExportAllMips = value; }
    public IImageFormat RRTexFormat { get => rrtexOptions.RRTexFormat; set => rrtexOptions.RRTexFormat = value; }
    public bool ConvertRgd { get => rgdOptions.ConvertRgd; set => rgdOptions.ConvertRgd = value; }
    private readonly IExportRRTexOptions rrtexOptions;
    private readonly IExportRgdOptions rgdOptions;

    public ExportArchiveNodeOptions(string outputDirectoryPath, IExportRRTexOptions rrtexOptions, IExportRgdOptions rgdOptions)
    {
        OutputDirectoryPath = outputDirectoryPath;
        this.rrtexOptions = rrtexOptions;
        this.rgdOptions = rgdOptions;
    }
}

public static class ExportArchiveUtil
{
    private static void ExportRawNode(IArchiveFileNode node, string outPath)
    {
        File.WriteAllBytes(outPath, node.GetData().ToArray());
    }

    private static void ExportRRTexNode(IArchiveFileNode node, string outPath, IExportRRTexOptions options)
    {
        if (!options.ConvertRRTex)
        {
            ExportRawNode(node, outPath);
            return;
        }

        var textures = ReadFormat.RRTex(new MemoryStream(node.GetData().ToArray()), options.RRTexFormat);
        foreach (var texture in textures)
        {
            var mipOutPath = Path.Combine(
                Path.GetDirectoryName(outPath),
                options.ExportAllMips ?
                    $"{Path.GetFileNameWithoutExtension(outPath)}_mip{texture.Mip}.png" :
                    outPath
            );

            File.WriteAllBytes(mipOutPath, texture.Data);

            if (!options.ExportAllMips)
            {
                break;
            }
        }
    }

    private static void ExportRgdNode(IArchiveFileNode node, string outPath, IExportRgdOptions options)
    {
        if (!options.ConvertRgd)
        {
            ExportRawNode(node, outPath);
            return;
        }

        var nodes = ReadFormat.RGD(new MemoryStream(node.GetData().ToArray()));
        string json = GameDataJsonUtil.GameDataToJson(nodes);
        File.WriteAllText(Path.ChangeExtension(outPath, ".json"), json);
    }

    public static void ExportArchiveNode(IArchiveNode node, IExportArchiveNodeOptions options)
    {
        void ExportRecursive(IArchiveNode childNode)
        {
            if (childNode is IArchiveFileNode file)
            {
                string relativePath = Path.GetRelativePath(node.FullName, childNode.FullName);
                string outPath = Path.Join(options.OutputDirectoryPath, node.Name, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                switch (file.Extension)
                {
                    case ".rrtex":
                        ExportRRTexNode(file, outPath, options);
                        break;
                    case ".rgd":
                        ExportRgdNode(file, outPath, options);
                        break;
                    default:
                        ExportRawNode(file, outPath);
                        break;
                }
            }
            else if (childNode is IArchiveFolderNode folderNode)
            {
                foreach (var childNodeChild in folderNode.Children)
                {
                    ExportRecursive(childNodeChild);
                }
            }
        }

        ExportRecursive(node);
    }

    public static bool? ShowExportArchiveNodeDialog(IArchiveNode node, string title)
    {
        bool? dialogResult;

        switch (node)
        {
            case IArchiveFileNode file:
                {
                    ExportFileDialog dialog = new(file.Extension switch
                    {
                        ".rrtex" => new ExportRRTexViewModel(),
                        ".rgd" => new ExportRgdViewModel(),
                        _ => null
                    })
                    {
                        Title = title
                    };

                    dialogResult = dialog.ShowDialog();
                    if (dialogResult == true)
                    {
                        var outPath = dialog.ViewModel.OutputFilePath;
                        switch (dialog.ViewModel.ExportOptionsViewModel)
                        {
                            case IExportRRTexOptions rrtexOptions:
                                ExportRRTexNode(file, outPath, rrtexOptions);
                                break;
                            case IExportRgdOptions rgdOptions:
                                ExportRgdNode(file, outPath, rgdOptions);
                                break;
                            case null:
                                ExportRawNode(file, outPath);
                                break;
                            default:
                                throw new NotSupportedException($"Unhandled export options view model {dialog.ViewModel.ExportOptionsViewModel.GetType().FullName}");
                        }
                    }
                    break;
                }
            case IArchiveFolderNode:
                {
                    ExportFolderDialog dialog = new()
                    {
                        Title = title
                    };

                    dialogResult = dialog.ShowDialog();

                    if (dialogResult == true)
                    {
                        ExportArchiveNode(node, new ExportArchiveNodeOptions(
                            dialog.ViewModel.OutputDirectoryPath,
                            dialog.ViewModel.RRTexViewModel,
                            dialog.ViewModel.RgdViewModel
                        ));
                    }
                    break;
                }
            default:
                throw new NotSupportedException($"Can not export node of type {node.GetType()}");
        }

        return dialogResult;
    }
}
