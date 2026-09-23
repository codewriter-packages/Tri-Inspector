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
        private readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private readonly Dictionary<string, int> _groupToId = new Dictionary<string, int>();

        [SerializeField] private string selectedSampleType;
        [SerializeField] private string selectedCategory;

        private ScriptableObject _current;
        private UnityEditor.Editor _currentEditor;
        private TriEditorCore _currentPreview;
        private MonoScript _currentMonoScript;
        private readonly List<ScriptableObject> _categoryInstances = new List<ScriptableObject>();

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
            root.AddToClassList(Styles.Samples);

            var leftPane = new VisualElement();
            leftPane.AddToClassList(Styles.SamplesMenu);
            root.Add(leftPane);

            var searchRow = new VisualElement();
            searchRow.AddToClassList(Styles.SamplesSearchRow);

            var searchField = new ToolbarSearchField();
            searchField.AddToClassList(Styles.SamplesSearch);
            searchField.RegisterValueChangedCallback(evt => RebuildMenu(evt.newValue));
            searchRow.Add(searchField);

            leftPane.Add(searchRow);

            _menuTree = new TreeView
            {
                fixedItemHeight = 20,
                selectionType = SelectionType.Single,
                makeItem = MakeTreeItem,
            };
            _menuTree.AddToClassList(Styles.SamplesTree);
            _menuTree.bindItem = (element, index) =>
                ((Label) element).text = _menuTree.GetItemDataForIndex<MenuEntry>(index).Name;
            _menuTree.selectionChanged += OnMenuSelectionChanged;
            leftPane.Add(_menuTree);

            var rightPane = new ScrollView();
            rightPane.AddToClassList(Styles.SamplesDetailScroll);
            root.Add(rightPane);

            _detailContainer = new VisualElement();
            _detailContainer.AddToClassList(Styles.SamplesDetail);
            rightPane.Add(_detailContainer);

            RebuildMenu(string.Empty);
            RestoreSelection();
        }

        private void RestoreSelection()
        {
            var id = -1;

            if (!string.IsNullOrEmpty(selectedSampleType) &&
                Type.GetType(selectedSampleType) is { } type &&
                _typeToId.TryGetValue(type, out var sampleId))
            {
                id = sampleId;

                var group = type.Name.Split('_')[0];
                if (_groupToId.TryGetValue(group, out var groupId))
                {
                    _menuTree.ExpandItem(groupId);
                }
            }
            else if (!string.IsNullOrEmpty(selectedCategory) &&
                     _groupToId.TryGetValue(selectedCategory, out var categoryId))
            {
                id = categoryId;
            }

            if (id < 0)
            {
                ShowIntro();
                return;
            }

            // Clear so SetSelectionById is treated as a new selection and rebuilds
            // the detail (OnMenuSelectionChanged skips already-shown entries).
            selectedSampleType = null;
            selectedCategory = null;

            _menuTree.SetSelectionById(id);
            _menuTree.ScrollToItemById(id);
        }

        private static VisualElement MakeTreeItem()
        {
            var label = new Label();
            label.AddToClassList(Styles.SamplesTreeItem);
            return label;
        }

        private void OnDisable()
        {
            ClearCurrent();
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

            _typeToId.Clear();
            _groupToId.Clear();

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
                    _typeToId[type] = id;
                    children.Add(new TreeViewItemData<MenuEntry>(
                        id++, new MenuEntry(GetTypeNiceName(type), type)));
                }

                _groupToId[group.Key] = id;
                roots.Add(new TreeViewItemData<MenuEntry>(
                    id++, new MenuEntry(group.Key, null, group.Value), children));
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
            var entries = selection.OfType<MenuEntry>().ToList();
            if (entries.Count == 0)
            {
                ShowSample(null);
                return;
            }

            var entry = entries[0];
            if (entry.Type != null)
            {
                if (entry.Type.AssemblyQualifiedName == selectedSampleType)
                {
                    return;
                }

                ShowSample(entry.Type);
            }
            else
            {
                if (entry.Name == selectedCategory)
                {
                    return;
                }

                ShowCategory(entry);
            }
        }

        private void ClearCurrent()
        {
            if (_currentPreview != null)
            {
                _currentPreview.Dispose();
                _currentPreview = null;
            }

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

            foreach (var instance in _categoryInstances)
            {
                if (instance != null)
                {
                    DestroyImmediate(instance);
                }
            }

            _categoryInstances.Clear();

            _detailContainer?.Clear();
        }

        private void ShowSample(Type type)
        {
            ClearCurrent();

            selectedSampleType = type?.AssemblyQualifiedName;
            selectedCategory = null;

            if (type == null)
            {
                ShowIntro();
                return;
            }

            _current = CreateInstance(type);
            _current.name = GetTypeNiceName(type);
            _current.hideFlags = HideFlags.DontSave;

            _currentEditor = UnityEditor.Editor.CreateEditor(_current);
            _currentMonoScript = MonoScript.FromScriptableObject(_current);

            BuildSampleDetail();
        }

        private void ShowIntro()
        {
            ClearCurrent();

            var intro = new VisualElement();
            intro.AddToClassList(Styles.SamplesIntro);

            var title = new Label("Welcome to Tri Inspector Samples Explorer");
            title.AddToClassList(Styles.SamplesIntroTitle);
            intro.Add(title);

            var message = new Label(
                "Select any sample in the left menu to see attributes in action.");
            message.AddToClassList(Styles.SamplesIntroMessage);
            intro.Add(message);

            var prompt = new Label("What would you like to explore?");
            prompt.AddToClassList(Styles.SamplesIntroPrompt);
            intro.Add(prompt);

            var categories = new VisualElement();
            categories.AddToClassList(Styles.SamplesIntroButtons);

            foreach (var pair in _groupToId)
            {
                var categoryId = pair.Value;
                var button = new Button(() =>
                {
                    _menuTree.ExpandItem(categoryId);
                    _menuTree.SetSelectionById(categoryId);
                    _menuTree.ScrollToItemById(categoryId);
                })
                {
                    text = pair.Key,
                };
                button.AddToClassList(Styles.SamplesIntroButton);
                categories.Add(button);
            }

            intro.Add(categories);

            _detailContainer.Add(intro);
        }

        private void ShowCategory(MenuEntry entry)
        {
            ClearCurrent();

            selectedSampleType = null;
            selectedCategory = entry.Name;

            if (_groupToId.TryGetValue(entry.Name, out var categoryId))
            {
                _menuTree.CollapseAll();
                _menuTree.ExpandItem(categoryId);
            }

            var children = entry.Children ?? Array.Empty<Type>();

            var category = CreateInstance<TriSamplesCategory>();
            category.name = entry.Name;
            category.hideFlags = HideFlags.DontSave;

            foreach (var child in children)
            {
                var sample = CreateInstance(child);
                sample.name = GetTypeNiceName(child);
                sample.hideFlags = HideFlags.DontSave;
                _categoryInstances.Add(sample);

                var script = MonoScript.FromScriptableObject(sample);

                category.overview.Add(new CategorySampleEntry
                {
                    title = GetTypeNiceName(child),
                    description = script != null ? ExtractSummary(script.text) : null,
                    code = script != null ? script.text : null,
                    sample = sample,
                });
            }

            _current = category;
            _currentEditor = UnityEditor.Editor.CreateEditor(_current);

            var header = new Label(entry.Name);
            header.AddToClassList(Styles.SamplesHeader);
            _detailContainer.Add(header);

            _currentPreview = new TriEditorCore(_currentEditor.serializedObject)
            {
                HideMonoScript = true,
            };
            _detailContainer.Add(WrapPreview(_currentPreview));
        }

        private void BuildSampleDetail()
        {
            var header = new Label(_current.name);
            header.AddToClassList(Styles.SamplesHeader);
            _detailContainer.Add(header);

            if (_currentEditor.GetType() != typeof(TriScriptableObjectEditor))
            {
                _detailContainer.Add(new HelpBox(
                    "Detected third party asset that overrides all inspectors. " +
                    "Tri-Inspector's attributes might not work\n" +
                    _currentEditor.GetType().FullName, HelpBoxMessageType.Error));
            }

            var summary = ExtractSummary(_currentMonoScript.text);
            if (!string.IsNullOrEmpty(summary))
            {
                var description = new Label(summary);
                description.AddToClassList(Styles.SamplesDescription);
                _detailContainer.Add(description);
            }

            _detailContainer.Add(CreateSectionLabel("Preview"));
            _currentPreview = new TriEditorCore(_currentEditor.serializedObject)
            {
                HideMonoScript = true,
            };
            _detailContainer.Add(WrapPreview(_currentPreview));

            _detailContainer.Add(CreateSectionLabel("Code"));
            _detailContainer.Add(new TextField
            {
                multiline = true,
                isReadOnly = true,
                value = _currentMonoScript.text,
            });
        }

        private static VisualElement WrapPreview(TriEditorCore core)
        {
            var inspector = new VisualElement();
            inspector.AddToClassList(TriStyles.UnityInspectorElement);
            inspector.AddToClassList(TriStyles.UnityInspectorMainContainer);
            inspector.Add(core.CreateVisualElement());
            return inspector;
        }

        private static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList(Styles.SamplesSection);
            return label;
        }

        private static string ExtractSummary(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
            {
                return null;
            }

            var start = sourceText.IndexOf("<summary>", StringComparison.Ordinal);
            var end = sourceText.IndexOf("</summary>", StringComparison.Ordinal);
            if (start < 0 || end < 0 || end < start)
            {
                return null;
            }

            start += "<summary>".Length;
            var body = sourceText.Substring(start, end - start);

            var lines = body.Split('\n')
                .Select(line => line.TrimStart().TrimStart('/').Trim())
                .Where(line => line.Length > 0);

            return string.Join(" ", lines);
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

            return ObjectNames.NicifyVariableName(name);
        }

        private readonly struct MenuEntry
        {
            public readonly string Name;
            public readonly Type Type;
            public readonly IReadOnlyList<Type> Children;

            public MenuEntry(string name, Type type, IReadOnlyList<Type> children = null)
            {
                Name = name;
                Type = type;
                Children = children;
            }
        }

        private class TriSamplesCategory : ScriptableObject
        {
            [ListDrawerSettings(
                Draggable = false,
                HideArraySize = true,
                HideAddButton = true,
                HideRemoveButton = true,
                AlwaysExpanded = true)]
            public List<CategorySampleEntry> overview = new List<CategorySampleEntry>();
        }

        [Serializable]
        [DeclareFoldoutGroup("box", Title = "$" + nameof(title))]
        [DeclareFoldoutGroup("box/code", Title = "Source Code")]
        private class CategorySampleEntry
        {
            [HideInInspector] public string title;
            [HideInInspector] public string code;

            [Group("box")]
            [HideLabel, DisplayAsString(Multiline = true)]
            [PropertySpace(7, 7)]
            public string description;

            [Group("box")]
            [InlineEditor(InlineEditorModes.GUIOnly |
                          InlineEditorModes.CompletelyHideObjectField |
                          InlineEditorModes.DrawWithTriInspectorWithoutMonoScriptField)]
            public ScriptableObject sample;

            [ShowInInspector]
            [Group("box/code")]
            [HideLabel, PropertyTextArea]
            public string Code
            {
                get => code;
                set { }
            }
        }

        private static class Styles
        {
            public const string Samples = "tri-samples";
            public const string SamplesMenu = "tri-samples__menu";
            public const string SamplesSearch = "tri-samples__search";
            public const string SamplesSearchRow = "tri-samples__search-row";
            public const string SamplesTree = "tri-samples__tree";
            public const string SamplesDetailScroll = "tri-samples__detail-scroll";
            public const string SamplesDetail = "tri-samples__detail";
            public const string SamplesHeader = "tri-samples__header";
            public const string SamplesSection = "tri-samples__section";
            public const string SamplesDescription = "tri-samples__description";
            public const string SamplesTreeItem = "tri-samples__tree-item";
            public const string SamplesIntro = "tri-samples__intro";
            public const string SamplesIntroTitle = "tri-samples__intro-title";
            public const string SamplesIntroMessage = "tri-samples__intro-message";
            public const string SamplesIntroPrompt = "tri-samples__intro-prompt";
            public const string SamplesIntroButtons = "tri-samples__intro-buttons";
            public const string SamplesIntroButton = "tri-samples__intro-button";
        }
    }
}