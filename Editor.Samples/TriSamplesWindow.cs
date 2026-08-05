using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector.Editors;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Editor.Samples
{
    internal class TriSamplesWindow : EditorWindow
    {
        private readonly List<Type> _sampleTypes = new List<Type>();

        private ScriptableObject _current;
        private UnityEditor.Editor _currentEditor;
        private MonoScript _currentMonoScript;

        private TreeView _menuTree;
        private VisualElement _detailContainer;

        [MenuItem("Tools/Tri Inspector/Samples")]
        public static void Open()
        {
            var window = GetWindow<TriSamplesWindow>();
            window.titleContent = new GUIContent("Tri Samples");
            window.Show();
        }

        private void CreateGUI()
        {
            CollectSampleTypes();

            var root = rootVisualElement;
            TriStyleSheet.ApplyTo(root);
            root.AddToClassList(EditorGUIUtility.isProSkin ? "tri-dark" : "tri-light");
            root.AddToClassList(TriStyles.Samples);

            var leftPane = new VisualElement();
            leftPane.AddToClassList(TriStyles.SamplesMenu);
            root.Add(leftPane);

            var searchField = new ToolbarSearchField();
            searchField.AddToClassList(TriStyles.SamplesSearch);
            searchField.RegisterValueChangedCallback(evt => RebuildMenu(evt.newValue));
            leftPane.Add(searchField);

            _menuTree = new TreeView
            {
                fixedItemHeight = 20,
                selectionType = SelectionType.Single,
                makeItem = MakeTreeItem,
            };
            _menuTree.AddToClassList(TriStyles.SamplesTree);
            _menuTree.bindItem = (element, index) =>
                ((Label) element).text = _menuTree.GetItemDataForIndex<MenuEntry>(index).Name;
            _menuTree.selectionChanged += OnMenuSelectionChanged;
            leftPane.Add(_menuTree);

            var rightPane = new ScrollView();
            rightPane.AddToClassList(TriStyles.SamplesDetailScroll);
            root.Add(rightPane);

            _detailContainer = new VisualElement();
            _detailContainer.AddToClassList(TriStyles.SamplesDetail);
            rightPane.Add(_detailContainer);

            RebuildMenu(string.Empty);
        }

        private static VisualElement MakeTreeItem()
        {
            var label = new Label();
            label.AddToClassList(TriStyles.SamplesTreeItem);
            return label;
        }

        private void OnDisable()
        {
            ChangeCurrentSample(null);
        }

        private void CollectSampleTypes()
        {
            _sampleTypes.Clear();
            _sampleTypes.AddRange(typeof(TriSamplesWindow).Assembly.GetTypes()
                .Where(type => type.BaseType == typeof(ScriptableObject) && type.Name.EndsWith("Sample"))
                .OrderBy(type => type.Name));
        }

        private void RebuildMenu(string search)
        {
            var hasSearch = !string.IsNullOrEmpty(search);

            var groups = new List<KeyValuePair<string, List<Type>>>();
            var groupLookup = new Dictionary<string, List<Type>>();

            foreach (var type in _sampleTypes)
            {
                if (hasSearch &&
                    GetTypeNiceName(type).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var group = type.Name.Split('_')[0];
                if (!groupLookup.TryGetValue(group, out var list))
                {
                    groupLookup[group] = list = new List<Type>();
                    groups.Add(new KeyValuePair<string, List<Type>>(group, list));
                }

                list.Add(type);
            }

            var id = 0;
            var roots = new List<TreeViewItemData<MenuEntry>>();
            foreach (var group in groups)
            {
                var children = new List<TreeViewItemData<MenuEntry>>();
                foreach (var type in group.Value)
                {
                    children.Add(new TreeViewItemData<MenuEntry>(
                        id++, new MenuEntry(GetTypeNiceName(type), type)));
                }

                roots.Add(new TreeViewItemData<MenuEntry>(
                    id++, new MenuEntry(group.Key, null), children));
            }

            _menuTree.SetRootItems(roots);
            _menuTree.Rebuild();

            if (hasSearch)
            {
                _menuTree.ExpandAll();
            }
        }

        private void OnMenuSelectionChanged(IEnumerable<object> selection)
        {
            var type = selection.OfType<MenuEntry>().Select(entry => entry.Type).FirstOrDefault();
            ChangeCurrentSample(type);
        }

        private void ChangeCurrentSample(Type type)
        {
            if (_current != null)
            {
                DestroyImmediate(_current);
                _current = null;
            }

            if (_currentEditor != null)
            {
                DestroyImmediate(_currentEditor);
                _currentEditor = null;
            }

            _currentMonoScript = null;

            _detailContainer?.Clear();

            if (type == null)
            {
                return;
            }

            _current = CreateInstance(type);
            _current.name = GetTypeNiceName(type);
            _current.hideFlags = HideFlags.DontSave;

            _currentEditor = UnityEditor.Editor.CreateEditor(_current);
            _currentMonoScript = MonoScript.FromScriptableObject(_current);

            BuildDetail();
        }

        private void BuildDetail()
        {
            var header = new Label(_current.name);
            header.AddToClassList(TriStyles.SamplesHeader);
            _detailContainer.Add(header);

            if (_currentEditor.GetType() != typeof(TriScriptableObjectEditor))
            {
                _detailContainer.Add(new HelpBox(
                    "Detected third party asset that overrides all inspectors. " +
                    "Tri-Inspector's attributes might not work\n" +
                    _currentEditor.GetType().FullName, HelpBoxMessageType.Error));
            }

            _detailContainer.Add(CreateSectionLabel("Preview"));
            var previewBox = CreateBox();
            previewBox.Add(new InspectorElement(_currentEditor));
            _detailContainer.Add(previewBox);

            _detailContainer.Add(CreateSectionLabel("Code"));
            var codeBox = CreateBox();
            codeBox.Add(new TextField
            {
                multiline = true,
                isReadOnly = true,
                value = _currentMonoScript.text,
            });
            _detailContainer.Add(codeBox);
        }

        private static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList(TriStyles.SamplesSection);
            return label;
        }

        private static VisualElement CreateBox()
        {
            var box = new VisualElement();
            box.AddToClassList(TriStyles.SamplesBox);
            return box;
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

        private readonly struct MenuEntry
        {
            public readonly string Name;
            public readonly Type Type;

            public MenuEntry(string name, Type type)
            {
                Name = name;
                Type = type;
            }
        }
    }
}
