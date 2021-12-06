using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.Chunky.RRMaterial;
using AOEMods.Essence.Chunky.RRTex;
using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.Messaging;
using Ookii.Dialogs.Wpf;
using SharpGLTF.Materials;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AOEMods.Essence.Editor;

public interface IExportRRGeomOptions
{
    public bool ConvertRRGeom { get; set; }
    public bool OnlyGeometry { get; set; }
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
    public bool OnlyGeometry { get => rrgeomOptions.OnlyGeometry; set => rrgeomOptions.OnlyGeometry = value; }

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

        if (options.ExportAllMips)
        {
            foreach (var texture in ReadFormat.RRTex(new MemoryStream(node.GetData().ToArray()), options.RRTexFormat))
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
            TextureMip? texture = ReadFormat.RRTexLastMip(new MemoryStream(node.GetData().ToArray()), options.RRTexFormat);

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

        IArchive[] archives = WeakReferenceMessenger.Default.Send(new ArchivesRequest()).Response.ToArray();

        TextureMip? FindTexture(string path, RRTexType type)
        {
            foreach (var archive in archives)
            {
                var file = archive.FindFile(path);
                if (file != null)
                {
                    MemoryStream stream = new(file.GetData().ToArray());
                    return ReadFormat.RRTexLastMip(stream, PngFormat.Instance, type);
                }
            }

            return null;
        }

        Material? FindMaterial(string path, string? materialName)
        {
            foreach (var archive in archives)
            {
                var file = archive.FindFile(path);
                if (file != null)
                {
                    MemoryStream stream = new(file.GetData().ToArray());
                    return ReadFormat.RRMaterial(stream, materialName).First();
                }
            }

            return null;
        }

        int objectIndex = 0;
        foreach (var geometryObject in ReadFormat.RRGeom(new MemoryStream(node.GetData().ToArray())))
        {
            MaterialBuilder? gltfMaterial = null;

            if (!options.OnlyGeometry)
            {
                var material = FindMaterial(Path.ChangeExtension(node.FullName, ".rrmaterial"), geometryObject.MaterialName);
                if (material != null)
                {
                    string? diffusePath = null;
                    if (material.LodTextures.Textures.TryGetValue("albedoTexture", out string diffPath))
                    {
                        diffusePath = diffPath;
                    }
                    else if (material.LodTextures.Textures.TryGetValue("albedoTex", out string diffPath2))
                    {
                        diffusePath = diffPath2;
                    }

                    var diffuseTexture = diffusePath != null ? FindTexture(diffusePath, RRTexType.Generic) : null;

                    string? normalPath = null;
                    if (material.LodTextures.Textures.TryGetValue("normalMap", out string normPath))
                    {
                        normalPath = normPath;
                    }

                    var normalTexture = normalPath != null ? FindTexture(normalPath, RRTexType.NormalMap) : null;
                    var metalTexture = normalPath != null ? FindTexture(normalPath, RRTexType.Metal) : null;

                    gltfMaterial = GltfUtil.MaterialFromTextures(diffuseTexture, normalTexture, metalTexture);
                }
            }

            if (gltfMaterial == null)
            {
                gltfMaterial = GltfUtil.MaterialFromTextures(null, null, null);
            }

            var gltfModel = GltfUtil.GeometryObjectToModel(geometryObject, gltfMaterial);

            gltfModel.SaveGLB(Path.Combine(
                Path.GetDirectoryName(outPath),
                $"{Path.GetFileNameWithoutExtension(outPath)}_{objectIndex}.glb"
            ));

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
        List<IArchiveFileNode> fileNodes = new();

        void CollectFileNodesRecursive(IArchiveNode childNode)
        {
            if (childNode is IArchiveFolderNode folderNode)
            {
                foreach (var childNodeChild in folderNode.Children)
                {
                    CollectFileNodesRecursive(childNodeChild);
                }
            }
            else if (childNode is IArchiveFileNode fileNode)
            {
                fileNodes.Add(fileNode);
            }
        }

        CollectFileNodesRecursive(node);

        ProgressDialog progressDialog = new()
        {
            WindowTitle = "Extracting archive",
            ShowTimeRemaining = true,
            ProgressBarStyle = ProgressBarStyle.ProgressBar,
        };

        int progressPercent = 0;
        int processedNodes = 0;

        progressDialog.DoWork += (o, e) =>
        {
            void ProcessNode(IArchiveFileNode file)
            {
                string relativePath = node.FullName == "" ?
                    file.FullName :
                    Path.GetRelativePath(node.FullName, file.FullName);
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

                processedNodes++;

                int newProgressPercent = (int)Math.Round(100 * (float)processedNodes / Math.Max(1, fileNodes.Count));
                if (newProgressPercent > progressPercent)
                {
                    progressPercent = newProgressPercent;
                    progressDialog.ReportProgress(progressPercent, node.FullName, $"{processedNodes} / {fileNodes.Count}");
                }
            }

            foreach (var fileNode in fileNodes)
            {
                if (progressDialog.CancellationPending)
                {
                    break;
                }

                ProcessNode(fileNode);
            }
        };

        progressDialog.Show();
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
