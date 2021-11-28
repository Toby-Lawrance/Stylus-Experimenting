using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StrokeSimiliarity
{
    public interface IStrokeComparer
    {
        double Compare(IEnumerable<Point> baseStroke, IEnumerable<Point> compareStroke);
    }
}