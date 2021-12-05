using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace AOEMods.Essence.Editor;

public class ExportRgdViewModel : ObservableRecipient, IExportRgdOptions
{
    public bool ConvertRgd
    {
        get => convertRgd;
        set => SetProperty(ref convertRgd, value);
    }

    private bool convertRgd;

    private readonly IList<string> formats = new[]
    {
        "xml",
        "json"
    };

    public string Format
    {
        get => formats[FormatIndex];
        set => FormatIndex = formats.IndexOf(value);
    }

    public int FormatIndex
    {
        get => formatIndex;
        set => SetProperty(ref formatIndex, value);
    }

    private int formatIndex = 0;
}
