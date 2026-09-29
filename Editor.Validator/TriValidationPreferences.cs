using UnityEditor;

namespace TriInspector.Validator
{
    internal static class TriValidationPreferences
    {
        private const string AutoKey = "TriInspector.Validator.Auto";

        public static bool Auto
        {
            get => EditorPrefs.GetBool(AutoKey, true);
            set => EditorPrefs.SetBool(AutoKey, value);
        }

        public static bool IsAuto(TriValidationAutoTrigger trigger)
        {
            return Auto && IsAutoTriggerEnabled(trigger);
        }

        public static bool IsAutoTriggerEnabled(TriValidationAutoTrigger trigger)
        {
            return EditorPrefs.GetBool(GetKey(trigger), true);
        }

        public static void SetAutoTriggerEnabled(TriValidationAutoTrigger trigger, bool enabled)
        {
            EditorPrefs.SetBool(GetKey(trigger), enabled);
        }

        private static string GetKey(TriValidationAutoTrigger trigger)
        {
            return AutoKey + "." + trigger;
        }
    }
}