using System.Collections.Generic;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    internal sealed class TriValidationTreeModel
    {
        public TriValidationNode Root { get; }
        public List<TriValidationNode> Roots => Root.Children;

        private int _nextId;

        public TriValidationTreeModel()
        {
            Root = new TriValidationNode(_nextId++, new TriValidationNodeData());
        }

        public List<TreeViewItemData<TriValidationNodeData>> ToItems()
        {
            var items = new List<TreeViewItemData<TriValidationNodeData>>(Roots.Count);
            foreach (var root in Roots)
            {
                items.Add(root.ToItemData());
            }

            return items;
        }

        public TriValidationNode FindSceneNode(Scene scene)
        {
            foreach (var root in Roots)
            {
                if (root.Scene == scene)
                {
                    return root;
                }
            }

            return null;
        }

        public TriValidationNode FindGameObjectNodeInTree(GameObject go)
        {
            var sceneNode = FindSceneNode(go.scene);
            if (sceneNode == null)
            {
                return null;
            }

            using var _ = ListPool<GameObject>.Get(out var path);
            for (var t = go.transform; t != null; t = t.parent)
            {
                path.Add(t.gameObject);
            }

            var current = sceneNode;
            for (var i = path.Count - 1; i >= 0; i--)
            {
                current = current.FindChildByTarget(TriValidationNodeKind.GameObject, path[i]);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        public TriValidationNode FindComponentNodeInTree(MonoBehaviour behaviour)
        {
            var goNode = FindGameObjectNodeInTree(behaviour.gameObject);
            return goNode?.FindChildByTarget(TriValidationNodeKind.Component, behaviour);
        }

        public TriValidationNode FindAssetNodeInTree(Object asset, string assetPath)
        {
            var kind = asset switch
            {
                GameObject => TriValidationNodeKind.PrefabAsset,
                SceneAsset => TriValidationNodeKind.SceneAsset,
                _ => TriValidationNodeKind.Asset,
            };

            var folder = FindFolderByPathInTree(assetPath);
            return folder?.FindChildByTarget(kind, asset);
        }

        private TriValidationNode FindFolderByPathInTree(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            var segments = assetPath.Split('/');

            var folder = Root.FindChildByLabel(TriValidationNodeKind.Folder, segments[0]);
            for (var i = 1; i < segments.Length - 1 && folder != null; i++)
            {
                folder = folder.FindChildByLabel(TriValidationNodeKind.Folder, segments[i]);
            }

            return folder;
        }

        public TriValidationNode InsertGameObjectNode(GameObject go, TriValidationNode parent, out int insertIndex)
        {
            insertIndex = TriValidationTreeOrdering.ComputeGameObjectChildIndex(parent, go);
            return InsertGameObjectNode(go, parent, insertIndex);
        }

        public TriValidationNode InsertGameObjectNode(GameObject go, TriValidationNode parent, int? insertIndex)
        {
            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = TriValidationNodeKind.GameObject,
                label = go.name,
                target = go,
                icon = EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image,
            });
            LinkParentChild(parent, node, insertIndex);
            return node;
        }

        public TriValidationNode InsertComponentNode(MonoBehaviour component, TriValidationNode parent,
            out int insertIndex)
        {
            insertIndex = TriValidationTreeOrdering.ComputeComponentChildIndex(parent, component);
            return InsertComponentNode(component, parent, insertIndex);
        }

        public TriValidationNode InsertComponentNode(MonoBehaviour component, TriValidationNode parent,
            int? insertIndex)
        {
            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = TriValidationNodeKind.Component,
                label = ObjectNames.GetInspectorTitle(component),
                target = component,
                icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image,
            });
            LinkParentChild(parent, node, insertIndex);
            return node;
        }

        public TriValidationNode InsertFolderNode(string name, TriValidationNode parent, out int insertIndex)
        {
            insertIndex = parent == null
                ? Roots.Count
                : TriValidationTreeOrdering.ComputeFolderChildIndex(parent, true, name);
            return InsertFolderNode(name, parent, insertIndex);
        }

        public TriValidationNode InsertFolderNode(string name, TriValidationNode parent, int? insertIndex)
        {
            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = TriValidationNodeKind.Folder,
                label = name,
                target = null,
                icon = EditorGUIUtility.IconContent("Folder Icon").image,
                isRoot = parent == Root,
            });
            LinkParentChild(parent, node, insertIndex);
            return node;
        }

        public TriValidationNode InsertAssetNode(TriValidationNode parent, TriAssetValidationResult asset,
            out int insertIndex)
        {
            insertIndex = TriValidationTreeOrdering.ComputeFolderChildIndex(parent, false, asset.label);
            return InsertAssetNode(parent, asset, insertIndex);
        }

        public TriValidationNode InsertAssetNode(TriValidationNode parent, TriAssetValidationResult asset,
            int? insertIndex)
        {
            if (asset.countOnlyKind.HasValue)
            {
                return InsertCountOnlyNode(parent, asset, asset.countOnlyKind.Value, insertIndex);
            }

            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = TriValidationNodeKind.Asset,
                label = asset.label,
                target = asset.asset,
                icon = AssetDatabase.GetCachedIcon(asset.assetPath)
                       ?? EditorGUIUtility.ObjectContent(asset.asset, asset.asset.GetType()).image,
            });
            LinkParentChild(parent, node, insertIndex);
            return node;
        }

        private TriValidationNode InsertCountOnlyNode(
            TriValidationNode parent, TriAssetValidationResult asset, TriValidationNodeKind kind, int? insertIndex)
        {
            var severity = asset.errorCount > 0 ? TriMessageType.Error
                : asset.warningCount > 0 ? TriMessageType.Warning
                : TriMessageType.None;

            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = kind,
                label = asset.label,
                target = asset.asset,
                icon = AssetDatabase.GetCachedIcon(asset.assetPath)
                       ?? EditorGUIUtility.ObjectContent(asset.asset, asset.asset.GetType()).image,
                severity = severity,
                errorCount = asset.errorCount,
                warningCount = asset.warningCount,
            });
            LinkParentChild(parent, node, insertIndex);
            return node;
        }

        public void InsertIssueNodes(TriValidationNode parent, Object target, List<TriObjectValidationResult> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                return;
            }

            foreach (var obj in issues)
            {
                foreach (var issue in obj.issues)
                {
                    var issueNode = new TriValidationNode(_nextId++, MakeIssueData(
                        target, issue.result.Message, issue.result.MessageType, issue.property?.PropertyPath));
                    LinkParentChild(parent, issueNode, insertIndex: null);
                }
            }
        }

        public TriValidationNode InsertSceneNode(Scene scene, string assetPath, out int insertIndex)
        {
            var isPreviewScene = EditorSceneManager.IsPreviewScene(scene);
            var isUntitled = string.IsNullOrEmpty(scene.path);

            var target = !isPreviewScene && !isUntitled
                ? AssetDatabase.LoadAssetAtPath<Object>(assetPath)
                : null;

            var label = string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
            var icon = isPreviewScene
                ? (AssetDatabase.GetCachedIcon(assetPath)
                   ?? EditorGUIUtility.IconContent("Prefab Icon").image)
                : EditorGUIUtility.IconContent("SceneAsset Icon").image;

            var node = new TriValidationNode(_nextId++, new TriValidationNodeData
            {
                kind = TriValidationNodeKind.Scene,
                isRoot = true,
                label = label,
                icon = icon,
                target = target,
            })
            {
                Scene = scene,
            };
            insertIndex = TriValidationTreeOrdering.ComputeRootIndex(Roots, node);
            LinkParentChild(Root, node, insertIndex);
            return node;
        }

        public TriValidationNode InsertIssueNode(TriValidationNode componentNode, TriValidationIssueKey key,
            out int insertIndex)
        {
            insertIndex = componentNode.Children.Count;

            var data = MakeIssueData(
                componentNode.Data.target, key.message, key.severity, key.propertyPath);
            var node = new TriValidationNode(_nextId++, data);
            LinkParentChild(componentNode, node, insertIndex);
            return node;
        }

        public static void LinkParentChild(TriValidationNode parent, TriValidationNode child, int? insertIndex = null)
        {
            child.Parent = parent;
            var children = parent.Children;
            children.Insert(insertIndex ?? children.Count, child);
        }

        public static TriValidationNodeData MakeIssueData(
            Object target, string message, TriMessageType severity, string path)
        {
            return new TriValidationNodeData
            {
                kind = TriValidationNodeKind.Issue,
                label = message,
                target = target,
                severity = severity,
                icon = severity.GetIcon(),
                propertyPath = path,
                errorCount = severity == TriMessageType.Error ? 1 : 0,
                warningCount = severity == TriMessageType.Warning ? 1 : 0,
                infoCount = severity == TriMessageType.Info ? 1 : 0,
            };
        }
    }
}