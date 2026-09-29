using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace TriInspector.Validator
{
    /// <summary>
    /// Pure ordering rules that keep a live-inserted node where the next full build would place it: components
    /// ordered by <c>GetComponents</c> order and kept ahead of the child-GameObject block, GameObjects ordered
    /// by transform sibling index after that block. Shared by every incremental insert/reorder so the live
    /// tree can't drift from the rebuilt one.
    /// </summary>
    internal static class TriValidationTreeOrdering
    {
        public static int ComputeRootIndex(List<TriValidationNode> roots, TriValidationNode node)
        {
            var order = RootOrder(node);

            var index = 0;
            foreach (var root in roots)
            {
                if (RootOrder(root) <= order)
                {
                    index++;
                }
            }

            return index;
        }

        private static int RootOrder(TriValidationNode node)
        {
            if (node.Data.kind == TriValidationNodeKind.Scene)
            {
                return EditorSceneManager.IsPreviewScene(node.Scene) ? 0 : 1;
            }

            return 2;
        }

        public static int ComputeComponentChildIndex(TriValidationNode goNode, MonoBehaviour behaviour)
        {
            using var _ = ListPool<Component>.Get(out var components);

            behaviour.gameObject.GetComponents(components);
            var order = components.IndexOf(behaviour);

            var index = 0;
            foreach (var child in goNode.Children)
            {
                if (child.Data.kind != TriValidationNodeKind.Component)
                {
                    continue;
                }

                var childOrder = components.IndexOf(child.Data.target as Component);
                if (childOrder >= 0 && childOrder < order)
                {
                    index++;
                }
            }

            return index;
        }

        public static int ComputeGameObjectChildIndex(TriValidationNode parentNode, GameObject go)
        {
            var siblingIndex = go.transform.GetSiblingIndex();

            var index = 0;
            foreach (var child in parentNode.Children)
            {
                if (child.Data.kind == TriValidationNodeKind.Component)
                {
                    // GameObjects always follow the component block.
                    index++;
                    continue;
                }

                if (child.Data.kind != TriValidationNodeKind.GameObject)
                {
                    continue;
                }

                if (child.Data.target is GameObject childGo && childGo.transform.GetSiblingIndex() < siblingIndex)
                {
                    index++;
                }
            }

            return index;
        }

        public static int ComputeFolderChildIndex(TriValidationNode folder, bool isFolder, string label)
        {
            var index = 0;
            foreach (var child in folder.Children)
            {
                var childFolder = child.Data.kind == TriValidationNodeKind.Folder;
                if (CompareFolderChild(childFolder, child.Data.label, isFolder, label) < 0)
                {
                    index++;
                }
            }

            return index;
        }

        private static int CompareFolderChild(bool aFolder, string aLabel, bool bFolder, string bLabel)
        {
            if (aFolder != bFolder)
            {
                return aFolder ? -1 : 1;
            }

            return string.CompareOrdinal(aLabel, bLabel);
        }

        public static bool HasChildGameObject(TriValidationNode node)
        {
            foreach (var child in node.Children)
            {
                if (child.Data.kind == TriValidationNodeKind.GameObject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}