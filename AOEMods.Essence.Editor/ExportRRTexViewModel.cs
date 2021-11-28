using Microsoft.Toolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace AOEMods.Essence.Editor;

public class ExportRRTexViewModel : ObservableRecipient, IExportRRTexOptions
{
    public bool ConvertRRTex
    {
        get => convertRRTex;
        set => SetProperty(ref convertRRTex, value);
    }

    private bool convertRRTex;

    public bool ExportAllMips
    {
        get => exportAllMips;
        set => SetProperty(ref exportAllMips, value);
    }

    private bool exportAllMips;

    public IImageFormat RRTexFormat { get; set; } = PngFormat.Instance;
}
