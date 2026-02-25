using System;
using System.IO;
using System.Windows;

namespace MultiStreamViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BuildGrid(2, 2);
        }

        private void BuildGrid(int rows, int cols)
        {
            GridHost.Rows = rows;
            GridHost.Columns = cols;
            GridHost.Children.Clear();

            for (int i = 0; i < rows * cols; i++)
                GridHost.Children.Add(new StreamTile());
        }

        private void Grid2x2_Click(object sender, RoutedEventArgs e) => BuildGrid(2, 2);
        private void Grid3x3_Click(object sender, RoutedEventArgs e) => BuildGrid(3, 3);

        
        private void LoadDemo_Click(object sender, RoutedEventArgs e)
        {
            var demoDir = Path.Combine(AppContext.BaseDirectory, "DemoVideos");
            var demo1 = Path.Combine(demoDir, "demo1.mp4");
            var demo2 = Path.Combine(demoDir, "demo2.mp4");

            for (int i = 0; i < GridHost.Children.Count; i++)
            {
                if (GridHost.Children[i] is StreamTile tile)
                {
                    var file = (i % 2 == 0) ? demo1 : demo2;
                    tile.SetUrl(file);
                    tile.PlayCurrent();
                }
            }

        }


    }
}