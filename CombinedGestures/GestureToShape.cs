using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace CombinedGestures
{
    public static class GestureToShape
    {
        private static readonly Dictionary<ApplicationGesture, Func<Point,double,double,FrameworkElement>> MethodList =
            new()
            {
                // { ApplicationGesture.Check , DrawCheck},
                { ApplicationGesture.Circle , DrawCircle},
                { ApplicationGesture.Down , DrawDown},
                { ApplicationGesture.Up , DrawUp},
                { ApplicationGesture.Left , DrawLeft},
                { ApplicationGesture.Right , DrawRight},
                { ApplicationGesture.Square , DrawSquare},
                { ApplicationGesture.Star , DrawStar},
                { ApplicationGesture.Tap , DrawTap},
                { ApplicationGesture.Triangle , DrawTriangle},
                // { ApplicationGesture.ChevronDown , DrawChevronDown},
                // { ApplicationGesture.ChevronUp , DrawChevronUp},
                // { ApplicationGesture.ChevronLeft , DrawChevronLeft},
                // { ApplicationGesture.ChevronRight , DrawChevronRight},
            };

        private static Brush _gestureBrush = Brushes.LightGray;
        
        public static void SetGestureBrush(Brush col)
        {
            if (col is not null)
            {
                _gestureBrush = col;
            }
        }

        private static double _strokeWidth = 3.0;

        public static void SetGestureStrokeWidth(double val)
        {
            if (val > 0.0)
            {
                _strokeWidth = val;
            }
        }
        
        public static FrameworkElement DrawGesture(ApplicationGesture gesture, Point center, double totalWidth, double totalHeight)
        {
            return !MethodList.ContainsKey(gesture) ? null : MethodList[gesture](center,totalWidth,totalHeight);
        }
        
        private static T SetElementCenterPositionOnInkCanvas<T>(T element,double x, double y)
            where T : FrameworkElement
        {
            InkCanvas.SetTop(element, y - (element.Height/2.0));
            InkCanvas.SetLeft(element, x - (element.Width/2.0));
            return element;
        }

        private static T GetBaseContainer<T>(double width, double height)
        where T : FrameworkElement, new()
        {
            var canvas = new T()
            {
                Width = width,
                Height = height
            };
            return canvas;
        }

        private static FrameworkElement DrawCircle(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Grid>(Math.Max(totalWidth, totalHeight),Math.Max(totalWidth, totalHeight));
            var circle = new Ellipse();
            circle.Stroke = _gestureBrush;
            circle.Stretch = Stretch.UniformToFill;
            circle.StrokeThickness = _strokeWidth;
            circle.HorizontalAlignment = HorizontalAlignment.Stretch;
            circle.VerticalAlignment = VerticalAlignment.Stretch;
            canvas.Children.Add(circle);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }

        private static FrameworkElement DrawCheck(Point center, double totalWidth, double totalHeight)
        {
            throw new NotImplementedException();
        }

        private static Point RotatePointAroundAnother(Point centreOfRotation, Point pointToRotate, double angle)
        {
            double DegToRad(double deg) => deg * Math.PI / 180.0;
            var s = Math.Sin(DegToRad(angle));
            var c = Math.Cos(DegToRad(angle));
            var baseTranslatedPoint = new Point()
            {
                X = pointToRotate.X - centreOfRotation.X,
                Y = pointToRotate.Y - centreOfRotation.Y
            };
            return new Point
            {
                X = ((baseTranslatedPoint.X) * c - (baseTranslatedPoint.Y) * s) + centreOfRotation.X,
                Y = ((baseTranslatedPoint.X) * s + (baseTranslatedPoint.Y) * c) + centreOfRotation.Y
            };
        }

        private static FrameworkElement DrawStar(Point center, double totalWidth, double totalHeight)
        {
            var avgSize = (totalWidth + totalHeight) / 2.0;
            var canvas = GetBaseContainer<Canvas>(avgSize,avgSize);

            var star = new Polygon();
            star.Stroke = _gestureBrush;
            star.StrokeThickness = _strokeWidth;
            star.Fill = Brushes.Transparent;

            var startPoint = new Point(canvas.Width/2.0, 0);
            var canvasCenter = new Point(canvas.Width / 2.0, canvas.Height / 2.0);
            var starPoints = new[]
            {
                RotatePointAroundAnother(canvasCenter,startPoint,72*0), 
                RotatePointAroundAnother(canvasCenter,startPoint,72*2), 
                RotatePointAroundAnother(canvasCenter,startPoint,72*4),
                RotatePointAroundAnother(canvasCenter,startPoint,72*1), 
                RotatePointAroundAnother(canvasCenter,startPoint,72*3)
            };

            star.Points = new PointCollection(starPoints);
            canvas.Children.Add(star);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawDown(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Grid>(totalWidth,totalHeight);
            var line = new Rectangle();
            line.Fill = _gestureBrush;
            line.Stretch = Stretch.UniformToFill;
            line.Width = _strokeWidth;
            line.HorizontalAlignment = HorizontalAlignment.Center;
            line.VerticalAlignment = VerticalAlignment.Stretch;

            canvas.Children.Add(line);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawUp(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Grid>(totalWidth,totalHeight);
            var line = new Rectangle();
            line.Fill = _gestureBrush;
            line.Stretch = Stretch.UniformToFill;
            line.Width = _strokeWidth;
            line.HorizontalAlignment = HorizontalAlignment.Center;
            line.VerticalAlignment = VerticalAlignment.Stretch;

            canvas.Children.Add(line);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawLeft(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Grid>(totalWidth,totalHeight);
            var line = new Rectangle();
            line.Fill = _gestureBrush;
            line.Stretch = Stretch.UniformToFill;
            line.Height = _strokeWidth;
            line.HorizontalAlignment = HorizontalAlignment.Stretch;
            line.VerticalAlignment = VerticalAlignment.Center;

            canvas.Children.Add(line);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawRight(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Grid>(totalWidth,totalHeight);
            var line = new Rectangle();
            line.Fill = _gestureBrush;
            line.Stretch = Stretch.UniformToFill;
            line.Height = _strokeWidth;
            line.HorizontalAlignment = HorizontalAlignment.Stretch;
            line.VerticalAlignment = VerticalAlignment.Center;

            canvas.Children.Add(line);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawSquare(Point center, double totalWidth, double totalHeight)
        {
            var avgSize = (totalWidth + totalHeight) / 2.0;
            var canvas = GetBaseContainer<Grid>(avgSize,avgSize);
            var rectangle = new Rectangle();
            rectangle.Stroke = _gestureBrush;
            rectangle.StrokeThickness = _strokeWidth;
            rectangle.Stretch = Stretch.UniformToFill;
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;

            canvas.Children.Add(rectangle);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawTap(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Canvas>(totalWidth,totalHeight);
            var dot = new Ellipse();
            dot.Fill = _gestureBrush;
            dot.Width = _strokeWidth;
            dot.Height = _strokeWidth;

            canvas.Children.Add(dot);
            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }
        
        private static FrameworkElement DrawTriangle(Point center, double totalWidth, double totalHeight)
        {
            var canvas = GetBaseContainer<Canvas>(totalWidth,totalHeight);
            var triangle = new Polygon();
            triangle.Stroke = _gestureBrush;
            triangle.StrokeThickness = _strokeWidth;
            triangle.Stretch = Stretch.UniformToFill;
            triangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            triangle.VerticalAlignment = VerticalAlignment.Stretch;
            triangle.Fill = Brushes.Transparent;
            triangle.Points = new PointCollection(new[] { new Point(0, canvas.Height), new Point(canvas.Width, canvas.Height), new Point(canvas.Width/2.0, 0) });

            canvas.Children.Add(triangle);

            canvas = SetElementCenterPositionOnInkCanvas(canvas, center.X, center.Y);

            return canvas;
        }

        private static FrameworkElement DrawChevronUp()
        {
            throw new NotImplementedException();
        }
        
        private static FrameworkElement DrawChevronDown()
        {
            throw new NotImplementedException();
        }
        
        private static FrameworkElement DrawChevronLeft()
        {
            throw new NotImplementedException();
        }
        
        private static FrameworkElement DrawChevronRight()
        {
            throw new NotImplementedException();
        }
    }
}