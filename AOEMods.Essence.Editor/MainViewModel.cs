using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace AOEMods.Essence.Editor;

public record OpenStreamMessage(Stream Stream, string Extension);

public class MainViewModel : ObservableRecipient, IRecipient<OpenStreamMessage>
{
    public RelayCommand OpenArchiveCommand { get; }

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
        OpenArchiveCommand = new RelayCommand(OpenArchive);

        WeakReferenceMessenger.Default.Register<OpenStreamMessage>(this);
    }

    public void OpenFile(string path)
    {
        OpenStream(File.OpenRead(path), Path.GetExtension(path));
    }

    public void OpenStream(Stream stream, string extension)
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

    public void AddRgdTab(Stream stream)
    {
        TabItems.Add(new GameDataViewModel()
        {
            RootNodes = new ObservableCollection<RGDNode>(ReadFormat.RGD(stream))
        });
    }

    public void AddSgaTab(Stream stream)
    {
        TabItems.Add(new ArchiveViewModel()
        {
            Archive = (new ArchiveReader(stream)).ReadArchive()
        });
    }

    public void AddRRTexTab(Stream stream)
    {
        TabItems.Add(new TextureViewModel()
        {
            ImageFile = ReadFormat.RRTex(stream, PngFormat.Instance).First()
        });
    }

    private void OpenArchive()
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

    public void Receive(OpenStreamMessage message)
    {
        OpenStream(message.Stream, message.Extension);
    }
}
