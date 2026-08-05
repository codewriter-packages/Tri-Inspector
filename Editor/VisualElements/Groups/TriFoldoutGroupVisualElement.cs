using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriFoldoutGroupVisualElement : TriBoxGroupBaseVisualElement
    {
        private readonly Foldout _foldout;

        public TriFoldoutGroupVisualElement(string title, bool expandedByDefault, bool hideIfChildrenInvisible)
            : base(title, hideIfChildrenInvisible)
        {
            _foldout = new Foldout
            {
                text = title,
                value = expandedByDefault,
            };

            Add(_foldout);
            UseContent(_foldout.contentContainer);
        }

        protected override void OnSync(string title)
        {
            _foldout.text = title;
        }
    }
}
