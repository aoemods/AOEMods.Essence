using AOEMods.Essence.Chunky.RGD;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public override string TabTitle => "RGD";

    public ICommand ExportCommand { get; }

    public GameDataViewModel()
    {
        ExportCommand = new RelayCommand(Export);
    }

    private void Export()
    {
        if (RootNodes != null)
        {
            string gameDataJson = GameDataJsonUtil.GameDataToJson(RootNodes);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"json (*.json)|*.json|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, gameDataJson, Encoding.ASCII);
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
