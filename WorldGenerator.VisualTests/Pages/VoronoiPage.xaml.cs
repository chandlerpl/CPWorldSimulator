using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points;
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
    public partial class VoronoiPage : Page
    {
        List<PageValues> items;

        public VoronoiPage()
        {
            InitializeComponent();
            items = new List<PageValues>();
            items.Add(new PageValues<int>() { Name = "Seed", Value = 4354758 });
            items.Add(new PageValues<int>() { Name = "Point Count", Value = 250 });
            items.Add(new PageValues<int>() { Name = "Noisy Edges", Value = 0 });
            items.Add(new PageValues<bool>() { Name = "Use Delaunay", Value = false });
            items.Add(new PageValues<int>() { Name = "Relaxation", Value = 4 });
            items.Add(new PageValues<bool>() { Name = "Use Random Colours", Value = true });
            VisualElement.ItemSources = items;
        }

        private void VisualElement_GenerateClicked(object sender, EventArgs e)
        {
            Generate();
        }

        private async Task Generate()
        {
            FortunesAlgorithm voronoiGen = new FortunesAlgorithm()
            {
                Seed = (int)items.Find(r => r.Name == "Seed").Value,
                PointCount = (int)items.Find(r => r.Name == "Point Count").Value,
                UseDelaunay = (bool)items.Find(r => r.Name == "Use Delaunay").Value,
                UseNoisyEdges = (int)items.Find(r => r.Name == "Noisy Edges").Value > 0,
                NoisyEdgesNo = (int)items.Find(r => r.Name == "Noisy Edges").Value,
                MaxX = VisualElement.ResultImage.Width,
                MaxY = VisualElement.ResultImage.Height
            };
            voronoiGen.Generate();

            int relaxation = (int)items.Find(r => r.Name == "Relaxation").Value;
            if(relaxation > 0)
                voronoiGen.Relax(relaxation);

            System.Drawing.Pen blue = new System.Drawing.Pen(System.Drawing.Color.Blue, 8);
            System.Drawing.Pen blackPoint = new System.Drawing.Pen(System.Drawing.Color.Black, 5);
            System.Drawing.Pen black = new System.Drawing.Pen(System.Drawing.Color.Black, 2);
            Random rand = new Random();

            Bitmap src = new Bitmap((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);

            using var graphics = Graphics.FromImage(src);
            foreach (var sites in voronoiGen.Sites)
            {
                PointF[] po = new PointF[sites.Points.Count];

                SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(rand.Next(10, 255), rand.Next(10, 255), rand.Next(10, 255)));
                for (int i = 0; i < sites.Points.Count; i++)
                {
                    if (sites.Points[i] is NoisyPoint point)
                    {
                        po[i] = new PointF((float)(sites.Points[i].X + point.XNoise), (float)(sites.Points[i].Y + point.YNoise));
                    }
                    else
                    {
                        po[i] = new PointF((float)sites.Points[i].X, (float)sites.Points[i].Y);
                        if (i + 1 < sites.Points.Count)
                        {
                            graphics.DrawLine(blue, new PointF((float)sites.Points[i].X, (float)sites.Points[i].Y), new PointF((float)sites.Points[i + 1].X, (float)sites.Points[i + 1].Y));
                        }
                    }
                }

                if((bool)items.Find(r => r.Name == "Use Random Colours").Value)
                    graphics.FillPolygon(brush, po);

                graphics.DrawLine(blackPoint, (float)sites.X, (float)sites.Y, (float)sites.X + 2f, (float)sites.Y + 2f);
            }

            if((bool)items.Find(r => r.Name == "Use Delaunay").Value)
            {
                foreach (var sites in voronoiGen.Delaunay)
                {
                    PointF po = new PointF((float)sites.Item1.X, (float)sites.Item1.Y);
                    PointF po1 = new PointF((float)sites.Item2.X, (float)sites.Item2.Y);

                    graphics.DrawLine(black, po, po1);
                }
            }

            VisualElement.Image = Utilities.BitmapToImageSource(src);
        }
    }
}
