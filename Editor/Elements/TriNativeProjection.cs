using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal static class TriNativeProjection
    {
        public static bool IsNative(TriElement element)
        {
            return element is TriValidatorsElement
                   || element is TriListElement
                   || element is TriInlineGenericElement
                   || element is TriFoldoutElement
                   || element is TriReferenceElement
                   || element is TriNoDrawerElement
                   || element is TriBuiltInPropertyElement
                   || element is TriObjectReferenceElement;
        }

        public static VisualElement ProjectField(TriProperty property, TriElement element)
        {
            if (IsNative(element))
            {
                return element.CreateVisualElement(property);
            }

            return new TriImguiContainerImpl(property, element, applyPropertyContext: true);
        }
    }
}
