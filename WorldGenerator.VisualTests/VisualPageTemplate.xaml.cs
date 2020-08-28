using System;
using System.Collections;
using System.Collections.Generic;
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

namespace WorldGenerator.VisualTests
{
    /// <summary>
    /// Interaction logic for VisualPageTemplate.xaml
    /// </summary>
    public partial class VisualPageTemplate : UserControl
    {
        public event EventHandler GenerateClicked;

        public IEnumerable ItemSources { get => PageInfo.ItemsSource; set => PageInfo.ItemsSource = value; }

        public ImageSource Image { get => ResultImage.Source; set => ResultImage.Source = value; }

        public VisualPageTemplate()
        {
            InitializeComponent();

        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            GenerateClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
