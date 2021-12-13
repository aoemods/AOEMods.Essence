using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RGD;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public class GameDataViewModel : TabItemViewModel
{
    public ObservableCollection<GameDataNodeViewModel>? RootChildren
    {
        get => rootChildren;
        set => SetProperty(ref rootChildren, value);
    }

    private ObservableCollection<GameDataNodeViewModel>? rootChildren = null;

    public ObservableCollection<RGDNode>? RootNodes
    {
        get => rootNodes;
        set => SetProperty(ref rootNodes, value);
    }

    private ObservableCollection<RGDNode>? rootNodes = null;

    public ICommand ExportJsonCommand { get; }
    public ICommand ExportXmlCommand { get; }
    public ICommand ExportRgdCommand { get; }

    public GameDataViewModel()
    {
        ExportJsonCommand = new RelayCommand(ExportJson);
        ExportXmlCommand = new RelayCommand(ExportXml);
        ExportRgdCommand = new RelayCommand(ExportRgd);
    }

    private void ExportJson()
    {
        if (RootNodes != null)
        {
            string gameDataJson = GameDataJsonUtil.GameDataToJson(RootNodes);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"json (*.json)|*.json|All files (*.*)|*.*",
                FileName = "data.json",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, gameDataJson, Encoding.ASCII);
            }
        }
    }

    private void ExportXml()
    {
        if (RootNodes != null)
        {
            string gameDataXml = GameDataXmlUtil.GameDataToXml(RootNodes);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"xml (*.xml)|*.xml|All files (*.*)|*.*",
                FileName = "data.xml",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, gameDataXml, Encoding.ASCII);
            }
        }
    }

    private void ExportRgd()
    {
        if (RootNodes != null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"rgd (*.rgd)|*.rgd|All files (*.*)|*.*",
                FileName = "data.rgd",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var outputFile = File.OpenWrite(saveFileDialog.FileName);
                FormatWriter.WriteRGD(outputFile, RootNodes);
            }
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(RootNodes))
        {
            if (RootNodes != null)
            {
                RootChildren = new ObservableCollection<GameDataNodeViewModel>(RootNodes.Select(node => new GameDataNodeViewModel(node)));
            }
            else
            {
                RootChildren = null;
            }
        }
    }
}
