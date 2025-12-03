using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector.Editors;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#if UNITY_6000_2_OR_NEWER
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace TriInspector.Editor.Samples
{
    internal class TriSamplesWindow : EditorWindow
    {
        private MenuTree _menuTree;
        private SearchField _searchField;

        private ScriptableObject _current;
        private UnityEditor.Editor _currentEditor;
        private MonoScript _currentMonoScript;
        private Vector2 _currentScroll;

        [MenuItem("Tools/Tri Inspector/Samples")]
        public static void Open()
        {
            var window = GetWindow<TriSamplesWindow>();
            window.titleContent = new GUIContent("Tri Samples");
            window.Show();
        }

        private void OnEnable()
        {
            _menuTree = new MenuTree(new TreeViewState());
            _menuTree.SelectedTypeChanged += ChangeCurrentSample;

            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _menuTree.SetFocusAndEnsureSelectedItem;

            _menuTree.Reload();
        }

        private void OnDisable()
        {
            ChangeCurrentSample(null);
        }

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    DrawMenu();
                }

                var separatorRect = GUILayoutUtility.GetLastRect();
                separatorRect.xMin = separatorRect.xMax;
                separatorRect.xMax += 1;
                GUI.Box(separatorRect, "");

                using (new GUILayout.VerticalScope())
                {
                    DrawElement();
                }
            }
        }

        private void DrawMenu()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUILayout.Space(5);
                _menuTree.searchString = _searchField.OnToolbarGUI(_menuTree.searchString, GUILayout.ExpandWidth(true));
                GUILayout.Space(5);
            }

            var menuRect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _menuTree.OnGUI(menuRect);
        }

        private void DrawElement()
        {
            if (_currentEditor == null || _currentMonoScript == null)
            {
                return;
            }

            using (var scrollScope = new GUILayout.ScrollViewScope(_currentScroll))
            {
                _currentScroll = scrollScope.scrollPosition;

                using (new GUILayout.VerticalScope(SampleWindowStyles.Padding))
                {
                    GUILayout.Label(_current.name, SampleWindowStyles.HeaderDisplayNameLabel);

                    if (_currentEditor.GetType() != typeof(TriScriptableObjectEditor))
                    {
                        EditorGUILayout.HelpBox(
                            "Detected third party asset that overrides all inspectors. Tri-Inspector's attributes might not work\n" +
                            _currentEditor.GetType().FullName, MessageType.Error);
                    }

                    GUILayout.Space(10);
                    GUILayout.Label("Preview", EditorStyles.boldLabel);

                    using (new GUILayout.VerticalScope(SampleWindowStyles.BoxWithPadding))
                    {
                        _currentEditor.OnInspectorGUI();
                    }

                    GUILayout.Space(10);
                    GUILayout.Label("Code", EditorStyles.boldLabel);

                    using (new GUILayout.VerticalScope(SampleWindowStyles.BoxWithPadding))
                    {
                        GUILayout.TextField(_currentMonoScript.text);
                    }
                }
            }
        }

        private void ChangeCurrentSample(Type type)
        {
            if (_current != null)
            {
                DestroyImmediate(_current);
                _current = null;
            }

            DestroyImmediate(_currentEditor);

            _currentScroll = Vector2.zero;

            if (type != null)
            {
                _current = CreateInstance(type);
                _current.name = GetTypeNiceName(type);
                _current.hideFlags = HideFlags.DontSave;

                _currentEditor = UnityEditor.Editor.CreateEditor(_current);
                _currentMonoScript = MonoScript.FromScriptableObject(_current);
            }
        }

        private static string GetTypeNiceName(Type type)
        {
            var name = type.Name;

            if (name.Contains('_'))
            {
                var index = name.IndexOf('_');
                name = name.Substring(index + 1);
            }

            if (name.EndsWith("Sample"))
            {
                name = name.Remove(name.Length - "Sample".Length);
            }

            return name;
        }

        private class MenuTree : TreeView
        {
            private readonly Dictionary<string, GroupItem> _groups = new Dictionary<string, GroupItem>();

            public event Action<Type> SelectedTypeChanged;

            public MenuTree(TreeViewState state) : base(state)
            {
            }

            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                base.SelectionChanged(selectedIds);

                var type = selectedIds.Count > 0 && FindItem(selectedIds[0], rootItem) is SampleItem sampleItem
                    ? sampleItem.Type
                    : null;

                SelectedTypeChanged?.Invoke(type);
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(-1, -1);

                var sampleTypes = typeof(TriSamplesWindow).Assembly.GetTypes()
                    .Where(type => type.BaseType == typeof(ScriptableObject) && type.Name.EndsWith("Sample"))
                    .OrderBy(type => type.Name)
                    .ToList();

                var id = 0;
                foreach (var sampleType in sampleTypes)
                {
                    var group = sampleType.Name.Split('_')[0];

                    if (!_groups.TryGetValue(group, out var groupItem))
                    {
                        _groups[group] = groupItem = new GroupItem(++id, group);

                        root.AddChild(groupItem);
                    }

                    groupItem.AddChild(new SampleItem(++id, sampleType));
                }

                return root;
            }

            private class GroupItem : TreeViewItem
            {
                public GroupItem(int id, string name) : base(id, 0, name)
                {
                }
            }

            private class SampleItem : TreeViewItem
            {
                public Type Type { get; }

                public SampleItem(int id, Type type) : base(id, 1, GetTypeNiceName(type))
                {
                    Type = type;
                }
            }
        }
    }
}