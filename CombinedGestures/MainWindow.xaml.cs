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

namespace CombinedGestures
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

        private void DrawingCanvas_OnStylusInAirMove(object sender, StylusEventArgs e)
        {
            
        }

        private void DrawingCanvas_OnStylusMove(object sender, StylusEventArgs e)
        {
            
        }

        private void DrawingCanvas_OnGesture(object sender, InkCanvasGestureEventArgs e)
        {
            var strongestGesture = e.GetGestureRecognitionResults().FirstOrDefault(grr => grr.RecognitionConfidence == RecognitionConfidence.Strong);

            if (strongestGesture is null || strongestGesture.ApplicationGesture == ApplicationGesture.NoGesture)
            {
                return;
            }

            var allPoints = e.Strokes.SelectMany(stroke => stroke.StylusPoints).ToArray();
            var allX = allPoints.Select(point => point.X).ToArray();
            var allY = allPoints.Select(point => point.Y).ToArray();
            var width = allX.Max() - allX.Min();
            var height = allY.Max() - allY.Min();
            var center = new Point(allX.Min() + width / 2.0,allY.Min() + height / 2.0);
            var shapeToDraw = GestureToShape.DrawGesture(strongestGesture.ApplicationGesture,center,width,height);
            if (shapeToDraw is null)
            {
                var redStrokes = e.Strokes.Select(stroke =>
                {
                    stroke.DrawingAttributes.Color = Colors.Red;
                    return stroke;
                });
                DrawingCanvas.Strokes.Add(new StrokeCollection(redStrokes));
            }
            else
            {
                var circle = new Ellipse();
                circle.Width = 5;
                circle.Height = 5;
                circle.Fill = Brushes.Blue;
                circle = SetElementCenterPositionOnInkCanvas(circle, center.X,
                    center.Y);
                DrawingCanvas.Children.Add(circle);

                DrawingCanvas.Children.Add(shapeToDraw);
            }
            var textBox = new TextBlock()
            {
                Background = Brushes.Beige,
                Foreground = Brushes.Black,
                Text = strongestGesture.ApplicationGesture.ToString()
            };
            DrawingCanvas.Children.Add(SetElementTopLeftPositionOnInkCanvas(textBox, allX.Max(), allY.Max()));
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
    }
}