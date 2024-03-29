﻿using System.Windows;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for ExportFileDialog.xaml
    /// </summary>
    public partial class ExportFileDialog : Window
    {
        public ExportFileDialogViewModel ViewModel => (ExportFileDialogViewModel)DataContext;

        public ExportFileDialog(object? exportOptionsViewModel)
        {
            DataContext = new ExportFileDialogViewModel()
            {
                ExportOptionsViewModel = exportOptionsViewModel
            };

            InitializeComponent();
        }

        private void OnExportClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
