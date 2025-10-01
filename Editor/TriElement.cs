using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public class TriElement
    {
        private static readonly List<TriElement> Empty = new List<TriElement>();

        private float _cachedHeight;
        private bool _attached;
        private List<TriElement> _children = Empty;

        [PublicAPI]
        public int ChildrenCount => _children.Count;

        public bool IsAttached => _attached;

        internal float CachedHeight => _cachedHeight;

        [PublicAPI]
        public virtual bool Update()
        {
            if (!_attached)
            {
                Debug.LogError($"{GetType().Name} not attached");
            }

            var dirty = false;

            foreach (var child in _children)
            {
                dirty |= child.Update();
            }

            return dirty;
        }

        [PublicAPI]
        public virtual float GetHeight(float width)
        {
            if (!_attached)
            {
                Debug.LogError($"{GetType().Name} not attached");
            }

            if (Event.current.type != EventType.Layout)
            {
                return _cachedHeight;
            }

            switch (_children.Count)
            {
                case 0:
                    return _cachedHeight = 0f;

                case 1:
                    return _cachedHeight = _children[0].GetHeight(width);

                default:
                {
                    _cachedHeight = (_children.Count - 1) * EditorGUIUtility.standardVerticalSpacing;

                    foreach (var child in _children)
                    {
                        _cachedHeight += child.GetHeight(width);
                    }

                    return _cachedHeight;
                }
            }
        }

        [PublicAPI]
        public virtual void OnGUI(Rect position)
        {
            if (!_attached)
            {
                Debug.LogError($"{GetType().Name} not attached");
            }

            switch (_children.Count)
            {
                case 0:
                    break;

                case 1:
                    _children[0].OnGUI(position);
                    break;

                default:
                {
                    var offset = 0f;
                    var spacing = EditorGUIUtility.standardVerticalSpacing;

                    foreach (var child in _children)
                    {
                        var childHeight = child.GetHeight(position.width);

                        child.OnGUI(new Rect(position.x, position.y + offset, position.width, childHeight));

                        offset += childHeight + spacing;
                    }

                    break;
                }
            }
        }

        [PublicAPI]
        public TriElement GetChild(int index)
        {
            return _children[index];
        }

        [PublicAPI]
        public void RemoveChildAt(int index)
        {
            if (_children.Count < index)
            {
                return;
            }

            var child = _children[index];
            _children.RemoveAt(index);

            if (_attached)
            {
                child.DetachInternal();
            }
        }

        [PublicAPI]
        public void RemoveAllChildren()
        {
            if (_attached)
            {
                foreach (var child in _children)
                {
                    child.DetachInternal();
                }
            }

            _children.Clear();
        }

        [PublicAPI]
        public void AddChild(TriElement child)
        {
            if (_children == Empty)
            {
                _children = new List<TriElement>();
            }

            _children.Add(child);

            if (_attached)
            {
                child.AttachInternal();
                child.Update();
            }
        }

        internal void AttachInternal()
        {
            if (_attached)
            {
                Debug.LogError($"{GetType().Name} already attached");
            }

            _attached = true;

            OnAttachToPanel();

            foreach (var child in _children)
            {
                child.AttachInternal();
                child.Update();
            }
        }

        internal void DetachInternal()
        {
            if (!_attached)
            {
                Debug.LogError($"{GetType().Name} not attached");
            }

            _attached = false;

            foreach (var child in _children)
            {
                child.DetachInternal();
            }

            OnDetachFromPanel();
        }

        protected virtual void OnAttachToPanel()
        {
        }

        protected virtual void OnDetachFromPanel()
        {
        }
    }
}