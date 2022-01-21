using System.Collections.Generic;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriTabGroupElement : TriHeaderGroupBaseElement
    {
        private const string DefaultTabName = "Main";

        private readonly List<string> _tabNames;
        private readonly Dictionary<string, TriElement> _tabElements;

        private string _activeTabName;

        public TriTabGroupElement()
        {
            _tabNames = new List<string>();
            _tabElements = new Dictionary<string, TriElement>();
            _activeTabName = null;
        }

        protected override void DrawHeader(Rect position)
        {
            if (_tabNames.Count == 0)
            {
                return;
            }

            var tabRect = new Rect(position)
            {
                width = position.width / _tabNames.Count,
            };

            if (_tabNames.Count == 1)
            {
                GUI.Toggle(tabRect, true, _tabNames[0], TriEditorStyles.TabOnlyOne);
            }
            else
            {
                for (int index = 0, tabCount = _tabNames.Count; index < tabCount; index++)
                {
                    var tabName = _tabNames[index];
                    var tabStyle = index == 0 ? TriEditorStyles.TabFirst
                        : index == tabCount - 1 ? TriEditorStyles.TabLast
                        : TriEditorStyles.TabMiddle;

                    var isTabActive = GUI.Toggle(tabRect, _activeTabName == tabName, tabName, tabStyle);
                    if (isTabActive && _activeTabName != tabName)
                    {
                        SetActiveTab(tabName);
                    }

                    tabRect.x += tabRect.width;
                }
            }
        }

        protected override void AddPropertyChild(TriElement element, TriProperty property)
        {
            var tabName = DefaultTabName;

            if (property.TryGetAttribute(out TabAttribute tab))
            {
                tabName = tab.TabName ?? tabName;
            }

            if (!_tabElements.TryGetValue(tabName, out var tabElement))
            {
                tabElement = new TriElement();

                _tabElements[tabName] = tabElement;
                _tabNames.Add(tabName);

                if (_activeTabName == null)
                {
                    SetActiveTab(tabName);
                }
            }

            tabElement.AddChild(element);
        }

        private void SetActiveTab(string tabName)
        {
            _activeTabName = tabName;

            RemoveAllChildren();

            AddChild(_tabElements[_activeTabName]);
        }
    }
}