using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace TriInspector.Validator
{
    internal class TriValidationTreeUseCasesController
    {
        private readonly TriValidationTreeController _controller;
        private readonly TriValidationTreeModel _model;

        private readonly Dictionary<Scene, List<TriObjectValidationResult>> _results = new();
        private readonly List<TriAssetValidationResult> _assetResults = new();

        public TriValidationTreeUseCasesController(TriValidationTreeModel model, TriValidationTreeController controller)
        {
            _controller = controller;
            _model = model;
        }

        public void RemoveScene(Scene scene)
        {
            var node = _model.FindSceneNode(scene);
            if (node == null)
            {
                return;
            }

            _controller.DetachNode(node, rebuildTree: false);
            _controller.RefreshItems();
        }

        public void RemoveScenes()
        {
            RemoveRoots(TriValidationNodeKind.Scene);
        }

        public void Clear()
        {
            _results.Clear();
            _assetResults.Clear();

            _model.Roots.Clear();

            _controller.RebuildTree();
        }

        public void RevalidateOpenedScenes()
        {
            TriValidationForegroundJob.Run(TriOpenedSceneValidationScanner.ScanSteps(_results));

            foreach (var pair in _results)
            {
                RebuildScene(_model, pair.Key, pair.Value);
            }

            TriValidationForegroundJob.Run(RebuildPrefabStageRootSteps(PrefabStageUtility.GetCurrentPrefabStage()));
        }

        public IEnumerator<TriValidationScanStep> RevalidateSceneSteps(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                yield break;
            }

            var results = new List<TriObjectValidationResult>();

            var steps = TriOpenedSceneValidationScanner.ScanSceneSteps(scene, results);
            try
            {
                while (steps.MoveNext())
                {
                    yield return steps.Current;
                }
            }
            finally
            {
                steps?.Dispose();
            }

            RebuildScene(_model, scene, results);
        }

        public IEnumerator<TriValidationScanStep> RebuildPrefabStageRootSteps(PrefabStage stage)
        {
            var root = stage != null ? stage.prefabContentsRoot : null;
            if (root == null)
            {
                yield break;
            }

            if (!TriValidatorSettings.instance.IsInScope(stage.assetPath))
            {
                yield break;
            }

            using (ListPool<TriObjectValidationResult>.Get(out var results))
            {
                var steps = TriOpenedSceneValidationScanner.ScanRootSteps(root, results);
                try
                {
                    while (steps.MoveNext())
                    {
                        yield return steps.Current;
                    }
                }
                finally
                {
                    steps.Dispose();
                }

                RebuildSceneRoot(_model, stage.scene, stage.assetPath, results);
            }

            _controller.RebuildTree();
        }

        public void RevalidateProject()
        {
            TriValidationForegroundJob.Run(TriAssetValidationScanner.ScanSteps(_assetResults));

            RemoveRoots(TriValidationNodeKind.Folder);
            BuildAssets(_model, _assetResults);

            _controller.RebuildTree();
        }

        private void RemoveRoots(TriValidationNodeKind kind)
        {
            for (var i = _model.Roots.Count - 1; i >= 0; i--)
            {
                if (_model.Roots[i].Data.kind == kind)
                {
                    _model.Roots.RemoveAt(i);
                }
            }
        }

        private void RebuildScene(TriValidationTreeModel model, Scene scene, List<TriObjectValidationResult> issues)
        {
            RebuildSceneRoot(model, scene, scene.path, issues);
        }

        private void RebuildSceneRoot(TriValidationTreeModel model, Scene scene, string assetPath,
            List<TriObjectValidationResult> issues)
        {
            RemoveScene(scene);

            if (issues.Count == 0)
            {
                return;
            }

            var node = model.InsertSceneNode(scene, assetPath, out _);

            BuildGameObjectHierarchy(model, node, issues);

            TriValidationTreeCounts.Recompute(node);

            _controller.RebuildTree();
        }

        private static void BuildAssets(TriValidationTreeModel model, List<TriAssetValidationResult> issues)
        {
            if (issues.Count == 0)
            {
                return;
            }

            var foldersLookup = new Dictionary<string, TriValidationNode>();

            foreach (var asset in issues)
            {
                if (asset.asset == null)
                {
                    continue;
                }

                var folderNode = GetOrCreateFolder(model, foldersLookup, GetDirectory(asset.assetPath));
                var node = model.InsertAssetNode(folderNode, asset, insertIndex: null);
                model.InsertIssueNodes(node, asset.asset, asset.objects);
            }

            foreach (var folder in foldersLookup.Values)
            {
                if (folder.Parent != model.Root)
                {
                    continue;
                }

                TriValidationTreeCounts.Recompute(folder);
            }
        }

        private static void BuildGameObjectHierarchy(
            TriValidationTreeModel model, TriValidationNode rootAnchor, List<TriObjectValidationResult> components)
        {
            // Build-time only: dedup ancestor GameObject nodes in O(1).
            var gameObjectNodes = new Dictionary<GameObject, TriValidationNode>();

            TriValidationNode GetGameObjectNode(GameObject go)
            {
                if (gameObjectNodes.TryGetValue(go, out var existing))
                {
                    return existing;
                }

                var parent = go.transform.parent;
                var parentNode = parent != null ? GetGameObjectNode(parent.gameObject) : rootAnchor;
                var node = model.InsertGameObjectNode(go, parentNode, insertIndex: null);
                gameObjectNodes.Add(go, node);
                return node;
            }

            foreach (var entry in components)
            {
                if (entry.obj is not MonoBehaviour behaviour)
                {
                    continue;
                }

                var goNode = GetGameObjectNode(behaviour.gameObject);
                var node = model.InsertComponentNode(behaviour, goNode, insertIndex: null);
                model.InsertIssueNodes(node, behaviour, new List<TriObjectValidationResult> {entry,});
            }
        }

        private static TriValidationNode GetOrCreateFolder(
            TriValidationTreeModel model, Dictionary<string, TriValidationNode> folders, string folderPath)
        {
            if (folders.TryGetValue(folderPath, out var existing))
            {
                return existing;
            }

            var slash = folderPath.LastIndexOf('/');
            var name = slash >= 0 ? folderPath.Substring(slash + 1) : folderPath;

            var parentNode =
                slash < 0 ? model.Root : GetOrCreateFolder(model, folders, folderPath.Substring(0, slash));

            var node = model.InsertFolderNode(name, parentNode, insertIndex: null);
            folders.Add(folderPath, node);
            return node;
        }

        private static string GetDirectory(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash >= 0 ? path.Substring(0, slash) : path;
        }
    }
}