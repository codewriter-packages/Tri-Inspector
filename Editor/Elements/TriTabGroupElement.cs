using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriTabGroupElement : TriHeaderGroupBaseElement
    {
        private const string DefaultTabName = "Main";

        private readonly List<TabInfo> _tabs;
        private readonly Dictionary<string, TriElement> _tabElements;
        private string _activeTabNameKey;
        private string _activeTabName;
        private Dictionary<int, (int realRow, int rowCount)> _rowToInfo;
        private int[] _rowCounts;

        private struct TabInfo
        {
            public string name;
            public int row;
            public ValueResolver<string> titleResolver;
            public TriProperty property;
        }

        public TriTabGroupElement()
        {
            _tabs = new List<TabInfo>();
            _tabElements = new Dictionary<string, TriElement>();
            _activeTabName = null;
        }

        protected override void DrawHeader(Rect position)
        {
            if (_tabs.Count == 0)
            {
                return;
            }

            if (_tabs.Count == 1)
            {
                var tab = _tabs[0];
                var content = tab.titleResolver.GetValue(tab.property);
                GUI.Toggle(position, true, content, TriEditorStyles.TabOnlyOne);
            }
            else
            {
                if (_rowToInfo == null)
                {
                    _rowToInfo = _tabs
                        .GroupBy(t => t.row)
                        .OrderBy(g => g.Key)
                        .Select((g, index) => (
                            row: g.Key,
                            realRow: index,
                            rowCount: g.Count()
                        ))
                        .ToDictionary(
                            x => x.row,
                            x => (x.realRow, x.rowCount)
                        );
                    _rowCounts = new int[_rowToInfo.Count];
                    foreach (var (_, (realRow, count)) in _rowToInfo)
                        _rowCounts[realRow] = count;
                }

                Span<Rect> tab_rects = stackalloc Rect[_rowToInfo.Count];
                for (int i = 0; i < tab_rects.Length; i++)
                {
                    tab_rects[i] = new Rect(
                        position.x,
                        position.y + base.GetHeaderHeight(0) * i,
                        position.width / _rowCounts[i],
                        base.GetHeaderHeight(0)
                    );
                }

                for (int index = 0, tabCount = _tabs.Count; index < tabCount; index++)
                {
                    var tab = _tabs[index];
                    var (realRow, rowCount) = _rowToInfo[tab.row];
                    var content = tab.titleResolver.GetValue(tab.property);
                    var tabStyle = index == 0 ? TriEditorStyles.TabFirst
                        : index == rowCount - 1 ? TriEditorStyles.TabLast
                        : TriEditorStyles.TabMiddle;

                    var isTabActive = GUI.Toggle(tab_rects[realRow], _activeTabName == tab.name, content, tabStyle);
                    if (isTabActive && _activeTabName != tab.name)
                    {
                        SetActiveTab(tab.name);
                    }

                    tab_rects[realRow].x += tab_rects[realRow].width;
                }
            }
        }

        protected override float GetHeaderHeight(float width)
        {
            return base.GetHeaderHeight(width) * _rowToInfo?.Count ?? 1;
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

                var info = new TabInfo
                {
                    name = tabName,
                    row = tab.Row,
                    titleResolver = ValueResolver.ResolveString(property.Definition, tabName),
                    property = property,
                };

                _tabElements[tabName] = tabElement;
                _tabs.Add(info);

                if (info.titleResolver.TryGetErrorString(out var error))
                {
                    tabElement.AddChild(new TriInfoBoxElement(error, TriMessageType.Error));
                }

                if (_activeTabNameKey == null && info.property.TryGetAttribute(out GroupAttribute groupAttribute))
                {
                    _activeTabNameKey = $"TriInspector.tab_grouop.{info.property.PropertyTree.TargetObjectType}.{groupAttribute.Path}.active";
                    _activeTabName = SessionState.GetString(_activeTabNameKey, null);
                }
                if (string.IsNullOrEmpty(_activeTabName) || _activeTabName == tabName)
                {
                    SetActiveTab(tabName);
                }
            }

            tabElement.AddChild(element);
        }

        private void SetActiveTab(string tabName)
        {
            _activeTabName = tabName;
            if (_activeTabNameKey != null)
                SessionState.SetString(_activeTabNameKey, tabName);

            RemoveAllChildren();

            AddChild(_tabElements[_activeTabName]);
        }
    }
}