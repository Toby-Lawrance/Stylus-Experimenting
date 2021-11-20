using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace CombinedGestures
{
    public class CombinedGestureResolver
    {
        private readonly CombinedGestureContainer _possibilities;
        private int step = 0;
        public int Step => step;
        private IEnumerable<string> currentPossibilityKeys;
        public IEnumerable<string> CurrentlyPossible => currentPossibilityKeys;
        private bool recognitionComplete = false;
        public bool RecognitionComplete => recognitionComplete;

        public Point CombinedGestureCenter = new();

        public CombinedGestureResolver(CombinedGestureContainer available)
        {
            _possibilities = available;
            currentPossibilityKeys = _possibilities.Gestures.Select(kvp => kvp.Key).ToList();
        }

        public void Reset()
        {
            currentPossibilityKeys = _possibilities.Gestures.Select(kvp => kvp.Key).ToList();
            recognitionComplete = false;
            step = 0;
        }

        public string AddGesture(ApplicationGesture gesture, Point gesturePosition)
        {
            if (RecognitionComplete)
            {
                return CurrentlyPossible.First();
            }

            var gestureRelativeCenter = new Point((gesturePosition - CombinedGestureCenter).X,(gesturePosition - CombinedGestureCenter).Y);

            currentPossibilityKeys = currentPossibilityKeys
                .Where(possibility =>
                    _possibilities.GetGestureStep(possibility, step).gesture == gesture)
                .Where(possibility => _possibilities.GetGestureStepCount(possibility) > step)
                .Where(possibility =>
                    _possibilities.GetGestureStep(possibility, step).centerOffset
                        .GetDistanceTo(gestureRelativeCenter) <=
                    _possibilities.Gestures[possibility].gesture.DistanceTolerance
                );

            foreach (var possibility in CurrentlyPossible)
            {
                //We are on the last step
                if (_possibilities.GetGestureStepCount(possibility) == step + 1)
                {
                    recognitionComplete = true;
                    return possibility;
                }
            }

            step++;
            return null;
        }

        public IEnumerable<(ApplicationGesture gesture, Point centerOffset)> GetPossibleNextSteps
        {
            get
            {
                return CurrentlyPossible.Select(possibility => _possibilities.GetGestureStep(possibility, step));
            }
        }
    }
}