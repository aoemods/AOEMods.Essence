using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public class ExportFileDialogViewModel : ObservableRecipient
{
    public object? ExportOptionsViewModel
    {
        get;
        init;
    }

    public string OutputFilePath
    {
        get => outputFilePath;
        set {
            SetProperty(ref outputFilePath, value);
            OnPropertyChanged(nameof(IsExportEnabled));
        }
    }
    private string outputFilePath = "";

    public string InitialFileName { get; set; }
    public string Extension { get; set; }

    public bool IsExportEnabled { get => !string.IsNullOrWhiteSpace(OutputFilePath); }

    public ICommand BrowseOutputFileCommand { get; }

    public ExportFileDialogViewModel()
    {
        BrowseOutputFileCommand = new RelayCommand(BrowseOutputFile);
        Extension = "";
        InitialFileName = "";
    }

    private void BrowseOutputFile()
    {
        var defaultExt = Extension;
        if (ExportOptionsViewModel is IExportRgdOptions exportRgdOptions && exportRgdOptions.ConvertRgd)
        {
            defaultExt = exportRgdOptions.Format.StartsWith(".") ? exportRgdOptions.Format : "." + exportRgdOptions.Format;
        }
        SaveFileDialog dialog = new()
        {
            Title = "Select a directory to unpack the archive's folder into",
            FileName = Path.ChangeExtension(InitialFileName, defaultExt),
            DefaultExt = defaultExt,
            Filter = $"{defaultExt}|*{defaultExt}|All Files|*.*",
            FilterIndex = 2
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFilePath = dialog.FileName;
        }
    }
}
