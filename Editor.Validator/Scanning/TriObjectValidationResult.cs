using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    public struct TriObjectValidationResult
    {
        public string objType;

        public string globalId;

        public List<string> hierarchyPath;
        public List<TriValidationIssue> issues;

        public Object obj;
    }
}