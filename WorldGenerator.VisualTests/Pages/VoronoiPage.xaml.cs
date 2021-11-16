using CPWS.WorldGenerator.PoissonDisc;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace WorldGenerator.VisualTests.Pages
{
    /// <summary>
    /// Interaction logic for VoronoiPage.xaml
    /// </summary>
    public partial class VoronoiPage : Page
    {
        List<PageValues> items;

        public VoronoiPage()
        {
            InitializeComponent();
            items = new List<PageValues>();
            items.Add(new PageValues<int>() { Name = "Seed", Value = 4354758 });
            items.Add(new PageValues<int>() { Name = "Point Count", Value = 3 });
            items.Add(new PageValues<int>() { Name = "Noisy Edges", Value = 0 });
            items.Add(new PageValues<bool>() { Name = "Use Delaunay", Value = false });
            items.Add(new PageValues<int>() { Name = "Relaxation", Value = 4 });
            items.Add(new PageValues<bool>() { Name = "Use Random Colours", Value = true });
            items.Add(new PageValues<double>() { Name = "Radius", Value = 100 });
            VisualElement.ItemSources = items;
        }

        private void VisualElement_GenerateClicked(object sender, EventArgs e)
        {
            Generate();
        }

        private async Task Generate()
        {
            BowyerAlgorithm2D voronoiGen = new BowyerAlgorithm2D((int)items.Find(r => r.Name == "Seed").Value)
            {
                PointCount = (int)items.Find(r => r.Name == "Point Count").Value,
                //UseDelaunay = (bool)items.Find(r => r.Name == "Use Delaunay").Value,
                //UseNoisyEdges = (int)items.Find(r => r.Name == "Noisy Edges").Value > 0,
                //NoisyEdgesNo = (int)items.Find(r => r.Name == "Noisy Edges").Value,
                MaxX = 300,
                MaxY = 300,
            };
            int seed = (int)items.Find(r => r.Name == "Seed").Value;
            PoissonDiscSampling sampling = new PoissonDiscSampling((double)items.Find(r => r.Name == "Radius").Value, (uint)seed);
            sampling.Sample2D(new CP.Common.Maths.Vector3D(300, 300, 0));
            // 
            //List<Triangle> triangulation = voronoiGen.Generate(sampling.points.Select(point => point.position).ToList(), true);
            List<VoronoiCell> cells = voronoiGen.GenerateVoronoi();

            //int relaxation = (int)items.Find(r => r.Name == "Relaxation").Value;
            //if(relaxation > 0)
            //voronoiGen.Relax(relaxation);

            System.Drawing.Pen blue = new System.Drawing.Pen(System.Drawing.Color.Blue, 2);
            System.Drawing.Pen red = new System.Drawing.Pen(System.Drawing.Color.Red, 2);
            System.Drawing.Pen blackPoint = new System.Drawing.Pen(System.Drawing.Color.Black, 5);
            System.Drawing.Pen black = new System.Drawing.Pen(System.Drawing.Color.White, 10);
            Random rand = new Random();

            Bitmap src = new Bitmap((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);

            using var graphics = Graphics.FromImage(src);
/*            foreach (var sites in triangulation)
            {
                foreach (var edge in sites.Edges)
                {
                    graphics.DrawLine(blue, new PointF((float)edge.PointA.X + 300, (float)edge.PointA.Y + 300), new PointF((float)edge.PointB.X + 300, (float)edge.PointB.Y + 300));
                }
            }*/
            foreach (var sites in cells)
            {
                foreach (var edge in sites.Edges)
                {
                    graphics.DrawLine(red, new PointF((float)edge.PointA.X + 300, (float)edge.PointA.Y + 300), new PointF((float)edge.PointB.X + 300, (float)edge.PointB.Y + 300));
                }
            }

            VisualElement.Image = Utilities.BitmapToImageSource(src);
        }
    }
}
