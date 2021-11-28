using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public class ExportFolderDialogViewModel : ObservableRecipient
{
    public ExportRRTexViewModel RRTexViewModel { get; } = new();
    public ExportRgdViewModel RgdViewModel { get; } = new();

    public string OutputDirectoryPath
    {
        get => outputDirectoryPath;
        set => SetProperty(ref outputDirectoryPath, value);
    }
    private string outputDirectoryPath = "";

    public ICommand BrowseOutputDirectoryCommand { get; }

    public ExportFolderDialogViewModel()
    {
        BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory);
    }

    private void BrowseOutputDirectory()
    {
        VistaFolderBrowserDialog dialog = new()
        {
            Description = "Select a directory to unpack the archive's folder into"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectoryPath = dialog.SelectedPath;
        }
    }
}
