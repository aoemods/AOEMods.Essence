using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AOEMods.Essence.Editor
{
    public abstract class TabItemViewModel : ObservableRecipient
    {
        public abstract string TabTitle
        {
            get;
        }
    }
}
