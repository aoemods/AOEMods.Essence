using Microsoft.Toolkit.Mvvm.Messaging;
using System.IO;
using System.Windows;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            foreach (var arg in e.Args)
            {
                WeakReferenceMessenger.Default.Send(new OpenStreamMessage(File.OpenRead(arg), Path.GetExtension(arg)));
            }
            mainWindow.Show();
        }
    }
}
