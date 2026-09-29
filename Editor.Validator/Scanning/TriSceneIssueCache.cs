using System;
using System.Collections.Generic;

namespace TriInspector.Validator
{
    [Serializable]
    internal struct SceneIssueEntry
    {
        public string guid;
        public int errors;
        public int warnings;
    }

    internal static class TriSceneIssueCache
    {
        private static readonly Dictionary<string, (int errors, int warnings)> Counts = new();

        public static void Clear()
        {
            Counts.Clear();
        }

        public static void Record(string guid, int errors, int warnings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            Counts[guid] = (errors, warnings);
        }

        public static void Export(List<SceneIssueEntry> entries)
        {
            entries.Clear();

            foreach (var pair in Counts)
            {
                entries.Add(new SceneIssueEntry
                {
                    guid = pair.Key,
                    errors = pair.Value.errors,
                    warnings = pair.Value.warnings,
                });
            }
        }

        public static void Import(List<SceneIssueEntry> entries)
        {
            foreach (var entry in entries)
            {
                Record(entry.guid, entry.errors, entry.warnings);
            }
        }
    }
}