using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriHeaderBoxGroupVisualElement : TriBoxGroupBaseVisualElement
    {
        private readonly Label _titleLabel;

        public TriHeaderBoxGroupVisualElement(string title, bool hideIfChildrenInvisible)
            : base(title, hideIfChildrenInvisible)
        {
            var header = new VisualElement();
            header.AddToClassList(TriStyles.BoxGroupHeader);

            _titleLabel = new Label();
            _titleLabel.AddToClassList(TriStyles.BoxGroupTitle);
            header.Add(_titleLabel);

            Add(header);

            var content = new VisualElement();
            Add(content);
            UseContent(content);
        }

        protected override void OnSync(string title)
        {
            _titleLabel.text = title;
        }
    }
}
