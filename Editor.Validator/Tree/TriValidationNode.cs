using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TriInspector.Validator
{
    internal sealed class TriValidationNode
    {
        public TriValidationNode(int id, TriValidationNodeData data)
        {
            Id = id;
            Data = data;
        }

        public int Id { get; }

        // Shared by reference with the copy the TreeView holds, so mutating counts here + RefreshItems()
        // updates the visible row without a structural rebuild.
        public TriValidationNodeData Data { get; }

        public TriValidationNode Parent { get; set; }
        public List<TriValidationNode> Children { get; } = new List<TriValidationNode>();

        // Set on scene (root) nodes only; used to match a behaviour's scene during live inserts.
        public Scene Scene { get; set; }

        public TreeViewItemData<TriValidationNodeData> ToItemData()
        {
            List<TreeViewItemData<TriValidationNodeData>> childItems = null;
            if (Children.Count > 0)
            {
                childItems = new List<TreeViewItemData<TriValidationNodeData>>(Children.Count);
                foreach (var child in Children)
                {
                    childItems.Add(child.ToItemData());
                }
            }

            return new TreeViewItemData<TriValidationNodeData>(Id, Data, childItems);
        }

        public TriValidationNode FindFolderChild(string name)
        {
            return FindChildByLabel(TriValidationNodeKind.Folder, name);
        }

        public TriValidationNode FindGameObjectChild(GameObject go)
        {
            return FindChildByTarget(TriValidationNodeKind.GameObject, go);
        }

        public TriValidationNode FindChildByTarget(TriValidationNodeKind kind, Object target)
        {
            foreach (var child in Children)
            {
                if (child.Data.kind == kind && child.Data.target == target)
                {
                    return child;
                }
            }

            return null;
        }

        public TriValidationNode FindChildByLabel(TriValidationNodeKind kind, string label)
        {
            foreach (var child in Children)
            {
                if (child.Data.kind == kind && child.Data.label == label)
                {
                    return child;
                }
            }

            return null;
        }
    }
}