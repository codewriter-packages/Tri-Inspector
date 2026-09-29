using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace TriInspector.Validator
{
    internal class TriValidationTreeObjectChangeStreamController
    {
        private readonly TriValidationTreeController _controller;
        private readonly TriValidationTreeRevalidationController _revalidationController;
        private readonly TriValidationTreeModel _model;

        public TriValidationTreeObjectChangeStreamController(
            TriValidationTreeModel model,
            TriValidationTreeController controller,
            TriValidationTreeRevalidationController revalidationController)
        {
            _controller = controller;
            _revalidationController = revalidationController;
            _model = model;
        }

        public void ApplyObjectChanges(ref ObjectChangeEventStream stream)
        {
            var structuralChanged = false;

            for (var i = 0; i < stream.length; i++)
            {
                switch (stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                    {
                        stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var e);
                        var obj = e.EntityID().Obj;
                        switch (obj)
                        {
                            case MonoBehaviour behaviour:
                                QueuePending(behaviour);
                                break;
                            case GameObject go:
                                structuralChanged |= UpdateGameObjectLabel(go);
                                break;
                        }

                        break;
                    }

                    case ObjectChangeKind.ChangeAssetObjectProperties:
                    {
                        stream.GetChangeAssetObjectPropertiesEvent(i, out var e);
                        _revalidationController.QueuePending(e.EntityID().Obj);
                        break;
                    }

                    case ObjectChangeKind.ChangeGameObjectStructure:
                    {
                        stream.GetChangeGameObjectStructureEvent(i, out var e);
                        structuralChanged |= _revalidationController.RevalidateGameObjectImmediate(e.EntityID(),
                            rebuildTree: false);
                        break;
                    }

                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                    {
                        stream.GetChangeGameObjectStructureHierarchyEvent(i, out var e);
                        structuralChanged |= _revalidationController.RevalidateGameObjectImmediate(e.EntityID(),
                            rebuildTree: false);
                        break;
                    }

                    case ObjectChangeKind.CreateGameObjectHierarchy:
                    {
                        stream.GetCreateGameObjectHierarchyEvent(i, out var e);
                        structuralChanged |= _revalidationController.RevalidateGameObjectImmediate(e.EntityID(),
                            rebuildTree: false);
                        break;
                    }

                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                    {
                        stream.GetDestroyGameObjectHierarchyEvent(i, out var e);
                        var parentGo = e.ParentEntityID().GameObject;
                        structuralChanged |= HandleDestroy(parentGo, e.scene, rebuildTree: false);
                        break;
                    }

                    case ObjectChangeKind.ChangeGameObjectParent:
                    {
                        stream.GetChangeGameObjectParentEvent(i, out var e);
                        var movedGo = e.EntityID().GameObject;
                        var previousParent = e.PreviousParentEntityID().GameObject;
                        structuralChanged |= HandleReParent(movedGo, previousParent, e.previousScene,
                            rebuildTree: false);
                        break;
                    }

                    case ObjectChangeKind.ChangeChildrenOrder:
                    {
                        stream.GetChangeChildrenOrderEvent(i, out var e);
                        if (e.EntityID().Obj is GameObject go)
                        {
                            var parentNode = _model.FindGameObjectNodeInTree(go);
                            if (parentNode != null)
                            {
                                structuralChanged |= ResortGameObjectChildren(parentNode, rebuildTree: false);
                            }
                        }

                        break;
                    }

                    case ObjectChangeKind.ChangeRootOrder:
                    {
                        stream.GetChangeRootOrderEvent(i, out var e);
                        var sceneNode = _model.FindSceneNode(e.scene);
                        if (sceneNode != null)
                        {
                            structuralChanged |= ResortGameObjectChildren(sceneNode, rebuildTree: false);
                        }

                        break;
                    }
                }
            }

            if (structuralChanged)
            {
                _controller.RefreshItems();
            }
        }

        private bool UpdateGameObjectLabel(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            var node = _model.FindGameObjectNodeInTree(go);
            if (node == null || node.Data.label == go.name)
            {
                return false;
            }

            node.Data.label = go.name;
            return true;
        }

        private void QueuePending(MonoBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }

            var scene = behaviour.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || !TriValidatorSettings.instance.IsInScope(scene))
            {
                return;
            }

            _revalidationController.QueuePending(behaviour);
        }

        public bool ResortGameObjectChildren(TriValidationNode parentNode, bool rebuildTree)
        {
            using var _ = ListPool<TriValidationNode>.Get(out var goChildren);

            foreach (var child in parentNode.Children)
            {
                if (child.Data.kind == TriValidationNodeKind.GameObject && child.Data.target is GameObject)
                {
                    goChildren.Add(child);
                }
            }

            if (goChildren.Count < 2)
            {
                return false;
            }

            using var __ = ListPool<TriValidationNode>.Get(out var sorted);

            foreach (var child in goChildren)
            {
                sorted.Add(child);
            }

            sorted.Sort((a, b) => ((GameObject) a.Data.target).transform.GetSiblingIndex()
                .CompareTo(((GameObject) b.Data.target).transform.GetSiblingIndex()));

            var alreadyOrdered = true;
            for (var i = 0; i < goChildren.Count; i++)
            {
                if (goChildren[i] != sorted[i])
                {
                    alreadyOrdered = false;
                    break;
                }
            }

            if (alreadyOrdered)
            {
                return false;
            }

            // Detach all GameObject children, then re-attach them in sibling order (their subtrees ride
            // along via ToItemData). Component siblings keep their slots ahead of the GameObject block.
            foreach (var node in goChildren)
            {
                _controller.DetachNode(node, rebuildTree);
            }

            foreach (var node in sorted)
            {
                var go = (GameObject) node.Data.target;
                var index = TriValidationTreeOrdering.ComputeGameObjectChildIndex(parentNode, go);
                TriValidationTreeModel.LinkParentChild(parentNode, node, index);
                _controller.AttachItemExpanded(node, index, rebuildTree);
            }

            return true;
        }

        private bool HandleDestroy(GameObject parentGo, Scene scene, bool rebuildTree)
        {
            var parentNode = parentGo != null
                ? _model.FindGameObjectNodeInTree(parentGo)
                : _model.FindSceneNode(scene);

            if (parentNode == null || !_revalidationController.PruneNullTargets(parentNode, rebuildTree))
            {
                return false;
            }

            var survivor = _controller.PruneEmptyBranchUpward(parentNode, rebuildTree);
            _controller.SetDefaultExpansion(survivor, rebuildTree);
            TriValidationTreeCounts.RecomputeFrom(survivor);
            return true;
        }

        private bool HandleReParent(GameObject go, GameObject previousParent, Scene previousScene, bool rebuildTree)
        {
            if (go == null)
            {
                return false;
            }

            var oldParentNode = previousParent != null
                ? _model.FindGameObjectNodeInTree(previousParent)
                : _model.FindSceneNode(previousScene);

            if (oldParentNode == null)
            {
                return false;
            }

            var moved = oldParentNode.FindGameObjectChild(go);
            if (moved == null)
            {
                // The moved object wasn't in the tree (no issues under it); nothing to relocate.
                return false;
            }

            _controller.DetachNode(moved, rebuildTree);

            var newParentNode = _controller.EnsureParentAnchor(go, rebuildTree);

            var index = TriValidationTreeOrdering.ComputeGameObjectChildIndex(newParentNode, go);
            TriValidationTreeModel.LinkParentChild(newParentNode, moved, index);
            _controller.AttachItemExpanded(moved, index, rebuildTree);

            _controller.SetDefaultExpansion(newParentNode, rebuildTree);
            TriValidationTreeCounts.RecomputeUpward(newParentNode);

            var oldSurvivor = _controller.PruneEmptyBranchUpward(oldParentNode, rebuildTree);
            _controller.SetDefaultExpansion(oldSurvivor, rebuildTree);
            TriValidationTreeCounts.RecomputeUpward(oldSurvivor);
            return true;
        }
    }
}