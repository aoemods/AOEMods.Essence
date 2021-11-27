using System.Windows.Controls;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for TextureView.xaml
    /// </summary>
    public partial class TextureView : UserControl
    {
        public TextureViewModel ViewModel => (TextureViewModel)DataContext;

        public TextureView()
        {
            InitializeComponent();
        }
    }
}
