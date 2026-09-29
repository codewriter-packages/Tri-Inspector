using System.Collections.Generic;
using UnityEditor;

namespace TriInspector.Validator
{
    internal static class TriValidationForegroundJob
    {
        private const double MinReportInterval = 0.1;

        public static bool Run(IEnumerator<TriValidationScanStep> routine)
        {
            var lastReport = 0d;

            try
            {
                while (routine.MoveNext())
                {
                    var step = routine.Current;

                    var now = EditorApplication.timeSinceStartup;
                    if (now - lastReport < MinReportInterval || step.Equals(default))
                    {
                        continue;
                    }

                    lastReport = now;

                    if (EditorUtility.DisplayCancelableProgressBar("Tri Validator", step.info, step.Fraction))
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                routine?.Dispose();

                EditorUtility.ClearProgressBar();
            }
        }
    }
}