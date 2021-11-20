using System;
using System.Windows;

namespace CombinedGestures
{
    public static class PointExtensions
    {
        public static double GetDistanceTo(this Point point, Point point1) =>
            Math.Sqrt((point1.X - point.X) * (point1.X - point.X) + (point1.Y - point.Y) * (point1.Y - point.Y));

    }
}