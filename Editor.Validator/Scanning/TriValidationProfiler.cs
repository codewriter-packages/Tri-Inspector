using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace TriInspector.Validator
{
    internal static class TriValidationProfiler
    {
        private static bool Enabled => TriValidatorSettings.instance.EnableProfiler;

        private struct TypeStats
        {
            public int count;
            public long ticks;
        }

        private struct ValidatorStats
        {
            public int count;
            public long ticks;
        }

        private static readonly Dictionary<string, TypeStats> Stats = new();
        private static readonly Dictionary<string, ValidatorStats> ValidatorStatsByKey = new();

        private static readonly Stopwatch ScanStopwatch = new();
        private static int _objectCount;

        public static void BeginScan()
        {
            if (!Enabled)
            {
                return;
            }

            Stats.Clear();
            ValidatorStatsByKey.Clear();
            _objectCount = 0;
            ScanStopwatch.Restart();

            TriValidationProfilerHook.onValidatorRun = RecordValidator;
        }

        private static void RecordValidator(string validatorTypeName, TriProperty property, long ticks)
        {
            var rootType = property.PropertyTree.TargetObjectType?.Name ?? "?";
            var path = property.IsRootProperty ? "<root>" : property.PropertyPath;
            var key = $"{rootType}.{path} [{validatorTypeName}]";

            ValidatorStatsByKey.TryGetValue(key, out var s);
            s.count++;
            s.ticks += ticks;
            ValidatorStatsByKey[key] = s;
        }

        public static void Record(string typeName, long ticks)
        {
            if (!Enabled)
            {
                return;
            }

            Stats.TryGetValue(typeName, out var s);
            s.count++;
            s.ticks += ticks;
            Stats[typeName] = s;

            _objectCount++;
        }

        public static void EndScanAndReport()
        {
            if (!Enabled)
            {
                return;
            }

            ScanStopwatch.Stop();
            TriValidationProfilerHook.onValidatorRun = null;

            var sorted = new List<KeyValuePair<string, TypeStats>>(Stats);
            sorted.Sort((a, b) => b.Value.ticks.CompareTo(a.Value.ticks));

            var sb = new StringBuilder();
            sb.AppendLine("[TriInspector] Project validation — performance report");
            sb.AppendLine($"  Total time: {FmtMs(ScanStopwatch.Elapsed.TotalMilliseconds)}  " +
                          $"({_objectCount} objects checked across {Stats.Count} types)");
            AppendTopObjects(sb, sorted);
            AppendTopValidators(sb);

            Debug.Log(sb.ToString());
        }

        private static void AppendTopObjects(StringBuilder sb, List<KeyValuePair<string, TypeStats>> sorted)
        {
            sb.AppendLine();
            sb.AppendLine("  Slowest object types (top 10):");
            sb.AppendLine($"    {"type",-40} {"checked",8} {"total time",12} {"avg per object",16}");

            var typeLimit = sorted.Count < 10 ? sorted.Count : 10;
            for (var i = 0; i < typeLimit; i++)
            {
                var s = sorted[i].Value;
                var totalMs = Ms(s.ticks);
                var avgMs = totalMs / s.count;
                sb.AppendLine($"    {Trim(sorted[i].Key, 40),-40} {s.count,8} {FmtMs(totalMs),12} {FmtMs(avgMs),16}");
            }
        }

        private static void AppendTopValidators(StringBuilder sb)
        {
            if (ValidatorStatsByKey.Count == 0)
            {
                return;
            }

            var sorted = new List<KeyValuePair<string, ValidatorStats>>(ValidatorStatsByKey);
            sorted.Sort((a, b) => b.Value.ticks.CompareTo(a.Value.ticks));

            sb.AppendLine();
            sb.AppendLine("  Slowest validation checks (type.field [rule], top 10):");
            sb.AppendLine($"    {"check",-70} {"times run",10} {"total time",12} {"avg",10}");

            var limit = sorted.Count < 10 ? sorted.Count : 10;
            for (var i = 0; i < limit; i++)
            {
                var kv = sorted[i];
                var s = kv.Value;
                var totalMs = Ms(s.ticks);
                var avgMs = totalMs / s.count;
                sb.AppendLine($"    {Trim(kv.Key, 70),-70} {s.count,10} {FmtMs(totalMs),12} {FmtMs(avgMs),10}");
            }
        }

        private static double Ms(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency;
        }

        private static string FmtMs(double ms)
        {
            return ms >= 1000.0 ? $"{ms / 1000.0:F1} s" : $"{ms:F1} ms";
        }

        private static string Trim(string s, int max)
        {
            return s.Length <= max ? s : s.Substring(0, max - 1) + "~";
        }
    }
}