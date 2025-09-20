using System.Collections.Generic;
using TriInspector.Resolvers;
using UnityEngine;
using UnityEditor;

namespace TriInspector.Elements
{
    public class TriTabGroupElement : TriHeaderGroupBaseElement
    {
        private const string DefaultTabName = "Main";
        const float minTabWidth = 80f;
        const float tabHeight = 20f;
        private readonly List<TabInfo> _tabs;
        private readonly Dictionary<string, TriElement> _tabElements;
        private string _activeTabName;
        private string _tabGroupId;
        private bool _isInitializing;
        private bool _activeTabLoaded = false;
        
        // Enhanced caching for performance
        private float[] _cachedTabWidths;
        private List<List<int>> _cachedRows;
        private float _cachedLayoutWidth = -1f;
        private float _cachedLayoutHeight = -1f;
        private bool _tabContentDirty = true;
        
        // Reusable objects to avoid allocations
        private readonly List<int> _reusableRowList = new List<int>();
        private readonly GUIContent _reusableGUIContent = new GUIContent();
        
        // Throttling for resize events
        private float _lastWidthChangeTime;
        private const float WidthChangeThrottleTime = 0.016f; // ~60fps throttling
        private float _pendingWidth = -1f;
        
        // Significant width change thresholds
        private const float MinWidthChangeThreshold = 5f; // Minimum pixels to trigger recalc
        private const float RelativeWidthChangeThreshold = 0.02f; // 2% relative change

        private struct TabInfo
        {
            public string name;
            public ValueResolver<string> titleResolver;
            public TriProperty property;
        }

        public TriTabGroupElement()
        {
            _tabs = new List<TabInfo>();
            _tabElements = new Dictionary<string, TriElement>();
            _activeTabName = null;
            _tabGroupId = "";
            _cachedTabWidths = null;
            _cachedRows = null;
        }

        private string GetTabGroupPreferenceKey()
        {
            if (string.IsNullOrEmpty(_tabGroupId) && _tabs.Count > 0 && _tabs[0].property != null)
            {
                var property = _tabs[0].property;
                var rootProperty = property;
                while (rootProperty.Parent != null)
                {
                    rootProperty = rootProperty.Parent;
                }

                var targetObject = rootProperty.Value as Object;
                if (targetObject != null)
                {
                    _tabGroupId = $"TriTabGroup_{targetObject.GetInstanceID()}_{property.PropertyPath}";
                }
            }
            return _tabGroupId;
        }

        private void SaveActiveTab()
        {
            if (_isInitializing) return;

            var preferenceKey = GetTabGroupPreferenceKey();
            if (!string.IsNullOrEmpty(preferenceKey) && !string.IsNullOrEmpty(_activeTabName))
            {
                EditorPrefs.SetString(preferenceKey, _activeTabName);
            }
        }

        private bool LoadActiveTab()
        {
            var preferenceKey = GetTabGroupPreferenceKey();
            if (!string.IsNullOrEmpty(preferenceKey))
            {
                var savedTab = EditorPrefs.GetString(preferenceKey, null);
                if (!string.IsNullOrEmpty(savedTab) && _tabElements.ContainsKey(savedTab))
                {
                    SetActiveTabInternal(savedTab);
                    return true;
                }
            }
            return false;
        }

        private void InvalidateLayoutCache()
        {
            _tabContentDirty = true;
            _cachedLayoutWidth = -1f;
            _cachedLayoutHeight = -1f;
            _cachedTabWidths = null;
            
            // Don't immediately clear _cachedRows - let it be reused if possible
            if (_cachedRows != null)
            {
                // Clear existing rows but keep the list structure for reuse
                foreach (var row in _cachedRows)
                {
                    row.Clear();
                }
            }
        }

