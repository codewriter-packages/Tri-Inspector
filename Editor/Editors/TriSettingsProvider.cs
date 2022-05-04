using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
    public class TriSettingsProvider : SettingsProvider
    {
        private class Styles
        {
            public static readonly GUIContent Mode = new GUIContent("Tri Inspector Mode");
        }

        public TriSettingsProvider()
            : base("Project/Tri Inspector", SettingsScope.Project)
        {
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            base.OnGUI(searchContext);

            DrawModeButton();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawModeButton()
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(Styles.Mode);

            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var isFull = HasScriptingDefine(targetGroup, "TRI_INSPECTOR");
            var isCompact = !isFull;

            if (isCompact != GUILayout.Toggle(isCompact, "Compact", EditorStyles.miniButtonLeft) && !isCompact)
            {
                UpdateScriptingDefine(targetGroup, toRemove: "TRI_INSPECTOR");
            }

            if (isFull != GUILayout.Toggle(isFull, "Full", EditorStyles.miniButtonRight) && !isFull)
            {
                UpdateScriptingDefine(targetGroup, toAdd: "TRI_INSPECTOR");
            }

            GUILayout.EndHorizontal();
        }

        private static bool HasScriptingDefine(BuildTargetGroup targetGroup, string define)
        {
            return PlayerSettings
                .GetScriptingDefineSymbolsForGroup(targetGroup)
                .Split(';')
                .Contains(define);
        }

        private static void UpdateScriptingDefine(BuildTargetGroup targetGroup,
            string toAdd = null, string toRemove = null)
        {
            var defines = PlayerSettings
                .GetScriptingDefineSymbolsForGroup(targetGroup)
                .Split(';')
                .ToList();

            if (toAdd != null && !defines.Contains(toAdd))
            {
                defines.Add(toAdd);
            }

            if (toRemove != null && defines.Contains(toRemove))
            {
                defines.Remove(toRemove);
            }

            var definesString = string.Join(";", defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, definesString);
        }

        [SettingsProvider]
        public static SettingsProvider CreateTriInspectorSettingsProvider()
        {
            var provider = new TriSettingsProvider
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>(),
            };

            return provider;
        }
    }
}