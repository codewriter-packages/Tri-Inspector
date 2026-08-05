using System.Collections.Generic;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriTabGroupVisualElement : TriPropertyCollectionVisualElement
    {
        private const string DefaultTabName = "Main";

        private readonly VisualElement _tabBar;
        private readonly VisualElement _content;
        private readonly Dictionary<int, VisualElement> _rows = new Dictionary<int, VisualElement>();
        private readonly Dictionary<string, VisualElement> _tabContents = new Dictionary<string, VisualElement>();
        private readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        private readonly List<TabInfo> _tabs = new List<TabInfo>();

        private string _activeTab;
        private string _activeTabKey;

        private struct TabInfo
        {
            public string name;
            public TriProperty property;
            public ValueResolver<string> titleResolver;
        }

        public TriTabGroupVisualElement()
        {
            AddToClassList(TriStyles.TabGroup);

            _tabBar = new VisualElement();

            _content = new VisualElement();
            _content.AddToClassList(TriStyles.TabGroupContent);

            Add(_tabBar);
            Add(_content);

            this.PeriodicRun(SyncTitles);
        }

        protected override void AddPropertyChild(VisualElement child, TriProperty property)
        {
            var tabName = DefaultTabName;
            var row = 0;

            if (property.TryGetAttribute(out TabAttribute tab))
            {
                tabName = tab.TabName ?? tabName;
                row = tab.Row;
            }

            if (!_tabContents.TryGetValue(tabName, out var tabContent))
            {
                tabContent = new VisualElement();
                _tabContents.Add(tabName, tabContent);
                _content.Add(tabContent);

                var titleResolver = ValueResolver.ResolveString(property.Definition, tabName);
                _tabs.Add(new TabInfo {name = tabName, property = property, titleResolver = titleResolver});

                if (titleResolver.TryGetErrorString(out var error))
                {
                    tabContent.Add(new TriInfoBoxVisualElement(error, TriMessageType.Error));
                }

                var capturedName = tabName;
                var button = new Button(() => SetActiveTab(capturedName))
                {
                    text = titleResolver.GetValue(property),
                };
                button.AddToClassList(TriStyles.TabGroupButton);
                _tabButtons.Add(tabName, button);

                var rowElement = GetRow(row);
                rowElement.Add(button);
                UpdateRowEdges(rowElement, row);

                if (_activeTabKey == null && property.TryGetAttribute(out GroupAttribute groupAttribute))
                {
                    _activeTabKey =
                        $"TriInspector.tab_group.{property.PropertyTree.TargetObjectType.Name}.{groupAttribute.Path}.active";
                    _activeTab = SessionState.GetString(_activeTabKey, null);
                }

                if (string.IsNullOrEmpty(_activeTab) || _activeTab == tabName)
                {
                    SetActiveTab(tabName);
                }
            }

            tabContent.Add(child);
        }

        private VisualElement GetRow(int row)
        {
            if (!_rows.TryGetValue(row, out var rowElement))
            {
                rowElement = new VisualElement();
                rowElement.AddToClassList(TriStyles.TabGroupRow);
                _rows.Add(row, rowElement);

                // Keep rows ordered by their declared row index?
                var insertIndex = 0;
                foreach (var key in _rows.Keys)
                {
                    if (key < row)
                    {
                        insertIndex++;
                    }
                }

                _tabBar.Insert(insertIndex, rowElement);
            }

            return rowElement;
        }

        // Round the outer top corners of the row: top-left on the first tab, top-right on the last.
        private static void UpdateRowEdges(VisualElement rowElement, int row)
        {
            var count = rowElement.childCount;

            for (var i = 0; i < count; i++)
            {
                var button = rowElement.ElementAt(i);
                button.EnableInClassList(TriStyles.TabGroupButtonFirst, i == 0);
                button.EnableInClassList(TriStyles.TabGroupButtonLast, i == count - 1);
            }
        }

        private void SetActiveTab(string tabName)
        {
            _activeTab = tabName;

            if (_activeTabKey != null)
            {
                SessionState.SetString(_activeTabKey, tabName);
            }

            foreach (var pair in _tabContents)
            {
                pair.Value.style.display = pair.Key == tabName ? DisplayStyle.Flex : DisplayStyle.None;
            }

            foreach (var pair in _tabButtons)
            {
                pair.Value.EnableInClassList(TriStyles.TabGroupButtonActive, pair.Key == tabName);
                pair.Value.EnableInClassList(TriStyles.TabGroupButtonNonActive, pair.Key != tabName);
            }
        }

        private void SyncTitles()
        {
            foreach (var tab in _tabs)
            {
                if (_tabButtons.TryGetValue(tab.name, out var button))
                {
                    button.text = tab.titleResolver.GetValue(tab.property);
                }
            }
        }
    }
}