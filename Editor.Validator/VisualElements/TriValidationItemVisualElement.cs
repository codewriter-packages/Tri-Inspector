using UnityEngine.UIElements;

namespace TriInspector.Validator
{
    internal sealed class TriValidationItemVisualElement : VisualElement
    {
        private readonly VisualElement _content;
        private readonly Image _icon;
        private readonly Label _label;

        public TriValidationItemVisualElement()
        {
            AddToClassList(TriValidationStyles.Row);

            _content = new VisualElement();
            _content.AddToClassList(TriValidationStyles.Content);
            Add(_content);

            _icon = new Image();
            _icon.AddToClassList(TriValidationStyles.Icon);
            _content.Add(_icon);

            _label = new Label();
            _content.Add(_label);
        }

        public void Bind(TriValidationNodeData data)
        {
            FindItemContainer()?.EnableInClassList(TriValidationStyles.RowRoot, data.isRoot);

            _content.EnableInClassList(TriValidationStyles.ContentComponent,
                data.kind == TriValidationNodeKind.Component);

            _icon.image = data.icon;
            _icon.style.display = data.icon != null ? DisplayStyle.Flex : DisplayStyle.None;

            _label.text = data.label;

            var isIssue = data.kind == TriValidationNodeKind.Issue;
            _label.EnableInClassList(TriValidationStyles.LabelWarning,
                isIssue && data.severity == TriMessageType.Warning);
            _label.EnableInClassList(TriValidationStyles.LabelError,
                isIssue && data.severity == TriMessageType.Error);
        }

        private VisualElement FindItemContainer()
        {
            for (var current = parent; current != null; current = current.parent)
            {
                if (current.ClassListContains("unity-tree-view__item"))
                {
                    return current;
                }
            }

            return null;
        }
    }
}