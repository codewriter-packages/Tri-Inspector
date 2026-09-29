using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    internal static class TriObjectValidator
    {
        public static bool TryValidateComponent(MonoBehaviour behaviour, out TriObjectValidationResult result)
        {
            if (!TryValidate(behaviour, out result))
            {
                return false;
            }

            var path = GetHierarchyPath(behaviour.gameObject);
            result.hierarchyPath = path;
            return true;
        }

        public static bool TryValidateScriptableObject(ScriptableObject so, out TriObjectValidationResult result)
        {
            if (!TryValidate(so, out result))
            {
                return false;
            }

            result.hierarchyPath = new List<string> {so.name,};
            return true;
        }

        private static bool TryValidate(Object obj, out TriObjectValidationResult result)
        {
            var issues = Validate(obj);
            if (issues == null || issues.Count == 0)
            {
                result = default;
                return false;
            }

            result = new TriObjectValidationResult
            {
                obj = obj,
                objType = obj.GetType().Name,
                globalId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString(),
                issues = issues,
            };
            return true;
        }

        private static List<TriValidationIssue> Validate(Object target)
        {
            if (target == null)
            {
                return null;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            using var propertyTree = new TriPropertyTreeForValidation(target);
            propertyTree.Update(forceUpdate: false);
            propertyTree.RunValidation();

            List<TriValidationIssue> issues = null;
            propertyTree.EnumerateValidationResults((property, validationResult) =>
            {
                if (validationResult.MessageType != TriMessageType.Error &&
                    validationResult.MessageType != TriMessageType.Warning)
                {
                    return;
                }

                issues ??= new List<TriValidationIssue>();
                issues.Add(new TriValidationIssue
                {
                    property = property,
                    result = validationResult,
                });
            });

            TriValidationProfiler.Record(target.GetType().Name, sw.ElapsedTicks);

            return issues;
        }

        private static List<string> GetHierarchyPath(GameObject go)
        {
            var path = new List<string>();

            for (var current = go.transform; current != null; current = current.parent)
            {
                path.Add(current.name);
            }

            path.Reverse();
            return path;
        }
    }
}