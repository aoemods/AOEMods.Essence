using System.Windows;

namespace AOEMods.Essence.Editor;

public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public BooleanToVisibilityConverter()
        : base(Visibility.Visible, Visibility.Collapsed)
    {
    }
}
