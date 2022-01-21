using System;

namespace TriInspector.Utilities
{
    internal static class InternalEditorUtilityProxy
    {
        private static Func<UnityEngine.Object, bool> _getIsInspectorExpanded;
        private static Action<UnityEngine.Object, bool> _setIsInspectorExpanded;

        public static bool GetIsInspectorExpanded(UnityEngine.Object obj)
        {
            if (_getIsInspectorExpanded == null)
            {
                _getIsInspectorExpanded = TriReflectionUtilities
                    .GetUnityEditorTypeByName("InternalEditorUtility")
                    .CompileStaticMethod<UnityEngine.Object, bool>("GetIsInspectorExpanded");
            }

            return _getIsInspectorExpanded.Invoke(obj);
        }

        public static void SetIsInspectorExpanded(UnityEngine.Object obj, bool isExpanded)
        {
            if (_setIsInspectorExpanded == null)
            {
                _setIsInspectorExpanded = TriReflectionUtilities
                    .GetUnityEditorTypeByName("InternalEditorUtility")
                    .CompileStaticVoidMethod<UnityEngine.Object, bool>("SetIsInspectorExpanded");
            }

            _setIsInspectorExpanded.Invoke(obj, isExpanded);
        }
    }
}