using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
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
        set => SetProperty(ref outputFilePath, value);
    }
    private string outputFilePath = "";

    public ICommand BrowseOutputFileCommand { get; }

    public ExportFileDialogViewModel()
    {
        BrowseOutputFileCommand = new RelayCommand(BrowseOutputFile);
    }

    private void BrowseOutputFile()
    {
        SaveFileDialog dialog = new()
        {
            Title = "Select a directory to unpack the archive's folder into"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFilePath = dialog.FileName;
        }
    }
}
