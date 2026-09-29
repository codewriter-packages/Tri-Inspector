using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    internal sealed class TriValidationTreeController
    {
        private readonly BaseTreeView _treeView;
        private readonly TriValidationTreeModel _model;
        private readonly TriValidationControllerChangeHandler[] _changeHandlers;

        public TriValidationTreeController(TriValidationTreeModel model, BaseTreeView treeView,
            TriValidationControllerChangeHandler[] changeHandlers)
        {
            _treeView = treeView;
            _model = model;
            _changeHandlers = changeHandlers;

            _treeView.SetRootItems(_model.ToItems());
            _treeView.Rebuild();
        }

        public void RebuildTree()
        {
            _treeView.SetRootItems(_model.ToItems());
            _treeView.Rebuild();

            RefreshItems();
        }

        public void RefreshItems()
        {
            foreach (var changeHandler in _changeHandlers)
            {
                changeHandler.HandleChanged();
            }

            ApplyDefaultExpansion(_model.Roots, rebuildTree: false);

            _treeView.RefreshItems();
        }

        public void InsertAssetNode(TriAssetValidationResult validation, bool rebuildTree)
        {
            var slash = validation.assetPath.LastIndexOf('/');
            var folderPath = slash >= 0 ? validation.assetPath.Substring(0, slash) : validation.assetPath;

            var folder = EnsureFolderNode(folderPath, rebuildTree);

            var assetNode = _model.InsertAssetNode(folder, validation, out var insertIndex);

            _model.InsertIssueNodes(assetNode, validation.asset, validation.objects);

            AttachItemExpanded(assetNode, insertIndex, rebuildTree);

            TriValidationTreeCounts.RecomputeFrom(assetNode);
        }

        private TriValidationNode EnsureFolderNode(string folderPath, bool rebuildTree)
        {
            var segments = folderPath.Split('/');

            var parent = _model.Root;

            foreach (var name in segments)
            {
                var existing = parent.FindFolderChild(name);
                if (existing != null)
                {
                    parent = existing;
                    continue;
                }

                var node = _model.InsertFolderNode(name, parent, out var insertIndex);

                if (parent == _model.Root)
                {
                    _treeView.AddItem(node.ToItemData(), rebuildTree: rebuildTree);
                    _treeView.ExpandItem(node.Id, refresh: rebuildTree);
                }
                else
                {
                    _treeView.AddItem(node.ToItemData(), parent.Id, insertIndex, rebuildTree);
                    _treeView.ExpandItem(parent.Id, refresh: rebuildTree);
                }

                parent = node;
            }

            return parent;
        }

        public TriValidationNode EnsureParentAnchor(GameObject go, bool rebuildTree)
        {
            var parent = go.transform.parent;
            return parent != null
                ? EnsureGameObjectNode(parent.gameObject, rebuildTree)
                : EnsureSceneNode(go.scene, rebuildTree);
        }

        public TriValidationNode EnsureGameObjectNode(GameObject go, bool rebuildTree)
        {
            var parentNode = EnsureParentAnchor(go, rebuildTree);

            var existing = parentNode.FindGameObjectChild(go);
            if (existing != null)
            {
                return existing;
            }

            var node = _model.InsertGameObjectNode(go, parentNode, out var insertIndex);
            _treeView.AddItem(node.ToItemData(), parentNode.Id, insertIndex, rebuildTree);

            SetDefaultExpansion(node, rebuildTree);
            SetDefaultExpansion(parentNode, rebuildTree);

            return node;
        }

        private TriValidationNode EnsureSceneNode(Scene scene, bool rebuildTree)
        {
            var existing = _model.FindSceneNode(scene);
            if (existing != null)
            {
                return existing;
            }

            var node = _model.InsertSceneNode(scene, scene.path, out var insertIndex);
            _treeView.AddItem(node.ToItemData(), parentId: -1, insertIndex, rebuildTree);
            _treeView.ExpandItem(node.Id, refresh: rebuildTree);
            return node;
        }

        public void RemoveAssetNode(TriValidationNode assetNode, bool rebuildTree)
        {
            var folder = assetNode.Parent;

            DetachNode(assetNode, rebuildTree);

            var survivor = PruneEmptyBranchUpward(folder, rebuildTree);
            TriValidationTreeCounts.RecomputeFrom(survivor);
        }

        public void AttachItemExpanded(TriValidationNode node, int index, bool rebuildTree)
        {
            _treeView.AddItem(node.ToItemData(), node.Parent.Id, index, rebuildTree);
            if (SetDefaultExpansion(node, rebuildTree))
            {
                ApplyDefaultExpansion(node.Children, rebuildTree);
            }
        }

        public void DetachNode(TriValidationNode node, bool rebuildTree)
        {
            node.Parent.Children.Remove(node);
            node.Parent = null;

            _treeView.TryRemoveItem(node.Id, rebuildTree);
        }

        public TriValidationNode PruneEmptyBranchUpward(TriValidationNode node, bool rebuildTree)
        {
            while (IsPrunableEmpty(node))
            {
                var parent = node.Parent;
                DetachNode(node, rebuildTree);
                node = parent;
            }

            return node;
        }

        private static bool IsPrunableEmpty(TriValidationNode node)
        {
            var kind = node.Data.kind;
            var prunable = kind == TriValidationNodeKind.Component
                           || kind == TriValidationNodeKind.GameObject
                           || kind == TriValidationNodeKind.Folder;
            return prunable && node.Children.Count == 0;
        }

        public bool SetDefaultExpansion(TriValidationNode node, bool rebuildTree)
        {
            var kind = node.Data.kind;

            var expand = false;

            expand |= kind == TriValidationNodeKind.Scene;
            expand |= kind == TriValidationNodeKind.Folder;
            expand |= kind == TriValidationNodeKind.Asset && TriValidationTreeOrdering.HasChildGameObject(node);
            expand |= kind == TriValidationNodeKind.GameObject && TriValidationTreeOrdering.HasChildGameObject(node);

            if (expand)
            {
                _treeView.ExpandItem(node.Id, refresh: rebuildTree);
            }
            else
            {
                _treeView.CollapseItem(node.Id, refresh: rebuildTree);
            }

            return expand;
        }

        private void ApplyDefaultExpansion(List<TriValidationNode> nodes, bool rebuildTree)
        {
            foreach (var node in nodes)
            {
                if (SetDefaultExpansion(node, rebuildTree))
                {
                    ApplyDefaultExpansion(node.Children, rebuildTree);
                }
            }
        }

        public void CollectKnownTargets(List<Object> buffer)
        {
            buffer.Clear();

            foreach (var root in _model.Roots)
            {
                CollectKnownTargets(root, buffer);
            }
        }

        private static void CollectKnownTargets(TriValidationNode node, List<Object> buffer)
        {
            switch (node.Data.kind)
            {
                case TriValidationNodeKind.Component:
                    if (node.Data.target != null)
                    {
                        buffer.Add(node.Data.target);
                    }

                    break;

                case TriValidationNodeKind.Asset:
                case TriValidationNodeKind.PrefabAsset:
                    if (node.Data.target != null)
                    {
                        buffer.Add(node.Data.target);
                    }

                    return;
            }

            foreach (var child in node.Children)
            {
                CollectKnownTargets(child, buffer);
            }
        }
    }
}