using AOEMods.Essence.Chunky.Graph;
using System.IO;
using System.Linq;
using System.Windows;
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

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ChunkyNodeViewModel nodeViewModel)
            {
                if (nodeViewModel.Node is IChunkyDataNode dataNode)
                {
                    ViewModel.DataStream = new MemoryStream(dataNode.GetData().ToArray());
                }
            }

            e.Handled = false;
        }
    }
}
