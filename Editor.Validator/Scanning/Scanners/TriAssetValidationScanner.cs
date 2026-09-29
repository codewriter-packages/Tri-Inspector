using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace TriInspector.Validator
{
    internal static class TriAssetValidationScanner
    {
        public static IEnumerator<TriValidationScanStep> ScanSteps(List<TriAssetValidationResult> results)
        {
            results.Clear();

            TriValidationProfiler.BeginScan();

            var settings = TriValidatorSettings.instance;

            var allPaths = AssetDatabase.GetAllAssetPaths();
            for (var i = 0; i < allPaths.Length; i++)
            {
                var path = allPaths[i];

                if (!settings.IsInScope(path))
                {
                    continue;
                }

                var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (mainType == null || settings.IsExcludedType(mainType))
                {
                    continue;
                }

                yield return new TriValidationScanStep(path, i, allPaths.Length);

                if (mainType == typeof(SceneAsset))
                {
                    ScanClosedSceneAsset(path, AssetDatabase.AssetPathToGUID(path), results);
                    continue;
                }

                foreach (var loaded in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (loaded == null)
                    {
                        continue;
                    }

                    if (loaded is ScriptableObject so &&
                        TriAssetValidator.TryValidateScriptableObject(so, path, out var soValidation))
                    {
                        results.Add(soValidation);
                    }
                    else if (loaded is GameObject root && AssetDatabase.IsMainAsset(root) &&
                             TriAssetValidator.TryValidateGameObject(root, path, out var goValidation))
                    {
                        results.Add(goValidation);
                    }
                }
            }

            TriValidationProfiler.EndScanAndReport();
        }

        private static void ScanClosedSceneAsset(string path, string guid, List<TriAssetValidationResult> results)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                var errors = 0;
                var warnings = 0;
                var objects = default(List<TriObjectValidationResult>);

                using (ListPool<TriObjectValidationResult>.Get(out var sceneObjResults))
                {
                    var steps = TriOpenedSceneValidationScanner.ScanSceneSteps(scene, sceneObjResults);
                    while (steps.MoveNext())
                    {
                    }

                    foreach (var sceneObjResult in sceneObjResults)
                    {
                        if (sceneObjResult.issues == null)
                        {
                            continue;
                        }

                        objects ??= new List<TriObjectValidationResult>();
                        objects.Add(sceneObjResult);

                        foreach (var issue in sceneObjResult.issues)
                        {
                            switch (issue.result.MessageType)
                            {
                                case TriMessageType.Error:
                                    errors++;
                                    break;
                                case TriMessageType.Warning:
                                    warnings++;
                                    break;
                            }
                        }
                    }
                }

                TriSceneIssueCache.Record(guid, errors, warnings);

                if (errors + warnings <= 0)
                {
                    return;
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (sceneAsset == null)
                {
                    return;
                }

                results.Add(new TriAssetValidationResult
                {
                    assetPath = path,
                    label = sceneAsset.name,
                    asset = sceneAsset,
                    objects = objects,
                    countOnlyKind = TriValidationNodeKind.SceneAsset,
                    errorCount = errors,
                    warningCount = warnings,
                });
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}