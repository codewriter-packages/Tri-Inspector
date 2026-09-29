using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    [Serializable]
    internal class TriValidationNodeData
    {
        public TriValidationNodeKind kind;
        public bool isRoot;
        public string label;
        public Texture icon;
        public Object target;
        public TriMessageType severity;
        public string propertyPath;

        public int errorCount;
        public int warningCount;
        public int infoCount;
    }
}