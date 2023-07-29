using System;
using UnityEditor;
using UnityEngine;

namespace TriInspectorUnityInternalBridge
{
    internal static class GUIClipProxy
    {
        private static Func<Vector2, Vector2> _guiClipUnClipVector2;

        [InitializeOnLoadMethod]
        private static void Setup()
        {
            var imGuiModuleAssembly = typeof(GUI).Assembly;
            var guiClipType = imGuiModuleAssembly.GetType("UnityEngine.GUIClip", throwOnError: true);

            _guiClipUnClipVector2 = (Func<Vector2, Vector2>) Delegate.CreateDelegate(typeof(Func<Vector2, Vector2>),
                guiClipType.GetMethod("Unclip", new[] {typeof(Vector2)}));
        }

        public static Vector2 UnClip(Vector2 pos)
        {
            return _guiClipUnClipVector2.Invoke(pos);
        }
    }
}