using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.Graph;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.SGA.Graph;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public record OpenStreamMessage(Stream Stream, string Extension, string Title);
public class ArchivesRequest : RequestMessage<IEnumerable<IArchive>>
{
}

public record CloseTabMessage(TabItemViewModel TabItemViewModel);

public class MainViewModel : ObservableRecipient, IRecipient<OpenStreamMessage>, IRecipient<ArchivesRequest>, IRecipient<CloseTabMessage>
{
    public ICommand OpenFileDialogCommand { get; }
    public ICommand OpenFilesInDirectoryDialogCommand { get; }
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
        OpenFilesInDirectoryDialogCommand = new RelayCommand(OpenFilesInDirectoryDialog);
        OpenDirectoryDialogCommand = new RelayCommand(OpenDirectoryDialog);

        WeakReferenceMessenger.Default.Register<OpenStreamMessage>(this);
        WeakReferenceMessenger.Default.Register<ArchivesRequest>(this);
        WeakReferenceMessenger.Default.Register<CloseTabMessage>(this);
    }

    public void Receive(ArchivesRequest message)
    {
        message.Reply(TabItems
            .OfType<ArchiveViewModel>()
            .Select(archiveViewModel => archiveViewModel.Archive)
            .Where(archive => archive != null)
            .Cast<IArchive>()
        );
    }

    public void Receive(OpenStreamMessage message)
    {
        OpenStream(message.Stream, message.Extension, message.Title);
    }

    public void Receive(CloseTabMessage message)
    {
        TabItems.Remove(message.TabItemViewModel);
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
            Filter = "Essence files(*.sga, *.rgd, *.rrtex, *.rrgeom, *.rrmaterial) | *.sga; *.rgd; *.rrtex; *.rrgeom; *.rrmaterial; | sga files(*.sga) | *.sga | rgd files(*.rgd) | *.rgd | rrtex files(*.rrtex) | *.rrtex | rrgeom files(*.rrgeom) | *.rrgeom | rrmaterial files(*.rrmaterial) | *.rrmaterial | All files(*.*) | *.*",
            RestoreDirectory = true,
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

    private void OpenFilesInDirectoryDialog()
    {
        VistaFolderBrowserDialog dialog = new()
        {
            Description = "Select the directory of which to open all SGA archives"
        };

        if (dialog.ShowDialog() == true)
        {
            Matcher matcher = new();
            matcher.AddInclude("**/*.sga");
            var sgaPaths = matcher.GetResultsInFullPath(dialog.SelectedPath).ToArray();

            foreach (var sgaPath in sgaPaths)
            {
                OpenFile(sgaPath);
            }

            SelectedTabIndex = TabItems.Count - 1;
        }
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
            case ".rrgeom":
                AddRRGeomTab(stream, title);
                break;
            case ".rrmaterial":
            case "chunky":
                AddChunkyTab(stream, title);
                break;
            case ".rec":
                stream.Position = 0x90;
                AddChunkyTab(stream, title);
                Trace.WriteLine($"Remaining data: {stream.Length - stream.Position:X} at {stream.Position:X}");
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
            RootNodes = new ObservableCollection<RGDNode>(FormatReader.ReadRGD(stream)),
            TabTitle = title,
        });
    }

    private void AddSgaTab(Stream stream, string title)
    {
        TabItems.Add(new ArchiveViewModel()
        {
            Archive = Archive.FromStream(stream),
            TabTitle = title,
        });
    }

    private void AddRRTexTab(Stream stream, string title)
    {
        TabItems.Add(new TextureViewModel()
        {
            ImageFile = FormatReader.ReadRRTexLastMip(stream, PngFormat.Instance),
            TabTitle = title,
        });
    }
    private void AddRRGeomTab(Stream stream, string title)
    {
        TabItems.Add(new GeometryObjectViewModel()
        {
            GeometryObject = FormatReader.ReadRRGeom(stream).First(),
            TabTitle = title,
        });
    }

    private void AddChunkyTab(Stream stream, string title)
    {
        TabItems.Add(new ChunkyViewModel()
        {
            ChunkyFile = ChunkyFile.FromStream(stream),
            TabTitle = title,
        });
    }
}
