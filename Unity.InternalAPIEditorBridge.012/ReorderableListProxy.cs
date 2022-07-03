using UnityEditorInternal;
using UnityEngine;

namespace TriInspectorUnityInternalBridge
{
    internal static class ReorderableListProxy
    {
        public static void DoListHeader(ReorderableList list, Rect headerRect)
        {
            if (list.showDefaultBackground && Event.current.type == EventType.Repaint)
            {
                ReorderableList.defaultBehaviours.DrawHeaderBackground(headerRect);
            }

            headerRect.xMin += 6f;
            headerRect.xMax -= 6f;
            headerRect.height -= 2f;
            headerRect.y += 1;

            list.drawHeaderCallback?.Invoke(headerRect);
        }

        public static void ClearCacheRecursive(ReorderableList list)
        {
#if UNITY_2021_3_OR_NEWER || UNITY_2020_3
            list.InvalidateCacheRecursive();
#elif UNITY_2020_2_OR_NEWER
            list.ClearCacheRecursive();
#endif
        }
    }
}