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
        private readonly CombinedGestureResolver resolver;
        private CombinedGestureContainer knownGestures = new ();
        public MainWindow()
        {
            InitializeComponent();

            var CircleStar = new CombinedGestureDefinition(new[]
                { (ApplicationGesture.Circle, new Point()), (ApplicationGesture.Star, new Point()) });
            var StarCircle = new CombinedGestureDefinition(new[]
                { (ApplicationGesture.Star, new Point()), (ApplicationGesture.Circle, new Point()) });

            var succ = knownGestures.TryAddGesture("Circle Star",CircleStar,() => {CreateTextBlock("Circle Star", 0 , 0);});
            if (!succ)
            {
                CreateTextBlock("Failure", 0 , 0);
            }
            var succ2 = knownGestures.TryAddGesture("Star Circle",StarCircle,() => {CreateTextBlock("Star Circle", 0 , 0);});
            if (!succ2)
            {
                CreateTextBlock("Failure", 0 , 0);
            }

            resolver = new CombinedGestureResolver(knownGestures);
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

        private void DrawingCanvas_OnGesture(object sender, InkCanvasGestureEventArgs e)
        {
            var strongestGesture = e.GetGestureRecognitionResults().FirstOrDefault(grr => grr.RecognitionConfidence == RecognitionConfidence.Strong);

            if (strongestGesture is null || strongestGesture.ApplicationGesture == ApplicationGesture.NoGesture)
            {
                return;
            }

            var drawer = new GestureToShape();

            var allPoints = e.Strokes.SelectMany(stroke => stroke.StylusPoints).ToArray();
            var allX = allPoints.Select(point => point.X).ToArray();
            var allY = allPoints.Select(point => point.Y).ToArray();
            var width = allX.Max() - allX.Min();
            var height = allY.Max() - allY.Min();
            var center = new Point(allX.Min() + width / 2.0,allY.Min() + height / 2.0);

            if (resolver.RecognitionComplete)
            {
                resolver.Reset();
                var toRemove = new List<UIElement>();
                foreach (UIElement child in DrawingCanvas.Children)
                {
                    if (child is not FrameworkElement fe)
                    {
                        continue;
                    }

                    if (fe.Tag is not string s)
                    {
                        continue;
                    }

                    if (s == "temp")
                    {
                        toRemove.Add(child);
                    }
                }

                foreach (var removable in toRemove)
                {
                    DrawingCanvas.Children.Remove(removable);
                }
            }
            
            if (resolver.Step == 0)
            {
                resolver.CombinedGestureCenter = center;
            }
            // var shapeToDraw = GestureToShape.DrawGesture(strongestGesture.ApplicationGesture,center,width,height);
            // if (shapeToDraw is null)
            // {
            //     var redStrokes = e.Strokes.Select(stroke =>
            //     {
            //         stroke.DrawingAttributes.Color = Colors.Red;
            //         return stroke;
            //     });
            //     DrawingCanvas.Strokes.Add(new StrokeCollection(redStrokes));
            // }
            // else
            // {
            //     var circle = new Ellipse();
            //     circle.Width = 5;
            //     circle.Height = 5;
            //     circle.Fill = Brushes.Blue;
            //     circle = SetElementCenterPositionOnInkCanvas(circle, center.X,
            //         center.Y);
            //     DrawingCanvas.Children.Add(circle);
            //
            //     DrawingCanvas.Children.Add(shapeToDraw);
            // }
            var result = resolver.AddGesture(strongestGesture.ApplicationGesture, center);

            if (result is not null)
            {
                var completeDrawer = new GestureToShape();
                completeDrawer.SetGestureBrush(Brushes.Black);
                completeDrawer.SetGestureStrokeWidth(5);
                var completeDrawing = knownGestures.Gestures[result].gesture.SequenceOfGestures.Select(gest =>
                    completeDrawer.DrawGesture(gest.baseShape,
                        new Point(resolver.CombinedGestureCenter.X + gest.centerOffset.X,
                            resolver.CombinedGestureCenter.Y + gest.centerOffset.Y), width, height));
                foreach (var drawing in completeDrawing)
                {
                    DrawingCanvas.Children.Add(drawing);
                }
                knownGestures.Gestures[result].completionAction();
            }

            foreach (var (gesture,offset) in resolver.GetPossibleNextSteps)
            {
                var shapeToDraw = drawer.DrawGesture(gesture,new Point(resolver.CombinedGestureCenter.X + offset.X, resolver.CombinedGestureCenter.Y + offset.Y),width,height);
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
                    shapeToDraw.Tag = "temp";
                    DrawingCanvas.Children.Add(shapeToDraw);
                }
            }// var shapeToDraw = GestureToShape.DrawGesture(strongestGesture.ApplicationGesture,center,width,height);
            // if (shapeToDraw is null)
            // {
            //     var redStrokes = e.Strokes.Select(stroke =>
            //     {
            //         stroke.DrawingAttributes.Color = Colors.Red;
            //         return stroke;
            //     });
            //     DrawingCanvas.Strokes.Add(new StrokeCollection(redStrokes));
            // }
            // else
            // {
            //     var circle = new Ellipse();
            //     circle.Width = 5;
            //     circle.Height = 5;
            //     circle.Fill = Brushes.Blue;
            //     circle = SetElementCenterPositionOnInkCanvas(circle, center.X,
            //         center.Y);
            //     DrawingCanvas.Children.Add(circle);
            //
            //     DrawingCanvas.Children.Add(shapeToDraw);
            // }


            // var allPoints = e.Strokes.SelectMany(stroke => stroke.StylusPoints).ToArray();
            // var allX = allPoints.Select(point => point.X).ToArray();
            // var allY = allPoints.Select(point => point.Y).ToArray();
            // var width = allX.Max() - allX.Min();
            // var height = allY.Max() - allY.Min();
            // var center = new Point(allX.Min() + width / 2.0,allY.Min() + height / 2.0);
            // var shapeToDraw = GestureToShape.DrawGesture(strongestGesture.ApplicationGesture,center,width,height);
            // if (shapeToDraw is null)
            // {
            //     var redStrokes = e.Strokes.Select(stroke =>
            //     {
            //         stroke.DrawingAttributes.Color = Colors.Red;
            //         return stroke;
            //     });
            //     DrawingCanvas.Strokes.Add(new StrokeCollection(redStrokes));
            // }
            // else
            // {
            //     var circle = new Ellipse();
            //     circle.Width = 5;
            //     circle.Height = 5;
            //     circle.Fill = Brushes.Blue;
            //     circle = SetElementCenterPositionOnInkCanvas(circle, center.X,
            //         center.Y);
            //     DrawingCanvas.Children.Add(circle);
            //
            //     DrawingCanvas.Children.Add(shapeToDraw);
            // }
            // CreateTextBlock(strongestGesture.ApplicationGesture.ToString(),allX.Max(),allY.Max());
            
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