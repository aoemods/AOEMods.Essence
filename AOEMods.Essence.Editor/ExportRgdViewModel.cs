using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AOEMods.Essence.Editor;

public class ExportRgdViewModel : ObservableRecipient, IExportRgdOptions
{
    public bool ConvertRgd
    {
        get => convertRgd;
        set => SetProperty(ref convertRgd, value);
    }

    private bool convertRgd;
}
