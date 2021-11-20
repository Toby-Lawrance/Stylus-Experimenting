using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Ink;

namespace CombinedGestures
{
    public class CombinedGestureContainer
    {
        private Dictionary<string, (CombinedGestureDefinition gesture, Action completionAction)> _namedGestures = new();

        public IReadOnlyDictionary<string, (CombinedGestureDefinition gesture, Action completionAction)> Gestures =>
            _namedGestures;

        public bool TryAddGesture(string name, CombinedGestureDefinition gesture, Action completedAction)
        {
            if (_namedGestures.ContainsKey(name))
            {
                return false;
            }

            var identical = _namedGestures.Select(kvp => kvp.Value.gesture.Equals(gesture)).Any(x => x);
            if (identical)
            {
                return false;
            }

            _namedGestures.Add(name, (gesture, completedAction));
            return true;
        }

        public (ApplicationGesture gesture, Point centerOffset) GetGestureStep(string name, int stepIndex)
        {
            if (!_namedGestures.ContainsKey(name))
            {
                throw new ArgumentException($"The gesture: {name} was not found in the container");
            }

            if (GetGestureStepCount(name) <= stepIndex)
            {
                throw new ArgumentException($"The selected gesture: {name} does not have {stepIndex + 1} steps to it");
            }

            var (baseShape, centerOffset) = _namedGestures[name].gesture.SequenceOfGestures[stepIndex];
            return (baseShape: baseShape, centerOffset: centerOffset);
        }

        public int GetGestureStepCount(string name)
        {
            if (!_namedGestures.ContainsKey(name))
            {
                throw new ArgumentException($"The gesture: {name} was not found in the container");
            }

            return _namedGestures[name].gesture.SequenceOfGestures.Count;
        }
        
    }
}