using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OcadDiff.Logic;

namespace OcadDiff.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSourceFile_OnClick(object sender, RoutedEventArgs e)
        {
            SelectFile(SourceFile);
        }

        private void SelectFile(TextBox textBox)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".ocd";
            dlg.Filter = "Ocad documents (.ocd)|*.ocd";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                textBox.Text = filename;
            }

            UpdateDiff();
        }

        private void UpdateDiff()
        {
            if (!string.IsNullOrEmpty(SourceFile.Text) && !string.IsNullOrEmpty(TargetFile.Text))
            {
                var diff = new OcadDiffGenerator(SourceFile.Text, TargetFile.Text).GetDiff();

                TargetResult.Text = diff.Report.DifferencesString;
            }
        }

        private void BrowseTargetFile_OnClick(object sender, RoutedEventArgs e)
        {
            SelectFile(TargetFile);
        }
    }
}
