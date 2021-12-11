using AOEMods.Essence.Chunky.Graph;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AOEMods.Essence.Editor;

public class ChunkyNodeViewModel : ObservableRecipient
{
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string? name = null;

    public IChunkyNode? Node
    {
        get => node;
        set => SetProperty(ref node, value);
    }

    private IChunkyNode? node = null;

    public ObservableCollection<ChunkyNodeViewModel>? Children
    {
        get => children;
        set => SetProperty(ref children, value);
    }

    private ObservableCollection<ChunkyNodeViewModel>? children = null;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Node))
        {
            if (Node != null && Node is IChunkyFolderNode folderNode)
            {
                Children = new(folderNode.Children.Select(child => new ChunkyNodeViewModel()
                {
                    Node = child
                }));
            }
            else
            {
                Children = null;
            }

            if (Node != null)
            {
                Name = $"{Node.Header.Type} {Node.Header.Name} ({Node.Header.Path})";
            }
            else
            {
                Name = null;
            }
        }
    }
}
