using System;
using System.Collections.Generic;

namespace TriInspector.Validator
{
    internal static class TriValidationHierarchyCounts
    {
        private static Dictionary<TriEntityID, (int error, int warning)> _counts = new();

        public static event Action Changed;

        public static void Publish(Dictionary<TriEntityID, (int error, int warning)> counts)
        {
            _counts = counts ?? new Dictionary<TriEntityID, (int error, int warning)>();
            Changed?.Invoke();
        }

        public static bool TryGet(TriEntityID id, out int errorCount, out int warningCount)
        {
            if (_counts.TryGetValue(id, out var value))
            {
                errorCount = value.error;
                warningCount = value.warning;
                return true;
            }

            errorCount = 0;
            warningCount = 0;
            return false;
        }
    }
}