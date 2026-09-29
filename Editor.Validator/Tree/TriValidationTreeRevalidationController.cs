using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TriInspector.Validator
{
    internal class TriValidationTreeRevalidationController
    {
        private readonly TriValidationTreeController _controller;
        private readonly TriValidationTreeModel _model;

        private readonly HashSet<Object> _pendingTargets = new();

        private bool _flushQueued;

        public TriValidationTreeRevalidationController(TriValidationTreeModel model,
            TriValidationTreeController controller)
        {
            _controller = controller;
            _model = model;
        }

        public void ClearPending()
        {
            _pendingTargets.Clear();
        }

        public void QueuePending(IReadOnlyList<Object> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            foreach (var target in targets)
            {
                if (target != null)
                {
                    _pendingTargets.Add(target);
                }
            }

            if (_pendingTargets.Count > 0)
            {
                ScheduleFlush();
            }
        }

        public void QueuePending(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            _pendingTargets.Add(obj);
            ScheduleFlush();
        }

        private void ScheduleFlush()
        {
            if (_flushQueued)
            {
                return;
            }

            _flushQueued = true;
            EditorApplication.delayCall += FlushPending;
        }

        private void FlushPending()
        {
            _flushQueued = false;

            foreach (var target in _pendingTargets)
            {
                RevalidateTarget(target, rebuildTree: false);
            }

            _pendingTargets.Clear();

            _controller.RefreshItems();
        }

        private void RevalidateTarget(Object target, bool rebuildTree)
        {
            switch (target)
            {
                case MonoBehaviour behaviour:
                    RevalidateComponentImmediate(behaviour, rebuildTree);
                    break;
                case GameObject go when go.scene.IsValid():
                    RevalidateGameObjectImmediate(go, rebuildTree);
                    break;
                case SceneAsset:
                    // SceneAsset cannot be validated here
                    break;
                default:
                    RevalidateAssetImmediate(target, rebuildTree);
                    break;
            }
        }

        public IEnumerator<TriValidationScanStep> RestoreRoutine(IReadOnlyList<Object> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                yield break;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target != null)
                {
                    RevalidateTarget(target, rebuildTree: false);
                }

                yield return new TriValidationScanStep("Restoring known issues", i + 1, targets.Count);
            }

            _controller.RefreshItems();
        }

        public bool RevalidateGameObjectImmediate(TriEntityID entityID, bool rebuildTree)
        {
            return RevalidateGameObjectImmediate(entityID.Obj as GameObject, rebuildTree);
        }

        public bool RevalidateGameObjectImmediate(GameObject go, bool rebuildTree)
        {
            if (go == null)
            {
                return false;
            }

            var scene = go.scene;
            if (!scene.IsValid() || !scene.isLoaded || !TriValidatorSettings.instance.IsInScope(scene))
            {
                return false;
            }

            var changed = false;

            var gameObjectNode = _model.FindGameObjectNodeInTree(go);
            if (gameObjectNode != null && PruneNullTargets(gameObjectNode, rebuildTree))
            {
                var survivor = _controller.PruneEmptyBranchUpward(gameObjectNode, rebuildTree);
                _controller.SetDefaultExpansion(survivor, rebuildTree);
                TriValidationTreeCounts.RecomputeFrom(survivor);
                changed = true;
            }

            using (ListPool<MonoBehaviour>.Get(out var behaviours))
            {
                go.GetComponentsInChildren(true, behaviours);
                foreach (var behaviour in behaviours)
                {
                    changed |= RevalidateComponentImmediate(behaviour, rebuildTree);
                }
            }

            return changed;
        }

        public bool RevalidateAssetImmediate(Object asset, bool rebuildTree)
        {
            if (asset == null)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!TriValidatorSettings.instance.IsInScope(path))
            {
                // Drop any stale node if the asset was moved into an out-of-scope folder.
                var outOfScope = _model.FindAssetNodeInTree(asset, path);
                if (outOfScope != null)
                {
                    _controller.RemoveAssetNode(outOfScope, rebuildTree);
                    return true;
                }

                return false;
            }

            var existing = _model.FindAssetNodeInTree(asset, path);
            var hasIssues = TriAssetValidator.TryValidate(asset, out var validation);

            if (existing != null && asset is ScriptableObject)
            {
                if (hasIssues)
                {
                    return ReconcileComponent(existing, validation.objects, rebuildTree);
                }

                _controller.RemoveAssetNode(existing, rebuildTree);
                return true;
            }

            var changed = false;

            if (existing != null)
            {
                _controller.RemoveAssetNode(existing, rebuildTree);
                changed = true;
            }

            if (hasIssues)
            {
                _controller.InsertAssetNode(validation, rebuildTree);
                changed = true;
            }

            return changed;
        }

        public bool RevalidateComponentImmediate(MonoBehaviour behaviour, bool rebuildTree)
        {
            if (behaviour == null)
            {
                return false;
            }

            if (!TriValidatorSettings.instance.IsInScope(behaviour.gameObject.scene))
            {
                return false;
            }

            var hasIssues = TriObjectValidator.TryValidateComponent(behaviour, out var objIssues);

            var componentNode = _model.FindComponentNodeInTree(behaviour);
            if (componentNode != null)
            {
                return ReconcileComponent(componentNode, new List<TriObjectValidationResult> {objIssues}, rebuildTree);
            }

            if (hasIssues)
            {
                InsertComponent(behaviour, new List<TriObjectValidationResult> {objIssues}, rebuildTree);
                return true;
            }

            return false;
        }

        // Removes every GameObject/Component node whose target has been destroyed (fake-null), then, on the way
        // back up, drops any survivor left without issues so no empty branch lingers. Recurses into survivors.
        // Does not recompute counts — the caller does that once over the affected subtree.
        public bool PruneNullTargets(TriValidationNode node, bool rebuildTree)
        {
            var changed = false;

            for (var i = node.Children.Count - 1; i >= 0; i--)
            {
                var child = node.Children[i];
                var isObjectNode = child.Data.kind == TriValidationNodeKind.GameObject ||
                                   child.Data.kind == TriValidationNodeKind.Component;

                if (isObjectNode && child.Data.target == null)
                {
                    _controller.DetachNode(child, rebuildTree);
                    changed = true;
                    continue;
                }

                changed |= PruneNullTargets(child, rebuildTree);

                if (isObjectNode && child.Children.Count == 0)
                {
                    _controller.DetachNode(child, rebuildTree);
                    changed = true;
                }
            }

            return changed;
        }

        public void InsertComponent(MonoBehaviour behaviour, List<TriObjectValidationResult> issues, bool rebuildTree)
        {
            var goNode = _controller.EnsureGameObjectNode(behaviour.gameObject, rebuildTree);

            var node = _model.InsertComponentNode(behaviour, goNode, out var insertIndex);
            _controller.AttachItemExpanded(node, insertIndex, rebuildTree);

            ReconcileComponent(node, issues, rebuildTree);
        }

        // Syncs an existing component node to the actual current issues: removes the leaves of resolved
        // issues, adds leaves for issues that appeared, then recomputes counts up to the scene root.
        // If the component is left without issues, the now-empty branch is pruned upward (cascading through
        // GameObject ancestors) so the tree only ever shows nodes that still have issues.
        // Returns whether anything changed (so callers know to refresh the view).
        public bool ReconcileComponent(TriValidationNode componentNode, List<TriObjectValidationResult> objIssues,
            bool rebuildTree)
        {
            using var _ = HashSetPool<TriValidationIssueKey>.Get(out var current);
            using var __ = HashSetPool<TriValidationIssueKey>.Get(out var shown);

            foreach (var objIssue in objIssues)
            {
                if (objIssue.issues != null)
                {
                    foreach (var issue in objIssue.issues)
                    {
                        current.Add(TriValidationIssueKey.From(issue));
                    }
                }
            }

            var changed = false;

            for (var i = componentNode.Children.Count - 1; i >= 0; i--)
            {
                var child = componentNode.Children[i];
                var key = TriValidationIssueKey.From(child.Data);

                if (current.Contains(key))
                {
                    shown.Add(key);
                }
                else
                {
                    _controller.DetachNode(child, rebuildTree);
                    changed = true;
                }
            }

            foreach (var key in current)
            {
                if (shown.Contains(key))
                {
                    continue;
                }

                var node = _model.InsertIssueNode(componentNode, key, out var insertIndex);
                _controller.AttachItemExpanded(node, insertIndex, rebuildTree);
                changed = true;
            }

            if (changed)
            {
                var survivor = _controller.PruneEmptyBranchUpward(componentNode, rebuildTree);
                if (survivor != componentNode)
                {
                    _controller.SetDefaultExpansion(survivor, rebuildTree);
                }

                TriValidationTreeCounts.RecomputeUpward(survivor);
            }

            return changed;
        }
    }
}