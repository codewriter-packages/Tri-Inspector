using UnityEngine.UIElements;

namespace TriInspector.Utilities
{
    internal static class TriColumnSizes
    {
        public static void Apply(VisualElement element, float[] sizes, int index)
        {
            if (sizes != null && index < sizes.Length && sizes[index] > 0f)
            {
                if (sizes[index] < 1f)
                {
                    element.style.flexGrow = sizes[index];
                    element.style.flexBasis = 0;
                }
                else
                {
                    element.style.minWidth = sizes[index];
                    element.style.width = sizes[index];
                    element.style.flexGrow = 0;
                    element.style.flexShrink = 0;
                }
            }
            else
            {
                element.style.flexGrow = 1;
                element.style.flexBasis = 0;
            }
        }
    }
}
