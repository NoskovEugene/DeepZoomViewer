using DeepZoom.Controls;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
using DeepZoom;
using System.Windows.Controls;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Shapes;

namespace DeepZoom.TestApplication
{
    public partial class MainWindow
    {

        public MultiScaleImage Image;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog() { Filter = "Deep zoom image(*.dzi)|*.dzi" };
            if (op.ShowDialog() == true)
            {
                if (DZViewBox.Child != null)
                    DZViewBox.Child = null;
                if (Image != null)
                    Image = null;
                string path = op.FileName;
                DeepZoomImageTileSource source = new DeepZoomImageTileSource(new System.Uri(path));
                Image = new MultiScaleImage()
                {
                    Source = source
                };
                DZViewBox.Child = Image;
            }
        }

        private void GetMapButton_Click(object sender, RoutedEventArgs e)
        {
            MultiScaleTileSource source = Image.Source;
            int i = 0;
            while (true)
            {
                if (source.TilesAtLevel(i) > 1)
                    break;
                i++;
            }
            VisualTile tile = (VisualTile)Image.GetSource()[i - 1];
            if (tile.Source != null)
            {
                MapImage.ImageSource = tile.Source;
                Ellipse ellipse = new Ellipse();
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                ellipse.Fill = Brushes.White;
                ellipse.Stroke = Brushes.Black;
                ellipse.StrokeThickness = 1.5;
                ellipse.Width = 6;
                ellipse.Height = 6;
                if(MapBox.Children.Count < 1)
                {
                    MapBox.Children.Add(ellipse);
                }

            }
        }

        private void MapBox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MapImage.ImageSource != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(MapBox);
                Ellipse ellipse = (Ellipse)MapBox.Children[0];
                Canvas.SetLeft(ellipse, pos.X);
                Canvas.SetTop(ellipse, pos.Y);
                pos = new Point(pos.X / MapBox.ActualWidth, pos.Y / MapBox.ActualHeight);
                Image.SetPositionToCenter(pos, new Size(DZViewBox.ActualWidth, DZViewBox.ActualHeight));
            }
        }

        private void DZViewBox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Image != null && MapImage.ImageSource != null)
            {
                Point ViewPos = Image.GetCenterPositionOnProcent(new Size(DZViewBox.ActualWidth, DZViewBox.ActualHeight));
                Point PosToMap = new Point(ViewPos.X * MapBox.ActualWidth, ViewPos.Y * MapBox.ActualHeight);
                PosToMap.X = PosToMap.X < 0 ? 0 : PosToMap.X > MapBox.ActualWidth ? MapBox.ActualWidth : PosToMap.X;
                PosToMap.Y = PosToMap.Y < 0 ? 0 : PosToMap.Y > MapBox.ActualHeight ? MapBox.ActualHeight : PosToMap.Y;

                Ellipse ellipse = (Ellipse)MapBox.Children[0];
                Canvas.SetLeft(ellipse, PosToMap.X);
                Canvas.SetTop(ellipse, PosToMap.Y);
            }
        }

        //private void OpenButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{

        //    OpenFileDialog fileDialog = new OpenFileDialog()
        //    {
        //        Filter = "Deep zoom image(*.dzi)|*.dzi"
        //    };
        //    if (fileDialog.ShowDialog() == true)
        //    {
        //        if (panel1.Child != null)
        //            panel1.Child = null;
        //        if (Image != null)
        //            Image = null;
        //        string path = fileDialog.FileName;
        //        DeepZoomImageTileSource source = new DeepZoomImageTileSource(new System.Uri(path));
        //        Image = new MultiScaleImage()
        //        {
        //            Source = source
        //        };
        //        panel1.Child = Image;

        //        MessageBox.Show($"{Image.Source.AbsoluteHeight}, {Image.Source.AbsoluteWidth}");

        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{


        //}


        //private void Image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    mousePosition.Text = $"X = {e.GetPosition(null).X}, Y = {e.GetPosition(null).Y}";
        //    mousePosition.Text += $"\r\nOffset X = {Math.Round( Image.GetZoomable().Offset.X,3)}, Y = {Math.Round( Image.GetZoomable().Offset.Y,3)}";
        //    mousePosition.Text += $"\r\nact W = {Math.Round(Image.GetZoomable().ActualViewbox.Width,3)}, H = {Math.Round(Image.GetZoomable().ActualViewbox.Height,3)}";
        //    mousePosition.Text += $"\r\n{Image.Width}";
        //    Size actsize = Image.GetActualSize();
        //    mousePosition.Text += $"\r\n W = {actsize.Width} H = {actsize.Height}";
        //}

        //private void MapBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    Point OnMapPos = e.GetPosition(MapBox);
        //    Point OnMapProcent = new Point((OnMapPos.X * 100 / MapBox.ActualWidth)/100, (OnMapPos.Y * 100 / MapBox.ActualHeight)/100);
        //    mousePosition.Text = OnMapProcent.ToString();

        //    Image.SetPositionToCenter(OnMapProcent,new Size(panel1.ActualWidth,panel1.ActualHeight));
        //}

    }
}
