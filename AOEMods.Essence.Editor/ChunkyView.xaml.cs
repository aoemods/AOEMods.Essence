using System.Windows.Controls;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for ChunkyViewView.xaml
    /// </summary>
    public partial class ChunkyView : UserControl
    {
        public ChunkyViewModel ViewModel => (ChunkyViewModel)DataContext;

        public ChunkyView()
        {
            InitializeComponent();
        }
    }
}
