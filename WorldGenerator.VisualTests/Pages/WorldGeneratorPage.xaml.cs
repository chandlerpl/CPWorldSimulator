using CPWS.WorldGenerator.Generators;
using CPWS.WorldGenerator.Generators.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorldGenerator.VisualTests.Pages
{

    public partial class WorldGeneratorPage : Page
    {
        List<PageValues> items;

        public WorldGeneratorPage()
        {
            InitializeComponent();
            items = new List<PageValues>();
            items.Add(new PageValues<uint>() { Name = "Seed", Value = 4354758 });
            VisualElement.ItemSources = items;
        }

        private void VisualElement_GenerateClicked(object sender, RoutedEventArgs e)
        {
            Bitmap src = new Bitmap((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);

            GridWorldGenerator grid = new GridWorldGenerator((uint)items.Find(r => r.Name == "Seed").Value, NoiseType.SIMPLEX, (int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);
            grid.Generate();

            GridData[,] data = grid.Data.Data;

            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    int val = (int)(data[y, x].height * 255);
                    src.SetPixel(x, y, System.Drawing.Color.FromArgb(val, val, val));
                }
            }

            System.Drawing.Pen black = new System.Drawing.Pen(System.Drawing.Color.Black, 2);
            using (var graphics = Graphics.FromImage(src))
            {
                
            }

            black.Dispose();
            VisualElement.ResultImage.Source = Utilities.BitmapToImageSource(src);
        }
    }
}
