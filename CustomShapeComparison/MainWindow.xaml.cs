using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CustomShapeComparison;
using StrokeSimiliarity;

namespace CombinedGestures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly IStrokeComparer comparer;

        private IEnumerable<Point> square;
        private IEnumerable<Point> star;
        private IEnumerable<Point> triangle;

        public MainWindow()
        {
            InitializeComponent();

            comparer = new PointSimilarityComparer();
            
            CreateShapes();
        }

        private void CreateShapes()
        {
            square = new[] { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(0, 0) };
            //star = new[] { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(0, 0) };
            triangle = new[] { new Point(0, 0), new Point(1, 0), new Point(0.5, 1), new Point(0, 0)};
        }

        public void CreateTextBlock(string message, double posX, double posY)
        {
            var textBox = new TextBlock
            {
                Background = Brushes.Beige,
                Foreground = Brushes.Black,
                Text = message
            };
            DrawingCanvas.Children.Add(SetElementTopLeftPositionOnInkCanvas(textBox, posX, posY));
        }

        private void DrawingCanvas_OnStylusInAirMove(object sender, StylusEventArgs e)
        {
            
        }

        private void DrawingCanvas_OnStylusMove(object sender, StylusEventArgs e)
        {
            
        }

        private T SetElementCenterPositionOnInkCanvas<T>(T element,double x, double y)
            where T : FrameworkElement
        {
            InkCanvas.SetTop(element, y - (element.Height/2.0));
            InkCanvas.SetLeft(element, x - (element.Width/2.0));
            return element;
        }
        
        private T SetElementTopLeftPositionOnInkCanvas<T>(T element,double x, double y)
            where T : FrameworkElement
        {
            InkCanvas.SetTop(element, y);
            InkCanvas.SetLeft(element, x);
            return element;
        }

        private void DrawingCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var score = comparer.Compare(scaledSquare,e.Stroke.StylusPoints.Select(sp => new Point(sp.X, sp.Y)));
            
            CreateTextBlock($"Score: {score.score}", e.Stroke.StylusPoints.Last().X,e.Stroke.StylusPoints.Last().Y);

            foreach (var (p1,p2) in score.sampled.Zip(score.sampled.Skip(1), (p1,p2) => (p1,p2)))
            {
                var l = new Line
                {
                    X1 = p1.Item1.X + e.Stroke.StylusPoints.First().X,
                    X2 = p2.Item1.X + e.Stroke.StylusPoints.First().X,
                    Y1 = p1.Item1.Y + e.Stroke.StylusPoints.First().Y,
                    Y2 = p2.Item1.Y + e.Stroke.StylusPoints.First().Y,
                    Stroke = p1.distance switch
                    {
                        <= 5 => Brushes.Green,
                        <= 25 => Brushes.Orange,
                        <= 50 => Brushes.Yellow,
                        _ => Brushes.Red
                    }
                };

                DrawingCanvas.Children.Add(l);
            }
            DrawingCanvas.Strokes.Remove(e.Stroke);
        }

        private Point[] scaledSquare;
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            scaledSquare = square.Select(p => new Point(p.X * 200, p.Y * 200)).ToArray();
            var pl = new Polyline
            {
                Points = new PointCollection(scaledSquare),
                Stroke = Brushes.LightGray,
                StrokeThickness = 5.0,
                Fill = Brushes.Transparent,
                FillRule = FillRule.Nonzero
            };

            pl = SetElementTopLeftPositionOnInkCanvas(pl, DrawingCanvas.ActualWidth / 2.0, DrawingCanvas.ActualHeight / 2.0);
            DrawingCanvas.Children.Add(pl);
        }
    }
}