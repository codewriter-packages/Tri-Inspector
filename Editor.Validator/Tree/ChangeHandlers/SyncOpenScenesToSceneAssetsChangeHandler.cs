using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace TriInspector.Validator.ChangeHandlers
{
    internal class SyncOpenScenesToSceneAssetsChangeHandler : TriValidationControllerChangeHandler
    {
        private readonly TriValidationTreeController _controller;
        private readonly TriValidationTreeModel _model;

        public SyncOpenScenesToSceneAssetsChangeHandler(
            TriValidationTreeController controller,
            TriValidationTreeModel model)
        {
            _controller = controller;
            _model = model;
        }

        public override void HandleChanged()
        {
            // Mirror each open scene's live counts onto its folder SceneAsset entry. This may add, remove, or
            // re-count existing rows, so refresh the view when it did (a full rebuild has already refreshed,
            // but a live flush refreshed before RaiseChanged and won't see these edits otherwise).
            SyncOpenScenesToSceneAssets(rebuildTree: false);
        }

        /// <summary>
        /// Forwards each open scene's live, rolled-up Scene-root counts onto a folder SceneAsset entry
        /// so the entry shows up the moment a scene is opened and tracks edits in real time.
        /// <br/>
        /// <remarks>
        /// Also keeps the session cache fresh so the entry survives the scene being closed.
        /// </remarks>
        /// </summary>
        /// <returns>Returns whether any node was added, removed, or re-counted.</returns>
        private bool SyncOpenScenesToSceneAssets(bool rebuildTree)
        {
            var changed = false;

            using var _ = ListPool<TriValidationNode>.Get(out var sceneRoots);

            foreach (var root in _model.Roots)
            {
                if (root.Data.kind == TriValidationNodeKind.Scene &&
                    !EditorSceneManager.IsPreviewScene(root.Scene) &&
                    !string.IsNullOrEmpty(root.Scene.path))
                {
                    sceneRoots.Add(root);
                }
            }

            foreach (var sceneRoot in sceneRoots)
            {
                var path = sceneRoot.Scene.path;
                var errors = sceneRoot.Data.errorCount;
                var warnings = sceneRoot.Data.warningCount;
                var sceneAsset = sceneRoot.Data.target;

                TriSceneIssueCache.Record(AssetDatabase.AssetPathToGUID(path), errors, warnings);

                var node = _model.FindAssetNodeInTree(sceneAsset, path);

                if (errors + warnings <= 0)
                {
                    if (node != null)
                    {
                        _controller.RemoveAssetNode(node, rebuildTree);
                        changed = true;
                    }

                    continue;
                }

                if (node == null)
                {
                    if (sceneAsset != null)
                    {
                        _controller.InsertAssetNode(MakeSceneAssetValidation(path, sceneAsset, errors, warnings),
                            rebuildTree);
                        changed = true;
                    }
                }
                else
                {
                    changed |= UpdateSceneAssetNodeCounts(node, errors, warnings);
                }
            }

            return changed;
        }

        private static TriAssetValidationResult MakeSceneAssetValidation(
            string path, Object sceneAsset, int errors, int warnings)
        {
            return new TriAssetValidationResult
            {
                assetPath = path,
                label = sceneAsset.name,
                asset = sceneAsset,
                countOnlyKind = TriValidationNodeKind.SceneAsset,
                errorCount = errors,
                warningCount = warnings,
            };
        }

        private static bool UpdateSceneAssetNodeCounts(TriValidationNode node, int errors, int warnings)
        {
            if (node.Data.errorCount == errors && node.Data.warningCount == warnings)
            {
                return false;
            }

            node.Data.errorCount = errors;
            node.Data.warningCount = warnings;
            node.Data.severity = errors > 0 ? TriMessageType.Error
                : warnings > 0 ? TriMessageType.Warning
                : TriMessageType.None;

            TriValidationTreeCounts.RecomputeFrom(node);
            return true;
        }

        // Scene equivalent of RevalidateKnownTargets: rebuilds a count-only folder row for every scene with a
        // cached issue count so the rows survive a domain reload without needing a Project scan. A closed scene
        // can't be re-validated, so the counts come straight from the restored cache.
        public void RestoreCachedIssuesForSceneAssets(List<SceneIssueEntry> entries, bool rebuildTree)
        {
            var settings = TriValidatorSettings.instance;
            var changed = false;

            foreach (var entry in entries)
            {
                if (entry.errors + entry.warnings <= 0)
                {
                    continue;
                }

                var path = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (string.IsNullOrEmpty(path) || !settings.IsInScope(path))
                {
                    continue;
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (sceneAsset == null)
                {
                    continue;
                }

                if (_model.FindAssetNodeInTree(sceneAsset, path) != null)
                {
                    continue;
                }

                _controller.InsertAssetNode(MakeSceneAssetValidation(path, sceneAsset, entry.errors, entry.warnings),
                    rebuildTree);
                changed = true;
            }

            if (changed)
            {
                _controller.RefreshItems();
            }
        }
    }
}