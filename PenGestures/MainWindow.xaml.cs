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
using MathNet.Numerics;

namespace PenGestures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Ellipse hoverMarker = new() {Visibility = Visibility.Hidden};
        private void UIElement_OnStylusInAirMove(object sender, StylusEventArgs e)
        {
            hoverMarker.Visibility = Visibility.Visible;
            hoverMarker.Fill = Brushes.Transparent;
            hoverMarker.Stroke = Brushes.Blue;
            hoverMarker.StrokeThickness = 1.0;
            hoverMarker.Height = 25;
            hoverMarker.Width = 25;
            InkCanvas.SetLeft(hoverMarker,e.GetPosition(DrawingCanvas).X - (hoverMarker.Width/2.0));
            InkCanvas.SetTop(hoverMarker,e.GetPosition(DrawingCanvas).Y - (hoverMarker.Height/2.0));

            var device = e.StylusDevice.TabletDevice;
        }

        private void UIElement_OnStylusMove(object sender, StylusEventArgs e)
        {
            
        }

        private void DrawingCanvas_OnLostStylusCapture(object sender, StylusEventArgs e)
        {
            hoverMarker.Visibility = Visibility.Hidden;
        }

        private void DrawingCanvas_OnGotStylusCapture(object sender, StylusEventArgs e)
        {
            hoverMarker.Visibility = Visibility.Hidden;

            if (!DrawingCanvas.Children.Contains(hoverMarker))
            {
                DrawingCanvas.Children.Add(hoverMarker);
            }
        }

        private void PauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var x = 5;
        }

        private void ToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.EditingMode == InkCanvasEditingMode.GestureOnly)
            {
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
            }
            else
            {
                DrawingCanvas.EditingMode = InkCanvasEditingMode.GestureOnly;
            }
        }

        private void ColourRandomise_OnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            foreach (var stroke in DrawingCanvas.Strokes)
            {
                var col = new byte[3];
                r.NextBytes(col);
                stroke.DrawingAttributes.Color = Color.FromRgb(col[0],col[1],col[2]);
            }
        }

        private void Straighten_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var stroke in DrawingCanvas.Strokes)
            {
                var xs = stroke.StylusPoints.Select(p => p.X).ToArray();
                var ys = stroke.StylusPoints.Select(p => p.Y).ToArray();

                var lineFunc = Fit.LineFunc(xs, ys);

                var rSquaredValue = GoodnessOfFit.RSquared(stroke.StylusPoints.Select(p => lineFunc(p.X)),
                    stroke.StylusPoints.Select(p => p.Y));
                
                var bounding = stroke.GetBounds();
                var l = new Line
                {
                    X1 = bounding.Left,
                    X2 = bounding.Right
                };

                var col = Color.Add(Color.Multiply(Colors.Red, (float)(1.0 - rSquaredValue)),
                    Color.Multiply(Colors.Green, (float)(rSquaredValue)));
                l.Y1 = lineFunc(l.X1);
                l.Y2 = lineFunc(l.X2);
                l.Stroke = new SolidColorBrush(col);
                l.StrokeThickness = 5.0;
                l.Clip = new RectangleGeometry(bounding);
                DrawingCanvas.Children.Add(l);
            }
            DrawingCanvas.Strokes.Clear();
        }

        private void DrawingCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var stroke = e.Stroke;
            var xs = stroke.StylusPoints.Select(p => p.X).ToArray();
            var ys = stroke.StylusPoints.Select(p => p.Y).ToArray();

            var startPoint = stroke.StylusPoints.First();
            var endPoint = stroke.StylusPoints.Last();

            var gradient = Math.Abs(endPoint.Y - startPoint.Y) / Math.Abs(endPoint.X - startPoint.X);
            var intercept = endPoint.Y - gradient * endPoint.X;
            
            Func<double,double> lineFunc = (x) => gradient * x + intercept;

            var rSquaredValue = GoodnessOfFit.RSquared(stroke.StylusPoints.Select(p => lineFunc(p.X)),
                stroke.StylusPoints.Select(p => p.Y));
            
            var bounding = stroke.GetBounds();
            var l = new Line
            {
                X1 = startPoint.X,
                X2 = endPoint.X,
                Y1 = startPoint.Y,
                Y2 = endPoint.Y
            };

            var red = rSquaredValue <= 0.5 ? 1.0 : Math.Clamp(rSquaredValue * -2.0 + 2.0,0.0,1.0);
            var green = rSquaredValue >= 0.5 ? 1.0 : Math.Clamp(rSquaredValue * 2.0,0.0,1.0);
            var col = Color.FromRgb((byte)(red * 255), (byte)(green * 255), 0);
            l.Stroke = new SolidColorBrush(col);
            l.StrokeThickness = 5.0;
            l.Clip = new RectangleGeometry(bounding);
            DrawingCanvas.Children.Add(l);

            var t = new TextBlock { Text = rSquaredValue.ToString("F3"), Foreground = l.Stroke };
            InkCanvas.SetLeft(t,l.X2);
            InkCanvas.SetTop(t,l.Y2);
            DrawingCanvas.Children.Add(t);
        }

        private void DrawingCanvas_OnGesture(object sender, InkCanvasGestureEventArgs e)
        {
            var possibleGestures = e.GetGestureRecognitionResults().OrderBy(grr => grr.RecognitionConfidence);

            var strongGestures = possibleGestures.Where(grr => grr.RecognitionConfidence == RecognitionConfidence.Strong);
            
            if (strongGestures.FirstOrDefault(grr => grr.ApplicationGesture == ApplicationGesture.Star) is not null)
            {
                DrawingCanvas.Strokes.Add(e.Strokes);
                ColourRandomise_OnClick(null,null);
            }
        }
    }
}