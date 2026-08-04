using UnityEngine.UIElements;

namespace TriInspector
{
    public interface ITriElement
    {
        VisualElement CreateVisualElement(TriProperty property);
    }
}
