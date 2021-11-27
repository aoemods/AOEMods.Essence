using System.Windows.Controls;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for GameDataView.xaml
    /// </summary>
    public partial class GameDataView : UserControl
    {
        public GameDataViewModel ViewModel => (GameDataViewModel)DataContext;

        public GameDataView()
        {
            InitializeComponent();
        }
    }
}
