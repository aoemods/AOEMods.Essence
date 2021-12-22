using AOEMods.Essence.Chunky.RGD;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AOEMods.Essence.Editor;

public class GameDataNodeViewModel : TreeViewTabItemViewModel
{
    private static IReadOnlyDictionary<Type, string> TypeName = new Dictionary<Type, string>()
    {
        [typeof(int)] = "Integer",
        [typeof(float)] = "Float",
        [typeof(string)] = "String",
        [typeof(bool)] = "Boolean",
        [typeof(RGDNode[])] = "List",
    };

    public string? Key
    {
        get => key;
        set => SetProperty(ref key, value);
    }

    private string? key = null;

    public object? Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    private object? value = null;

    public string? DisplayValue
    {
        get => Value == null ? null : string.Format("{0}: {1} ({2})", Key, Value is IList<RGDNode> ? $"{{{Children?.Count}}}" : Value, TypeName[Value.GetType()]);
    }

    public RGDNode? Node
    {
        get => node;
        set => SetProperty(ref node, value);
    }

    private RGDNode? node = null;

    public GameDataNodeViewModel(RGDNode? node) : base()
    {
        Node = node;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Node))
        {
            if (Node != null)
            {
                Key = Node.Key;
                Value = Node.Value;
                if (Value is IList<RGDNode> children)
                {
                    Children = new ObservableCollection<TreeViewTabItemViewModel>(children.Select(child => new GameDataNodeViewModel(child)));
                }
                else
                {
                    Children = null;
                }
            }
            else
            {
                Key = null;
                Value = null;
                Children = null;
            }
        }
        else if (e.PropertyName == nameof(Value) || e.PropertyName == nameof(Key) || e.PropertyName == nameof(Children))
        {
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public override string GetSearchTarget()
    {
        return DisplayValue?.ToLowerInvariant().Trim() ?? key?.ToLowerInvariant().Trim() ?? string.Empty;
    }
}
