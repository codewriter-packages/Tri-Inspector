using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriTableListVisualElement : TriListVisualElement
    {
        private readonly TriProperty _property;
        private readonly List<string> _columnTitles = new List<string>();

        private VisualElement _columnsRow;

        public TriTableListVisualElement(TriProperty property) : base(property)
        {
            _property = property;

            AddToClassList(TriStyles.Table);

            ResolveColumns();

            var labelOverride = new TableListPropertyOverrideContext(property);
            RegisterCallback<AttachToPanelEvent>(_ => property.PropertyTree.AddPropertyOverride(labelOverride));
            RegisterCallback<DetachFromPanelEvent>(_ => property.PropertyTree.RemovePropertyOverride(labelOverride));
        }

        private void ResolveColumns()
        {
            _columnTitles.Clear();

            var elementType = _property.ArrayElementType;
            if (elementType == null)
            {
                return;
            }

            var definition = TriTypeDefinition.GetCached(elementType);
            if (definition.Properties.Count == 0)
            {
                _columnTitles.Add("Element");
                return;
            }

            foreach (var propertyDefinition in definition.Properties)
            {
                _columnTitles.Add(ObjectNames.NicifyVariableName(propertyDefinition.Name));
            }
        }

        protected override VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList(TriStyles.TableHeader);

            header.Add(base.CreateHeader());

            _columnsRow = new VisualElement();
            _columnsRow.AddToClassList(TriStyles.TableHeaderColumns);
            _columnsRow.EnableInClassList(TriStyles.TableHeaderReorderable, reorderable);

            foreach (var columnTitle in _columnTitles)
            {
                var cell = new Label(columnTitle);
                cell.AddToClassList(TriStyles.TableHeaderCell);
                _columnsRow.Add(cell);
            }

            if (allowRemove)
            {
                var spacer = new VisualElement();
                spacer.AddToClassList(TriStyles.TableHeaderRemoveSpacer);
                _columnsRow.Add(spacer);
            }

            header.Add(_columnsRow);

            return header;
        }

        protected override void SetExpanded(bool expanded)
        {
            base.SetExpanded(expanded);

            if (_columnsRow != null)
            {
                _columnsRow.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        protected override VisualElement CreateItemElement(TriProperty property)
        {
            var row = new VisualElement();
            row.AddToClassList(TriStyles.TableRow);

            if (property.PropertyType == TriPropertyType.Generic)
            {
                foreach (var child in property.ChildrenProperties)
                {
                    row.Add(CreateCell(child));
                }
            }
            else
            {
                row.Add(CreateCell(property));
            }

            return row;
        }

        private static VisualElement CreateCell(TriProperty property)
        {
            var cell = new VisualElement();
            cell.AddToClassList(TriStyles.TableCell);
            cell.AddToClassList(TriStyles.UnityInspectorElement);
            cell.AddToClassList(TriStyles.UnityInspectorMainContainer);
            cell.AddToClassList(TriStyles.TriInspectorElement);
            cell.Add(new TriPropertyVisualElement(property, new TriPropertyVisualElement.Props
            {
                forceInline = true,
            }));
            return cell;
        }

        private class TableListPropertyOverrideContext : TriPropertyOverrideContext
        {
            private readonly TriProperty _listProperty;
            private readonly GUIContent _noneLabel = GUIContent.none;

            public TableListPropertyOverrideContext(TriProperty listProperty)
            {
                _listProperty = listProperty;
            }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (property.PropertyType == TriPropertyType.Primitive &&
                    property.Parent?.Parent == _listProperty &&
                    !property.TryGetAttribute(out GroupAttribute _))
                {
                    displayName = _noneLabel;
                    return true;
                }

                displayName = default;
                return false;
            }
        }
    }
}