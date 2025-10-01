using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TriInspectorUnityInternalBridge
{
    internal static class GUIClipProxy
    {
        private static Func<Vector2, Vector2> _guiClipUnClipVector2;
        private static Func<Rect, Rect> _guiClipUnClipToWindowRect;
        private static Func<Rect> _guiClipVisibleRect;

        [InitializeOnLoadMethod]
        private static void Setup()
        {
            var imGuiModuleAssembly = typeof(GUI).Assembly;
            var guiClipType = imGuiModuleAssembly.GetType("UnityEngine.GUIClip", throwOnError: true);

            _guiClipUnClipVector2 = (Func<Vector2, Vector2>) Delegate.CreateDelegate(typeof(Func<Vector2, Vector2>),
                guiClipType.GetMethod("Unclip", new[] {typeof(Vector2)}));

            _guiClipUnClipToWindowRect = (Func<Rect, Rect>) Delegate.CreateDelegate(typeof(Func<Rect, Rect>),
                guiClipType.GetMethod("UnclipToWindow", new[] {typeof(Rect)}));

            _guiClipVisibleRect = (Func<Rect>) Delegate.CreateDelegate(typeof(Func<Rect>),
                guiClipType.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.NonPublic).GetMethod);
        }

        public static Rect VisibleRect => _guiClipVisibleRect.Invoke();

        public static Vector2 UnClip(Vector2 pos)
        {
            return _guiClipUnClipVector2.Invoke(pos);
        }

        public static Rect UnClipToWindow(Rect rect)
        {
            return _guiClipUnClipToWindowRect.Invoke(rect);
        }
    }
}