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
using WorldGenerator.VisualTests.Pages;

namespace WorldGenerator.VisualTests
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

        private void HeatmapPage_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "Heatmap",
                Name = "Heatmap",
                Content = new Frame
                {
                    Content = new HeatmapPage()
                }
            };

            TabController.Items.Add(newTabItem);
        }

        private void SimplexNoisePage_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "SimplexNoise",
                Name = "SimplexNoise",
                Content = new Frame
                {
                    Content = new SimplexNoisePage()
                }
            };

            TabController.Items.Add(newTabItem);
        }

        private void VoronoiPage_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "VoronoiDiagram",
                Name = "VoronoiDiagram",
                Content = new Frame
                {
                    Content = new VoronoiPage()
                }
            };

            TabController.Items.Add(newTabItem);
        }
    }
}