        private bool NeedsTabContentRecalculation()
        {
            if (_tabContentDirty)
                return true;
                
            // Check if any tab titles have changed (for dynamic titles)
            if (_cachedTabWidths != null && _cachedTabWidths.Length == _tabs.Count)
            {
                for (int i = 0; i < _tabs.Count; i++)
                {
                    var content = _tabs[i].titleResolver.GetValue(_tabs[i].property);
                    
                    // Reuse GUIContent object to avoid allocation
                    _reusableGUIContent.text = content;
                    var contentSize = GUI.skin.button.CalcSize(_reusableGUIContent);
                    var expectedWidth = Mathf.Max(contentSize.x + 20f, minTabWidth);
                    
                    if (Mathf.Abs(_cachedTabWidths[i] - expectedWidth) > 0.1f)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private void UpdateTabWidthsIfNeeded()
        {
            if (!NeedsTabContentRecalculation())
                return;

            // Reuse array if possible
            if (_cachedTabWidths == null || _cachedTabWidths.Length != _tabs.Count)
            {
                _cachedTabWidths = new float[_tabs.Count];
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                var content = _tabs[i].titleResolver.GetValue(_tabs[i].property);
                
                // Reuse GUIContent object to avoid allocation
                _reusableGUIContent.text = content;
                var contentSize = GUI.skin.button.CalcSize(_reusableGUIContent);
                _cachedTabWidths[i] = Mathf.Max(contentSize.x + 20f, minTabWidth);
            }

            _tabContentDirty = false;
            // Invalidate layout cache since tab widths changed
            _cachedLayoutWidth = -1f;
            
            // Clear rows but don't deallocate
            if (_cachedRows != null)
            {
                foreach (var row in _cachedRows)
                {
                    row.Clear();
                }
                _cachedRows.Clear();
            }
        }

        private bool IsWidthChangeSignificant(float newWidth, float cachedWidth)
        {
            if (cachedWidth < 0) return true; // No cached width
            
            var absoluteDiff = Mathf.Abs(newWidth - cachedWidth);
            
            // Must exceed minimum pixel threshold
            if (absoluteDiff < MinWidthChangeThreshold) return false;
            
            // AND must exceed relative threshold for larger widths
            var relativeDiff = absoluteDiff / Mathf.Max(cachedWidth, 1f);
            return relativeDiff >= RelativeWidthChangeThreshold;
        }

        private List<List<int>> GetRowsForWidth(float availableWidth)
        {
            // Throttle width changes to avoid excessive recalculations
            if (_pendingWidth != availableWidth)
            {
                _pendingWidth = availableWidth;
                _lastWidthChangeTime = Time.realtimeSinceStartup;
            }
            
            // If we're in a throttle period and have cached data, use it
            if (Time.realtimeSinceStartup - _lastWidthChangeTime < WidthChangeThrottleTime && 
                _cachedRows != null && 
                !IsWidthChangeSignificant(availableWidth, _cachedLayoutWidth))
            {
                return _cachedRows;
            }

            UpdateTabWidthsIfNeeded();
            
            // Return cached layout if width hasn't changed significantly
            if (!IsWidthChangeSignificant(availableWidth, _cachedLayoutWidth) && 
                _cachedRows != null && _cachedRows.Count > 0)
            {
                return _cachedRows;
            }
            
            // Initialize or reuse the cached rows structure
            if (_cachedRows == null)
            {
                _cachedRows = new List<List<int>>();
            }
            else
            {
                // Clear existing rows but reuse the lists
                foreach (var row in _cachedRows)
                {
                    row.Clear();
                }
                _cachedRows.Clear();
            }
            
            // Reuse the temporary row list
            _reusableRowList.Clear();
            var currentRowWidth = 0f;
            var availableRowsIndex = 0;
            
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (currentRowWidth + _cachedTabWidths[i] > availableWidth && _reusableRowList.Count > 0)
                {
                    // Reuse existing row list if available, otherwise create new
                    List<int> rowToAdd;
                    if (availableRowsIndex < _cachedRows.Count)
                    {
                        rowToAdd = _cachedRows[availableRowsIndex];
                    }
                    else
                    {
                        rowToAdd = new List<int>();
                        _cachedRows.Add(rowToAdd);
                    }
                    
                    // Copy items from reusable list
                    rowToAdd.AddRange(_reusableRowList);
                    availableRowsIndex++;
                    
                    _reusableRowList.Clear();
                    currentRowWidth = 0f;
                }
                
                _reusableRowList.Add(i);
                currentRowWidth += _cachedTabWidths[i];
            }
            
            if (_reusableRowList.Count > 0)
            {
                List<int> finalRow;
                if (availableRowsIndex < _cachedRows.Count)
                {
                    finalRow = _cachedRows[availableRowsIndex];
                }
                else
                {
                    finalRow = new List<int>();
                    _cachedRows.Add(finalRow);
                }
                
                finalRow.AddRange(_reusableRowList);
            }
            
            _cachedLayoutWidth = availableWidth;
            return _cachedRows;
        }

        private (float[] tabWidths, List<List<int>> rows) CalculateTabLayout(float availableWidth)
        {
            var rows = GetRowsForWidth(availableWidth);
            return (_cachedTabWidths, rows);
        }

        protected override float GetHeaderHeight(float width)
        {
            if (_tabs.Count <= 1)
            {
                return 20f; // Single row height
            }

            // Return cached height if width hasn't changed significantly
            if (!IsWidthChangeSignificant(width, _cachedLayoutWidth) && _cachedLayoutHeight > 0f)
            {
                return _cachedLayoutHeight;
            }

            var rows = GetRowsForWidth(width);
            _cachedLayoutHeight = rows.Count * tabHeight;

            return _cachedLayoutHeight;
        }

        protected override void DrawHeader(Rect position)
        {
            if (_tabs.Count == 0)
            {
                return;
            }

            // Load saved tab state if no active tab is set
            if (!_activeTabLoaded)
            {
                _activeTabLoaded = true;
                LoadActiveTab();
            }

            if (_tabs.Count == 1)
            {
                var tab = _tabs[0];
                var content = tab.titleResolver.GetValue(tab.property);
                var tabRect = new Rect(position) { width = position.width };
                GUI.Toggle(tabRect, true, content, TriEditorStyles.TabOnlyOne);
            }
            else
            {
                DrawMultiRowTabs(position);
            }
        }

        private void DrawMultiRowTabs(Rect position)
        {
            var (tabWidths, rows) = CalculateTabLayout(position.width);
            var availableWidth = position.width;

            // Draw tabs row by row
            var currentY = position.y;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var rowRect = new Rect(position.x, currentY, position.width, tabHeight);

                // Calculate actual widths for tabs in this row (distribute remaining space)
                var totalRowWidth = 0f;
                foreach (var tabIndex in row)
                {
                    totalRowWidth += tabWidths[tabIndex];
                }

                var extraSpace = Mathf.Max(0, availableWidth - totalRowWidth);
                var extraPerTab = row.Count > 0 ? extraSpace / row.Count : 0f;

                var currentX = position.x;

                for (int i = 0; i < row.Count; i++)
                {
                    var tabIndex = row[i];
                    var tab = _tabs[tabIndex];
                    var content = tab.titleResolver.GetValue(tab.property);
                    var tabWidth = tabWidths[tabIndex] + extraPerTab;

                    var tabRect = new Rect(currentX, currentY, tabWidth, tabHeight);

                    // Determine tab style based on position in the entire tab group
                    GUIStyle tabStyle;
                    if (_tabs.Count == 1)
                    {
                        tabStyle = TriEditorStyles.TabOnlyOne;
                    }
                    else if (tabIndex == 0)
                    {
                        tabStyle = TriEditorStyles.TabFirst;
                    }
                    else if (tabIndex == _tabs.Count - 1)
                    {
                        tabStyle = TriEditorStyles.TabLast;
                    }
                    else
                    {
                        tabStyle = TriEditorStyles.TabMiddle;
                    }

                    var isTabActive = GUI.Toggle(tabRect, _activeTabName == tab.name, content, tabStyle);
                    if (isTabActive && _activeTabName != tab.name)
                    {
                        SetActiveTab(tab.name);
                    }

                    currentX += tabWidth;
                }

                currentY += tabHeight;
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
                var info = new TabInfo
                {
                    name = tabName,
                    titleResolver = ValueResolver.ResolveString(property.Definition, tabName),
                    property = property,
                };
                _tabElements[tabName] = tabElement;
                _tabs.Add(info);
                
                // Mark cache as dirty when tabs are added
                InvalidateLayoutCache();

                if (info.titleResolver.TryGetErrorString(out var error))
                {
                    tabElement.AddChild(new TriInfoBoxElement(error, TriMessageType.Error));
                }

                if (_activeTabName == null)
                {
                    _isInitializing = true;
                    if (!LoadActiveTab())
                    {
                        SetActiveTabInternal(tabName);
                    }
                    _isInitializing = false;
                }
            }

            tabElement.AddChild(element);
        }

        private void SetActiveTabInternal(string tabName)
        {
            _activeTabName = tabName;
            RemoveAllChildren();
            AddChild(_tabElements[_activeTabName]);
        }

        private void SetActiveTab(string tabName)
        {
            SetActiveTabInternal(tabName);
            SaveActiveTab();
        }
    }
}