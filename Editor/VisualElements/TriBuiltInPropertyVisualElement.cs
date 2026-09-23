using UnityEditor;
using UnityEditor.UIElements;

namespace TriInspector.VisualElements
{
    public class TriBuiltInPropertyVisualElement : PropertyField
    {
        public TriBuiltInPropertyVisualElement(TriProperty property, SerializedProperty serializedProperty)
            : base(serializedProperty)
        {
            this.AutoSyncLabelFromProperty(property);
            this.BindProperty(serializedProperty);
        }
    }
}