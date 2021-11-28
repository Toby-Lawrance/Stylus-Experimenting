using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Windows;
using StrokeSimiliarity;

namespace CustomShapeComparison
{
    public class PointSimilarityComparer : IStrokeComparer
    {
        private int samplesToTake = 1000;
        private double difficultyFactor = 10.0;

        public PointSimilarityComparer()
        {
        }

        public PointSimilarityComparer(int samples, double distributionStdDev)
        {
            samplesToTake = samples;
            difficultyFactor = distributionStdDev;
        }

        public (Point[] baseSampled, Point[] compareSampled) GetSampledCurves(IEnumerable<Point> baseStroke,
            IEnumerable<Point> compareStroke)
        {
            var baseSampled = TransformToSampled(baseStroke.ToArray()).ToArray();
            var compareSampled = TransformToSampled(compareStroke.ToArray()).ToArray();

            return (baseSampled, compareSampled);
        }

        public static IEnumerable<Point> TransformTo(Point[] stroke, Point pO)
        {
            var firstOfSequence = stroke.First();
            return stroke.Select(p => new Point(p.X - firstOfSequence.X + pO.X, p.Y - firstOfSequence.Y + pO.Y));
        }

        public double Compare(IEnumerable<Point> baseStroke,
            IEnumerable<Point> compareStroke)
        {
            var baseStrokeArr = baseStroke.ToArray();
            var compareStrokeArr = compareStroke.ToArray();
            
            var (baseSampled, compareSampled) = GetSampledCurves(baseStrokeArr, compareStrokeArr);

            baseSampled = TransformTo(baseSampled, new Point()).ToArray();
            compareSampled = TransformTo(compareSampled, new Point()).ToArray();

            var averageResult = baseSampled
                .Zip(compareSampled, (p1, p2) => HalfNormalCDF(p1.GetDistanceTo(p2), difficultyFactor)).Average();

            return 1.0 - averageResult;
        }

        private static double HalfNormalCDF(double x, double stdDev)
        {
            var param = x / (stdDev * Math.Sqrt(2));
            return GaussERF(param);
        }
        
        private static double GaussERF(double x)
        {
            var multiplier = x < 0.0 ? -1.0 : 1.0;
            x *= multiplier;
            double[] a = { 0.0705230784, 0.0422820123, 0.0092705272,0.0001520143,0.0002765672,0.0000430638 };
            var denominator = 1.0 + a.Select((ai, i) => ai * Math.Pow(x, i + 1)).Sum();
            return (1.0 - (1.0 / denominator)) * multiplier;
        }

        private IEnumerable<Point> TransformToSampled(Point[] points)
        {
            var totalLength = GetPathLength(points);
            var distancePerSample = totalLength / (samplesToTake - 1);

            var distances = Enumerable.Range(0, samplesToTake).Select(i => i * distancePerSample).ToArray();

            return distances.Select(d => SampleLineAtDistance(points, d)).ToArray();
        }

        private static Point SampleLineAtDistance(Point[] polyLine, double distance)
        {
            var windowed = GetWindowedPoints(polyLine).ToArray();

            var cumulativeDistance = 0.0;
            foreach (var (p1, p2) in windowed)
            {
                var dist = p1.GetDistanceTo(p2);
                if (cumulativeDistance + dist >= distance)
                {
                    var distanceAlong = distance - cumulativeDistance;
                    return GetPointXDistanceAlongLine(p1, p2, distanceAlong);
                }

                cumulativeDistance += dist;
            }

            return polyLine.Last();
        }

        private static Point GetPointXDistanceAlongLine(Point p1, Point p2, double distance) => p1 == p2
            ? p1
            : InterpolateBetweenTwoPoints(p1, p2, Math.Clamp(distance / p1.GetDistanceTo(p2), 0.0, 1.0));

        private static Point InterpolateBetweenTwoPoints(Point p1, Point p2, double amount) => amount switch
        {
            <= 0.0 => p1,
            >= 1.0 => p2,
            _ => new Point((p2.X - p1.X) * amount + p1.X, (p2.Y - p1.Y) * amount + p1.Y)
        };

        private static IEnumerable<(Point p1, Point p2)> GetWindowedPoints(Point[] points) =>
            points.Zip(points.Skip(1), (p, p1) => (p, p1));

        private static double GetPathLength(Point[] points) =>
            GetWindowedPoints(points).Select(pair => pair.Item1.GetDistanceTo(pair.Item2)).Sum();
    }
}