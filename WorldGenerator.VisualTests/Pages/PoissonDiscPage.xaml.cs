using CPWS.WorldGenerator.PoissonDisc;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace WorldGenerator.VisualTests.Pages
{
    /// <summary>
    /// Interaction logic for VoronoiPage.xaml
    /// </summary>
    public partial class PoissonDiscPage : Page
    {
        List<PageValues> items;

        public PoissonDiscPage()
        {
            InitializeComponent();
            items = new List<PageValues>();
            items.Add(new PageValues<uint>() { Name = "Seed", Value = 4354758 });
            items.Add(new PageValues<double>() { Name = "Radius", Value = 10 });
            VisualElement.ItemSources = items;
        }

        private void VisualElement_GenerateClicked(object sender, EventArgs e)
        {
            Generate();
        }

        private async Task Generate()
        {
            PoissonDiscSampling sampling = new PoissonDiscSampling((double)items.Find(r => r.Name == "Radius").Value, (uint)items.Find(r => r.Name == "Seed").Value);
            sampling.Sample2D(new CP.Common.Maths.Vector3D((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height, 0));

            PoissonDiscSampling sampling2 = new PoissonDiscSampling((double)items.Find(r => r.Name == "Radius").Value, (uint)items.Find(r => r.Name == "Seed").Value);
            sampling2.Sample2D(new CP.Common.Maths.Vector3D((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height, 0));

            System.Drawing.Pen blue = new System.Drawing.Pen(System.Drawing.Color.Blue, 8);
            System.Drawing.Pen red = new System.Drawing.Pen(System.Drawing.Color.Red, 8);
            System.Drawing.Pen blackPoint = new System.Drawing.Pen(System.Drawing.Color.Black, 5);
            System.Drawing.Pen black = new System.Drawing.Pen(System.Drawing.Color.White, 10);
            Random rand = new Random();

            Bitmap src = new Bitmap((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);

            using var graphics = Graphics.FromImage(src);
/*            foreach (var sites in sampling.points)
            {
                graphics.DrawLine(blue, new PointF((float)sites.position.X - 1, (float)sites.position.Y - 1), new PointF((float)sites.position.X + 1, (float)sites.position.Y + 1));
                
            }
            foreach (var sites in sampling2.points)
            {
                graphics.DrawLine(red, new PointF((float)sites.position.X - 1, (float)sites.position.Y - 1), new PointF((float)sites.position.X + 1, (float)sites.position.Y + 1));

            }*/

            VisualElement.Image = Utilities.BitmapToImageSource(src);
        }
    }
}
