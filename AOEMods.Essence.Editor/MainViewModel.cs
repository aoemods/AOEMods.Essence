using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public record OpenStreamMessage(Stream Stream, string Extension);

public class MainViewModel : ObservableRecipient, IRecipient<OpenStreamMessage>
{
    public ICommand OpenFileDialogCommand { get; }
    public ICommand OpenDirectoryDialogCommand { get; }

    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set => SetProperty(ref selectedTabIndex, value);
    }

    private int selectedTabIndex = 0;

    public ObservableCollection<object> TabItems
    {
        get;
    } = new();

    public MainViewModel()
    {
        OpenFileDialogCommand = new RelayCommand(OpenFileDialog);
        OpenDirectoryDialogCommand = new RelayCommand(OpenDirectoryDialog);

        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(OpenStreamMessage message)
    {
        OpenStream(message.Stream, message.Extension);
    }

    public void OnDrop(IDataObject droppedObject)
    {
        if (droppedObject.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])droppedObject.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                OpenFile(file);
            }
        }
    }

    private void OpenFileDialog()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Filter = "sga files (*.sga)|*.sga|rgd files (*.rgd)|*.rgd|rrtex files (*.rrtex)|*.rrtex|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            OpenFile(openFileDialog.FileName);
        }
    }

    private void OpenFile(string filePath)
    {
        OpenStream(File.OpenRead(filePath), Path.GetExtension(filePath)); ;
    }

    private void OpenDirectoryDialog()
    {
        VistaFolderBrowserDialog dialog = new()
        {
            Description = "Select a directory to open as SGA archive"
        };

        if (dialog.ShowDialog() == true)
        {
            TabItems.Add(new ArchiveViewModel()
            {
                Archive = ArchiveReaderHelper.DirectoryToArchive(dialog.SelectedPath, "data")
            });

            SelectedTabIndex = TabItems.Count - 1;
        }
    }

    private void OpenStream(Stream stream, string extension)
    {
        switch (extension)
        {
            case ".rgd":
                AddRgdTab(stream);
                break;
            case ".sga":
                AddSgaTab(stream);
                break;
            case ".rrtex":
                AddRRTexTab(stream);
                break;
            default:
                MessageBox.Show(
                    $"Unsupported extension '{extension}'", "Unsupported extension",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
                return;
        }

        SelectedTabIndex = TabItems.Count - 1;
    }

    private void AddRgdTab(Stream stream)
    {
        TabItems.Add(new GameDataViewModel()
        {
            RootNodes = new ObservableCollection<RGDNode>(ReadFormat.RGD(stream))
        });
    }

    private void AddSgaTab(Stream stream)
    {
        TabItems.Add(new ArchiveViewModel()
        {
            Archive = (new ArchiveReader(stream)).ReadArchive()
        });
    }

    private void AddRRTexTab(Stream stream)
    {
        TabItems.Add(new TextureViewModel()
        {
            ImageFile = ReadFormat.RRTex(stream, PngFormat.Instance).First()
        });
    }
}
