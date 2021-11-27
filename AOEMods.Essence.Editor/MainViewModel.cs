using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using AOEMods.Essence.SGA;

namespace AOEMods.Essence.Editor;

public class MainViewModel : ObservableRecipient
{
    public RelayCommand OpenArchiveCommand { get; }

    public ObservableCollection<object> TabItems
    {
        get;
    } = new();

    public MainViewModel()
    {
        OpenArchiveCommand = new RelayCommand(OpenArchive);
    }

    public void OpenFile(string path)
    {
        TabItems.Add(Path.GetExtension(path) switch
        {
            ".rgd" => new GameDataViewModel()
            {
                RootNodes = new ObservableCollection<RGDNode>(ReadFormat.RGD(path))
            },
            ".sga" => new ArchiveViewModel()
            {
                Archive = (new ArchiveReader(File.OpenRead(path))).ReadArchive()
            },
            ".rrtex" => new TextureViewModel()
            {
                ImageFile = ReadFormat.RRTex(File.OpenRead(path), PngFormat.Instance).First()
            },
            _ => throw new NotSupportedException($"Unsupported extension ${path}")
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
}
