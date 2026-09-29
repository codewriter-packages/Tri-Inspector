using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace TriInspector.Validator
{
    public static class TriValidator
    {
        private const int SchemaVersion = 1;

        public static string ValidateProjectAsJson(bool prettyPrint = true)
        {
            using (ListPool<TriAssetValidationResult>.Get(out var tempIssues))
            {
                ValidateProject(tempIssues);
                return JsonUtility.ToJson(BuildReport(tempIssues), prettyPrint);
            }
        }

        public static List<TriAssetValidationResult> ValidateProject()
        {
            var results = new List<TriAssetValidationResult>();
            ValidateProject(results);
            return results;
        }

        public static void ValidateProject(List<TriAssetValidationResult> issues)
        {
            TriValidationForegroundJob.Run(TriAssetValidationScanner.ScanSteps(issues));
        }

        public static string ValidateOpenedScenesAsJson(bool prettyPrint = true)
        {
            using (ListPool<TriAssetValidationResult>.Get(out var tempIssues))
            {
                ValidateOpenedScenes(tempIssues);
                return JsonUtility.ToJson(BuildReport(tempIssues), prettyPrint);
            }
        }

        public static List<TriAssetValidationResult> ValidateOpenedScenes()
        {
            var results = new List<TriAssetValidationResult>();
            ValidateOpenedScenes(results);
            return results;
        }

        public static void ValidateOpenedScenes(List<TriAssetValidationResult> results)
        {
            results.Clear();

            var sceneResults = new Dictionary<Scene, List<TriObjectValidationResult>>();
            TriValidationForegroundJob.Run(TriOpenedSceneValidationScanner.ScanSteps(sceneResults));

            foreach (var pair in sceneResults)
            {
                var scene = pair.Key;
                var objects = pair.Value;
                if (objects == null || objects.Count == 0)
                {
                    continue;
                }

                var errors = 0;
                var warnings = 0;
                foreach (var obj in objects)
                {
                    if (obj.issues == null)
                    {
                        continue;
                    }

                    foreach (var issue in obj.issues)
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

                results.Add(new TriAssetValidationResult
                {
                    // Unsaved scenes have no asset path; the name is the only identifier we have.
                    assetPath = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path,
                    objects = objects,
                    countOnlyKind = TriValidationNodeKind.SceneAsset,
                    errorCount = errors,
                    warningCount = warnings,
                });
            }
        }

        public static string ValidateAssetAsJson(string globalId, bool prettyPrint = true)
        {
            var results = new List<TriAssetValidationResult>();
            ValidateAsset(globalId, results);
            return JsonUtility.ToJson(BuildReport(results), prettyPrint);
        }

        public static List<TriAssetValidationResult> ValidateAsset(string globalId)
        {
            var results = new List<TriAssetValidationResult>();
            ValidateAsset(globalId, results);
            return results;
        }

        public static void ValidateAsset(string globalId, List<TriAssetValidationResult> results)
        {
            results.Clear();

            if (string.IsNullOrEmpty(globalId))
            {
                throw new ArgumentException("globalId is required.", nameof(globalId));
            }

            if (!GlobalObjectId.TryParse(globalId, out var id))
            {
                throw new ArgumentException($"Could not parse GlobalObjectId '{globalId}'.", nameof(globalId));
            }

            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            if (obj == null)
            {
                throw new ArgumentException($"GlobalObjectId '{globalId}' did not resolve to an object.",
                    nameof(globalId));
            }

            // Resolve to the main asset so a component/sub-object id still validates its owning asset.
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"GlobalObjectId '{globalId}' does not point at a project asset.",
                    nameof(globalId));
            }

            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            var target = mainAsset != null ? mainAsset : obj;

            if (target is SceneAsset)
            {
                throw new ArgumentException(
                    "Validating a SceneAsset by id is not supported; open the scene and use ValidateOpenedScenes.",
                    nameof(globalId));
            }

            if (TriAssetValidator.TryValidate(target, out var validation))
            {
                results.Add(validation);
            }
        }

        private static ReportJson BuildReport(List<TriAssetValidationResult> results)
        {
            var report = new ReportJson
            {
                schemaVersion = SchemaVersion,
                assets = new List<AssetJson>(results.Count),
            };

            foreach (var asset in results)
            {
                var assetJson = new AssetJson
                {
                    assetPath = asset.assetPath,
                    kind = KindOf(asset.countOnlyKind),
                    objects = new List<ObjectJson>(asset.objects?.Count ?? 0),
                };

                if (asset.objects != null)
                {
                    foreach (var obj in asset.objects)
                    {
                        var objJson = new ObjectJson
                        {
                            objType = obj.objType,
                            globalId = obj.globalId,
                            hierarchyPath = obj.hierarchyPath,
                            issues = new List<IssueJson>(obj.issues?.Count ?? 0),
                        };

                        if (obj.issues != null)
                        {
                            foreach (var issue in obj.issues)
                            {
                                switch (issue.result.MessageType)
                                {
                                    case TriMessageType.Error:
                                        assetJson.errorCount++;
                                        break;
                                    case TriMessageType.Warning:
                                        assetJson.warningCount++;
                                        break;
                                }

                                objJson.issues.Add(new IssueJson
                                {
                                    propertyPath = issue.property?.PropertyPath,
                                    severity = SeverityOf(issue.result.MessageType),
                                    message = issue.result.Message,
                                });
                            }
                        }

                        assetJson.objects.Add(objJson);
                    }
                }

                report.errorCount += assetJson.errorCount;
                report.warningCount += assetJson.warningCount;
                report.assets.Add(assetJson);
            }

            report.assetCount = report.assets.Count;
            report.valid = report.errorCount == 0;
            return report;
        }

        private static string KindOf(TriValidationNodeKind? kind)
        {
            return kind switch
            {
                TriValidationNodeKind.SceneAsset => "scene",
                TriValidationNodeKind.PrefabAsset => "prefab",
                _ => "asset",
            };
        }

        private static string SeverityOf(TriMessageType type)
        {
            return type switch
            {
                TriMessageType.Error => "error",
                TriMessageType.Warning => "warning",
                TriMessageType.Info => "info",
                _ => "none",
            };
        }

        [Serializable]
        private struct ReportJson
        {
            public int schemaVersion;

            // True when the project has no validation errors. Warnings do not affect this flag, so a
            // consumer can gate on `valid` for build-blocking issues and still inspect `warningCount`.
            public bool valid;

            public int errorCount;
            public int warningCount;
            public int assetCount;
            public List<AssetJson> assets;
        }

        [Serializable]
        private struct AssetJson
        {
            public string assetPath;

            // "scene" | "prefab" | "asset" (ScriptableObject and other main assets).
            public string kind;

            public int errorCount;
            public int warningCount;
            public List<ObjectJson> objects;
        }

        [Serializable]
        private struct ObjectJson
        {
            public string objType;
            public string globalId;
            public List<string> hierarchyPath;
            public List<IssueJson> issues;
        }

        [Serializable]
        private struct IssueJson
        {
            public string propertyPath;

            // "error" | "warning" | "info".
            public string severity;

            public string message;
        }
    }
}
