using System.Collections.Generic;
using TriInspector.Validator.ChangeHandlers;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    public class TriValidationWindow : EditorWindow
    {
        [MenuItem("Tools/Tri Inspector/Validator")]
        public static void Open()
        {
            var window = GetWindow<TriValidationWindow>();
            window.titleContent = new GUIContent("Tri Validator");
            window.Show();
        }

        private TriValidationTreeVisualElement _tree;
        private TriValidationTreeController _controller;
        private TriValidationTreeRevalidationController _revalidationController;
        private TriValidationTreeObjectChangeStreamController _changeStreamController;
        private TriValidationTreeUseCasesController _useCasesController;

        private TriValidationBackgroundJob _restoreKnownIssuesJob;
        private readonly Dictionary<Scene, TriValidationBackgroundJob> _openedScenesValidateJobs = new();
        private TriValidationBackgroundJob _openedPrefabStageValidationJob;

        private PopulateHierarchyCountsChangeHandler _populateHierarchyCountsChangeHandler;
        private SyncOpenScenesToSceneAssetsChangeHandler _syncOpenScenesToSceneAssetsChangeHandler;
        private UpdateTitleChangeHandler _updateTitleChangeHandler;

        [SerializeField] private List<Object> knownIssues = new List<Object>();
        [SerializeField] private List<SceneIssueEntry> knownSceneIssues = new List<SceneIssueEntry>();

        private void OnEnable()
        {
            _tree = new TriValidationTreeVisualElement();
            _tree.ItemSelected += OnItemSelected;
            _tree.ItemChosen += OnItemChosen;

            var handlers = new TriValidationControllerChangeHandler[3];

            var model = new TriValidationTreeModel();
            var treeView = _tree.TreeView;
            _controller = new TriValidationTreeController(model, treeView, handlers);
            _revalidationController = new TriValidationTreeRevalidationController(model, _controller);
            _changeStreamController = new TriValidationTreeObjectChangeStreamController(
                model, _controller, _revalidationController);
            _useCasesController = new TriValidationTreeUseCasesController(model, _controller);

            handlers[0] = _populateHierarchyCountsChangeHandler = new PopulateHierarchyCountsChangeHandler(model);
            handlers[1] = _syncOpenScenesToSceneAssetsChangeHandler =
                new SyncOpenScenesToSceneAssetsChangeHandler(_controller, model);
            handlers[2] = _updateTitleChangeHandler = new UpdateTitleChangeHandler(model, this);

            AssemblyReloadEvents.beforeAssemblyReload += SaveKnownIssues;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            PrefabStage.prefabSaved += OnPrefabStageSaved;
            Selection.selectionChanged += OnSelectionChanged;
            ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            _restoreKnownIssuesJob?.Cancel();
            _openedPrefabStageValidationJob?.Cancel();
            foreach (var (_, job) in _openedScenesValidateJobs)
            {
                job.Cancel();
            }

            AssemblyReloadEvents.beforeAssemblyReload -= SaveKnownIssues;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            PrefabStage.prefabSaved -= OnPrefabStageSaved;
            Selection.selectionChanged -= OnSelectionChanged;
            ObjectChangeEvents.changesPublished -= OnObjectChangesPublished;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnFocus()
        {
            _updateTitleChangeHandler?.UpdateTitle();
        }

        private static bool IsAuto(TriValidationAutoTrigger trigger)
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode &&
                   TriValidationPreferences.IsAuto(trigger);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode &&
                TriValidationPreferences.IsAuto(TriValidationAutoTrigger.Selection))
            {
                _revalidationController?.QueuePending(Selection.objects);
            }
        }

        private void OnSceneSaved(Scene scene)
        {
            if (IsAuto(TriValidationAutoTrigger.SceneSave))
            {
                if (_openedScenesValidateJobs.TryGetValue(scene, out var job))
                {
                    job.Cancel();
                }

                _openedScenesValidateJobs[scene] = TriValidationBackgroundJob.Start(
                    "Tri - Revalidate scene",
                    _useCasesController.RevalidateSceneSteps(scene));
            }
        }

        private void OnSceneClosed(Scene scene)
        {
            if (_openedScenesValidateJobs.TryGetValue(scene, out var job))
            {
                job.Cancel();
            }

            _useCasesController?.RemoveScene(scene);
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (IsAuto(TriValidationAutoTrigger.SceneOpen))
            {
                if (_openedScenesValidateJobs.TryGetValue(scene, out var job))
                {
                    job.Cancel();
                }

                if (mode == OpenSceneMode.Single)
                {
                    _useCasesController.RemoveScenes();
                }

                _openedScenesValidateJobs[scene] = TriValidationBackgroundJob.Start(
                    "Tri - Revalidate scene",
                    _useCasesController.RevalidateSceneSteps(scene));
            }
        }

        private void OnPrefabStageOpened(PrefabStage stage)
        {
            if (IsAuto(TriValidationAutoTrigger.PrefabStageOpen))
            {
                _openedPrefabStageValidationJob?.Cancel();
                _openedPrefabStageValidationJob = TriValidationBackgroundJob.Start(
                    "Tri - Revalidate prefab strage",
                    _useCasesController.RebuildPrefabStageRootSteps(stage));
            }
        }

        private void OnPrefabStageClosing(PrefabStage stage)
        {
            _useCasesController?.RemoveScene(stage.scene);
        }

        private void OnPrefabStageSaved(GameObject prefabRoot)
        {
            if (IsAuto(TriValidationAutoTrigger.PrefabStageSave))
            {
                var stage = PrefabStageUtility.GetPrefabStage(prefabRoot);
                if (stage != null)
                {
                    _openedPrefabStageValidationJob?.Cancel();
                    _openedPrefabStageValidationJob = TriValidationBackgroundJob.Start(
                        "Tri - Revalidate prefab stage",
                        _useCasesController.RebuildPrefabStageRootSteps(stage));
                }
            }
        }

        private void OnSelectionChanged()
        {
            if (IsAuto(TriValidationAutoTrigger.Selection))
            {
                _revalidationController?.QueuePending(Selection.objects);
            }
        }

        private void OnObjectChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (IsAuto(TriValidationAutoTrigger.LiveChanges))
            {
                _changeStreamController?.ApplyObjectChanges(ref stream);
            }
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            TriStyleSheet.ApplyTo(root);

            root.Add(new TriValidationToolbarVisualElement(
                onValidateScenes: RunOpenedScenesValidation,
                onValidateProject: RunProjectValidation,
                onClear: ClearValidation
            ));

            root.Add(_tree);

            if (root.panel != null)
            {
                RestoreKnownIssues();
            }
            else
            {
                void OnPanelAttached(AttachToPanelEvent evt)
                {
                    root.UnregisterCallback<AttachToPanelEvent>(OnPanelAttached);
                    RestoreKnownIssues();
                }

                root.RegisterCallback<AttachToPanelEvent>(OnPanelAttached);
            }

            _controller.RefreshItems();
        }

        private void OnItemChosen(Object target)
        {
            EditorGUIUtility.PingObject(target);
        }

        private void OnItemSelected(Object target)
        {
            Selection.activeObject = target;
        }

        private void RunProjectValidation()
        {
            ClearValidation();
            _useCasesController.RevalidateProject();
        }

        private void RunOpenedScenesValidation()
        {
            _openedPrefabStageValidationJob?.Cancel();
            foreach (var (_, job) in _openedScenesValidateJobs)
            {
                job.Cancel();
            }

            _useCasesController.RevalidateOpenedScenes();
        }

        private void ClearValidation()
        {
            _openedPrefabStageValidationJob?.Cancel();
            _restoreKnownIssuesJob?.Cancel();

            foreach (var (_, job) in _openedScenesValidateJobs)
            {
                job.Cancel();
            }

            TriSceneIssueCache.Clear();
            knownIssues.Clear();
            knownSceneIssues.Clear();
            _revalidationController.ClearPending();
            _useCasesController.Clear();
        }

        private void SaveKnownIssues()
        {
            _restoreKnownIssuesJob?.Cancel();

            knownIssues.Clear();
            knownSceneIssues.Clear();

            if (TriValidationPreferences.IsAuto(TriValidationAutoTrigger.KnownIssuesOnDomainReload))
            {
                _controller?.CollectKnownTargets(knownIssues);
                TriSceneIssueCache.Export(knownSceneIssues);
            }
        }

        private void RestoreKnownIssues()
        {
            if (TriValidationPreferences.IsAuto(TriValidationAutoTrigger.KnownIssuesOnDomainReload))
            {
                TriSceneIssueCache.Import(knownSceneIssues);

                _syncOpenScenesToSceneAssetsChangeHandler.RestoreCachedIssuesForSceneAssets(knownSceneIssues,
                    rebuildTree: false);
                _controller.RefreshItems();

                _restoreKnownIssuesJob?.Cancel();
                _restoreKnownIssuesJob = TriValidationBackgroundJob.Start(
                    "Tri - Revalidate known issues",
                    _revalidationController.RestoreRoutine(new List<Object>(knownIssues)));
            }

            knownIssues.Clear();
            knownSceneIssues.Clear();
        }
    }
}