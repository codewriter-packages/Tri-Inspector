using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TriInspector.Validator
{
    internal static class TriAssetValidator
    {
        public static bool TryValidate(Object asset, out TriAssetValidationResult validation)
        {
            validation = default;

            if (asset == null)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            switch (asset)
            {
                case ScriptableObject so:
                    return TryValidateScriptableObject(so, path, out validation);

                case GameObject root:
                    return TryValidateGameObject(root, path, out validation);

                case SceneAsset:
                    Debug.LogError($"{nameof(TriAssetValidator)} not supports {nameof(SceneAsset)} validation");
                    return false;

                default:
                    return false;
            }
        }

        public static bool TryValidateScriptableObject(ScriptableObject so, string path,
            out TriAssetValidationResult validation)
        {
            validation = default;

            if (!TriObjectValidator.TryValidateScriptableObject(so, out var soResult))
            {
                return false;
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            validation = new TriAssetValidationResult
            {
                assetPath = path,
                label = AssetDatabase.IsMainAsset(so) ? fileName : $"{fileName} / {so.name}",
                asset = so,
                objects = new List<TriObjectValidationResult>
                {
                    soResult,
                },
            };
            return true;
        }

        public static bool TryValidateGameObject(GameObject root, string path, out TriAssetValidationResult validation)
        {
            validation = default;

            var errorCount = 0;
            var warningCount = 0;
            var perObjIssues = new List<TriObjectValidationResult>();

            using (ListPool<MonoBehaviour>.Get(out var behaviours))
            {
                root.GetComponentsInChildren(true, behaviours);
                foreach (var behaviour in behaviours)
                {
                    if (!TriObjectValidator.TryValidateComponent(behaviour, out var objResult))
                    {
                        continue;
                    }

                    perObjIssues.Add(objResult);

                    foreach (var issue in objResult.issues)
                    {
                        switch (issue.result.MessageType)
                        {
                            case TriMessageType.Error:
                                errorCount++;
                                break;
                            case TriMessageType.Warning:
                                warningCount++;
                                break;
                        }
                    }
                }
            }

            if (errorCount + warningCount == 0)
            {
                return false;
            }

            validation = new TriAssetValidationResult
            {
                assetPath = path,
                label = root.name,
                asset = root,
                objects = perObjIssues,
                countOnlyKind = TriValidationNodeKind.PrefabAsset,
                errorCount = errorCount,
                warningCount = warningCount,
            };
            return true;
        }
    }
}