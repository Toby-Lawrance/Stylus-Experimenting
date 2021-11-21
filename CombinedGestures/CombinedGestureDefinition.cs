using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace CombinedGestures
{
    public class CombinedGestureDefinition
    {
        public readonly IReadOnlyList<(ApplicationGesture baseShape,Point centerOffset)> SequenceOfGestures;
        private readonly double _distanceTolerance;
        public double DistanceTolerance => _distanceTolerance;

        public CombinedGestureDefinition(IEnumerable<(ApplicationGesture baseShape, Point centerOffset)> seq,
            double tolerance = 25.0)
        {
            SequenceOfGestures = new List<(ApplicationGesture baseShape,Point centerOffset)>(seq);
            _distanceTolerance = tolerance;
        }

        public override bool Equals(object? obj)
        {

            if (obj is not CombinedGestureDefinition cgd)
            {
                return false;
            }

            if (Math.Abs(_distanceTolerance - cgd._distanceTolerance) > double.Epsilon)
            {
                return false;
            }
                
            foreach (var (ours, theirs) in SequenceOfGestures.Zip(cgd.SequenceOfGestures))
            {
                if (ours.baseShape != theirs.baseShape)
                {
                    return false;
                }

                if (ours.centerOffset.GetDistanceTo(theirs.centerOffset) > _distanceTolerance)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join(",",
                SequenceOfGestures.Select((g) => $"({g.baseShape.ToString()},{g.centerOffset.ToString()})"));
        }
    }
}