using UnityEngine.UIElements;

namespace TriInspector
{
    public static class VisualElementExtensions
    {
        public static T FindAncestor<T>(this VisualElement element) where T : VisualElement
        {
            for (var current = element.parent; current != null; current = current.parent)
            {
                if (current is T result)
                {
                    return result;
                }
            }

            return null;
        }
    }
}