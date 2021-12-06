using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AOEMods.Essence.Editor;

public class ExportRRGeomViewModel : ObservableRecipient, IExportRRGeomOptions
{
    public bool ConvertRRGeom
    {
        get => convertRRGeom;
        set => SetProperty(ref convertRRGeom, value);
    }

    private bool convertRRGeom;

    public bool OnlyGeometry
    {
        get => onlyGeometry;
        set => SetProperty(ref onlyGeometry, value);
    }

    private bool onlyGeometry;
}
