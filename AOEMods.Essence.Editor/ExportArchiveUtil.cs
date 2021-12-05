using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRGeom;
using AOEMods.Essence.Chunky.RRTex;
using AOEMods.Essence.SGA;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AOEMods.Essence.Editor;

public interface IExportRRGeomOptions
{
    public bool ConvertRRGeom { get; set; }
}

public interface IExportRRTexOptions
{
    public bool ConvertRRTex { get; set; }
    public bool ExportAllMips { get; set; }
    public IImageFormat RRTexFormat { get; set; }
}

public interface IExportRgdOptions
{
    public bool ConvertRgd { get; set; }
    public string Format { get; set; }
}

public interface IExportArchiveNodeOptions : IExportRRTexOptions, IExportRRGeomOptions, IExportRgdOptions
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
    public bool ConvertRRGeom { get => rrgeomOptions.ConvertRRGeom; set => rrgeomOptions.ConvertRRGeom = value; }
    public bool ConvertRgd { get => rgdOptions.ConvertRgd; set => rgdOptions.ConvertRgd = value; }
    public string Format { get => rgdOptions.Format; set => rgdOptions.Format = value; }

    private readonly IExportRRTexOptions rrtexOptions;
    private readonly IExportRRGeomOptions rrgeomOptions;
    private readonly IExportRgdOptions rgdOptions;

    public ExportArchiveNodeOptions(
        string outputDirectoryPath, IExportRRTexOptions rrtexOptions,
        IExportRRGeomOptions rrgeomOptions, IExportRgdOptions rgdOptions
    )
    {
        OutputDirectoryPath = outputDirectoryPath;
        this.rrtexOptions = rrtexOptions;
        this.rrgeomOptions = rrgeomOptions;
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
        if (options.ExportAllMips)
        {
            foreach (var texture in textures)
            {
                var mipOutPath = Path.Combine(
                    Path.GetDirectoryName(outPath),
                    $"{Path.GetFileNameWithoutExtension(outPath)}_mip{texture.Mip}.png"
                );

                File.WriteAllBytes(mipOutPath, texture.Data);
            }
        }
        else
        {
            TextureMip? texture;
            try
            {
                texture = textures.Last();
            }
            catch
            {
                texture = null;
            }

            if (texture.HasValue)
            {
                var mipOutPath = Path.Combine(
                    Path.GetDirectoryName(outPath),
                    Path.ChangeExtension(outPath, ".png")
                );

                File.WriteAllBytes(mipOutPath, texture.Value.Data);
            }
        }
    }

    private static void ExportRRGeomNode(IArchiveFileNode node, string outPath, IExportRRGeomOptions options)
    {
        if (!options.ConvertRRGeom)
        {
            ExportRawNode(node, outPath);
            return;
        }

        int objectIndex = 0;
        foreach (var geometryObject in ReadFormat.RRGeom(new MemoryStream(node.GetData().ToArray())))
        {
            // Write .obj file
            using var fileStream = File.Open(
                Path.Combine(
                    Path.GetDirectoryName(outPath),
                    $"{Path.GetFileNameWithoutExtension(outPath)}_{objectIndex}.obj"
                ),
                FileMode.Create
            );

            RRGeomUtil.WriteGeometryObject(fileStream, geometryObject);

            objectIndex++;
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

        Func<IList<RGDNode>, string> conversionFunc = options.Format switch
        {
            "json" => GameDataJsonUtil.GameDataToJson,
            "xml" => GameDataXmlUtil.GameDataToXml,
            _ => throw new NotSupportedException($"Unsupported output format {options.Format}, supported values are json and xml.")
        };

        string converted = conversionFunc(nodes);
        File.WriteAllText(Path.ChangeExtension(outPath, $".{options.Format}"), converted);
    }

    public static void ExportArchiveNode(IArchiveNode node, IExportArchiveNodeOptions options)
    {
        void ExportRecursive(IArchiveNode childNode)
        {
            if (childNode is IArchiveFileNode file)
            {
                string relativePath = node.FullName == "" ?
                    childNode.FullName :
                    Path.GetRelativePath(node.FullName, childNode.FullName);
                string outPath = Path.Join(options.OutputDirectoryPath, node.Name, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                switch (file.Extension)
                {
                    case ".rrtex":
                        ExportRRTexNode(file, outPath, options);
                        break;
                    case ".rrgeom":
                        ExportRRGeomNode(file, outPath, options);
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
                        ".rrgeom" => new ExportRRGeomViewModel(),
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
                            case IExportRRGeomOptions rrgeomOptions:
                                ExportRRGeomNode(file, outPath, rrgeomOptions);
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
                            dialog.ViewModel.RRGeomViewModel,
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
