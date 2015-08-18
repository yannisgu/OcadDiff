using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using OcadParser.Renderer;
using Svg;
using Svg.Transforms;
using Color = System.Drawing.Color;

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
            DataContext = new DiffViewModel {ShowMaxWarning = Visibility.Hidden};
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
                Status.Text = "Loading... This will take a moment.";
                var diffGenerator = new OcadDiffGenerator(SourceFile.Text, TargetFile.Text);
                Task.Run(() =>
                {
                    var diff = diffGenerator.GetDiff();
                    var sourceRenderer = new OcadRenderer(diff.Source);
                    var targetRenderer = new OcadRenderer(diff.Target);
                    sourceRenderer.EnsureSvg();
                    targetRenderer.EnsureSvg();

                    var viewModel = new DiffViewModel();

                    var limit = 200;

                    foreach (var obj in diff.DeletedObjects.Take(limit))
                    {
                        var minX = obj.Poly.Min(p => p.X.Coordinate) - 1000;
                        var minY = -obj.Poly.Max(p => p.Y.Coordinate) - 1000;
                        var maxX = obj.Poly.Max(p => p.X.Coordinate) + 1000;
                        var maxY = -obj.Poly.Min(p => p.Y.Coordinate) + 1000;
                        var addedElements = new List<SvgElement>();
                        foreach (var element in sourceRenderer.ObjectElementMapping[obj])
                        {
                            var highlightElement = element.DeepCopy();
                            highlightElement.StrokeWidth = highlightElement.StrokeWidth + 100;
                            highlightElement.Stroke = new SvgColourServer(Color.Red);
                            highlightElement.StrokeOpacity= (float)0.5;
                            var index = sourceRenderer.Svg.Children.IndexOf(element);
                            sourceRenderer.Svg.Children.Insert(index, highlightElement);
                            addedElements.Add(highlightElement);
                        }

                        viewModel.Diffs.Add(new DiffViewModelItems()
                        {
                            LeftBitmap = sourceRenderer.GetBitmap(minX, minY, maxX, maxY, 300),
                            RightBitmap = targetRenderer.GetBitmap(minX, minY, maxX, maxY, 300),
                            Status = "Removed"
                        });

                        addedElements.ForEach(_ => sourceRenderer.Svg.Children.Remove(_));
                    }

                    foreach (var obj in diff.AddedObjects.Take(limit - diff.DeletedObjects.Count))
                    {
                        var minX = obj.Poly.Min(p => p.X.Coordinate) - 1000;
                        var minY = -obj.Poly.Max(p => p.Y.Coordinate) - 1000;
                        var maxX = obj.Poly.Max(p => p.X.Coordinate) + 1000;
                        var maxY = -obj.Poly.Min(p => p.Y.Coordinate) + 1000;


                        var addedElements = new List<SvgElement>();
                        foreach (var element in targetRenderer.ObjectElementMapping[obj])
                        {
                            var highlightElement = element.DeepCopy();
                            highlightElement.StrokeWidth = highlightElement.StrokeWidth + 100;
                            highlightElement.Stroke = new SvgColourServer(Color.Green);
                            highlightElement.StrokeOpacity = (float)0.5;
                            highlightElement.Fill = SvgPaintServer.None;
                            var index = targetRenderer.Svg.Children.IndexOf(element);
                            targetRenderer.Svg.Children.Insert(index, highlightElement);
                            addedElements.Add(highlightElement);
                        }
                        viewModel.Diffs.Add(new DiffViewModelItems()
                        {
                            LeftBitmap = sourceRenderer.GetBitmap(minX, minY, maxX, maxY, 300),
                            RightBitmap = targetRenderer.GetBitmap(minX, minY, maxX, maxY, 300),
                            Status = "Added"
                        });

                        addedElements.ForEach(_ => targetRenderer.Svg.Children.Remove(_));
                    }

                    viewModel.ShowMaxWarning =
                        limit < diff.AddedObjects.Count + diff.DeletedObjects.Count
                            ? Visibility.Visible
                            : Visibility.Hidden;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.DataContext = viewModel;
                        Status.Text = "";
                    }
                );
                });

            }
        }

        private void BrowseTargetFile_OnClick(object sender, RoutedEventArgs e)
        {
            SelectFile(TargetFile);
        }
    }

    public class DiffViewModel
    {
        public List<DiffViewModelItems> Diffs { get; } = new List<DiffViewModelItems>();
        public Visibility ShowMaxWarning { get; set; }
    }

    public class DiffViewModelItems
    {
        public string Status { get; set; }

        public Bitmap LeftBitmap { get; set; }

        public BitmapImage LeftBitmapImage
        {
            get {
                return GetBitmapImage(LeftBitmap);
            }
        }

        private BitmapImage GetBitmapImage(Bitmap bitmap)
        {
            Bitmap blank = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics g = Graphics.FromImage(blank);
            g.Clear(Color.White);
            g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);

            Bitmap tempImage = new Bitmap(blank);
            blank.Dispose();
            

            var ms = new MemoryStream();
            tempImage.Save(ms, ImageFormat.Bmp);

            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            //Return the image
            tempImage.Dispose();
            return image;



        }

        public Bitmap RightBitmap { get; set; }


        public BitmapImage RightBitmapImage
        {
            get { return GetBitmapImage(RightBitmap); }
        }
    }
}
