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

        public PointSimilarityComparer()
        {
            
        }

        public PointSimilarityComparer(int samples)
        {
            samplesToTake = samples;
        }
        
        public (double score,IEnumerable<(Point,double distance)> sampled) Compare(IEnumerable<Point> baseStroke, IEnumerable<Point> compareStroke)
        {
            var baseSampled = TransformToSampled(baseStroke.Distinct().ToArray()).ToArray();
            var compareSampled = TransformToSampled(compareStroke.Distinct().ToArray()).ToArray();

            baseSampled = baseSampled.Select(p => new Point(p.X - baseSampled.First().X, p.Y - baseSampled.First().Y)).ToArray();
            compareSampled = compareSampled.Select(p => new Point(p.X - compareSampled.First().X, p.Y - compareSampled.First().Y)).ToArray();

            var sampleMeasured = baseSampled.Zip(compareSampled, (p1, p2) => (p2,p1.GetDistanceTo(p2))).ToArray();

            return (sampleMeasured.Average(p => p.Item2),sampleMeasured);
        }

        private IEnumerable<Point> TransformToSampled(Point[] points)
        {
            var totalLength = GetPathLength(points);
            var distancePerSample = totalLength / samplesToTake;

            var distances = Enumerable.Range(0, samplesToTake).Select(i => i * distancePerSample).ToArray();

            return distances.Select(d => SampleLineAtDistance(points, d)).ToArray();
        }

        private static Point SampleLineAtDistance(Point[] polyLine, double distance)
        {
            var windowed = GetWindowedPoints(polyLine).ToArray();

            var cumulativeDistance = 0.0;
            foreach (var (p1,p2) in windowed)
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

        private static Point GetPointXDistanceAlongLine(Point p1, Point p2, double distance) => InterpolateBetweenTwoPoints(p1, p2, Math.Clamp(distance / p1.GetDistanceTo(p2),0.0,1.0));

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