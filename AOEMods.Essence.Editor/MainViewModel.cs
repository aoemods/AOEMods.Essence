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

public record OpenStreamMessage(Stream Stream, string Extension, string Title);

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
        OpenStream(message.Stream, message.Extension, message.Title);
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
            Filter = "Essence files(*.sga, *.rgd, *.rrtex, *.rrgeom) | *.sga; *.rgd; *.rrtex; *.rrgeom | sga files(*.sga) | *.sga | rgd files(*.rgd) | *.rgd | rrtex files(*.rrtex) | *.rrtex | rrgeom files(*.rrgeom) | *.rrgeom | All files(*.*) | *.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            OpenFile(openFileDialog.FileName);
        }
    }

    private void OpenFile(string filePath)
    {
        OpenStream(File.OpenRead(filePath), Path.GetExtension(filePath), Path.GetFileName(filePath));
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
                Archive = ArchiveReaderHelper.DirectoryToArchive(dialog.SelectedPath, "data"),
                TabTitle = new DirectoryInfo(dialog.SelectedPath).Name,
            });

            SelectedTabIndex = TabItems.Count - 1;
        }
    }

    private void OpenStream(Stream stream, string extension, string title)
    {
        switch (extension)
        {
            case ".rgd":
                AddRgdTab(stream, title);
                break;
            case ".sga":
                AddSgaTab(stream, title);
                break;
            case ".rrtex":
                AddRRTexTab(stream, title);
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

    private void AddRgdTab(Stream stream, string title)
    {
        TabItems.Add(new GameDataViewModel()
        {
            RootNodes = new ObservableCollection<RGDNode>(ReadFormat.RGD(stream)),
            TabTitle = title,
        });
    }

    private void AddSgaTab(Stream stream, string title)
    {
        TabItems.Add(new ArchiveViewModel()
        {
            Archive = (new ArchiveReader(stream)).ReadArchive(),
            TabTitle = title,
        });
    }

    private void AddRRTexTab(Stream stream, string title)
    {
        TabItems.Add(new TextureViewModel()
        {
            ImageFile = ReadFormat.RRTex(stream, PngFormat.Instance).Last(),
            TabTitle = title,
        });
    }
}
