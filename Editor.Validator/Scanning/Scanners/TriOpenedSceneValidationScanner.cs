using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace TriInspector.Validator
{
    internal static class TriOpenedSceneValidationScanner
    {
        public static IEnumerator<TriValidationScanStep> ScanSteps(
            Dictionary<Scene, List<TriObjectValidationResult>> results)
        {
            results.Clear();

            var settings = TriValidatorSettings.instance;

            var total = CountOpenedSceneBehaviours(settings);
            var processed = 0;

            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);

                if (!scene.IsValid() || !scene.isLoaded || !settings.IsInScope(scene))
                {
                    continue;
                }

                var sceneResults = new List<TriObjectValidationResult>();

                var sceneName = string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
                foreach (var rootGo in scene.GetRootGameObjects())
                {
                    foreach (var owner in ValidateHierarchySteps(rootGo, sceneResults))
                    {
                        processed++;

                        yield return new TriValidationScanStep(owner.name, processed, total);
                    }
                }

                if (sceneResults.Count > 0)
                {
                    results.Add(scene, sceneResults);
                }
            }
        }

        public static IEnumerator<TriValidationScanStep> ScanSceneSteps(Scene scene,
            List<TriObjectValidationResult> results)
        {
            results.Clear();

            if (!scene.IsValid() || !scene.isLoaded || !TriValidatorSettings.instance.IsInScope(scene))
            {
                yield break;
            }

            var roots = scene.GetRootGameObjects();

            var total = 0;
            foreach (var root in roots)
            {
                total += CountBehaviours(root);
            }

            var processed = 0;
            foreach (var root in roots)
            {
                foreach (var owner in ValidateHierarchySteps(root, results))
                {
                    processed++;

                    yield return new TriValidationScanStep(owner.name, processed, total);
                }
            }
        }

        public static IEnumerator<TriValidationScanStep> ScanRootSteps(GameObject root,
            List<TriObjectValidationResult> results)
        {
            results.Clear();

            if (root == null)
            {
                yield break;
            }

            var total = CountBehaviours(root);
            var processed = 0;

            foreach (var owner in ValidateHierarchySteps(root, results))
            {
                processed++;

                yield return new TriValidationScanStep(owner.name, processed, total);
            }
        }

        private static int CountOpenedSceneBehaviours(TriValidatorSettings settings)
        {
            var total = 0;
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.IsValid() || !scene.isLoaded || !settings.IsInScope(scene))
                {
                    continue;
                }

                foreach (var root in scene.GetRootGameObjects())
                {
                    total += CountBehaviours(root);
                }
            }

            return total;
        }

        private static int CountBehaviours(GameObject root)
        {
            using (ListPool<MonoBehaviour>.Get(out var buffer))
            {
                root.GetComponentsInChildren(true, buffer);
                return buffer.Count;
            }
        }

        private static IEnumerable<GameObject> ValidateHierarchySteps(
            GameObject go, List<TriObjectValidationResult> results)
        {
            if (go == null)
            {
                yield break;
            }

            using (ListPool<MonoBehaviour>.Get(out var behaviours))
            {
                go.GetComponents(behaviours);
                foreach (var behaviour in behaviours)
                {
                    if (behaviour == null)
                    {
                        continue;
                    }

                    if (TriObjectValidator.TryValidateComponent(behaviour, out var behResult))
                    {
                        results.Add(behResult);
                    }

                    yield return go;
                }
            }

            if (go == null)
            {
                yield break;
            }

            for (var childIndex = 0; childIndex < go.transform.childCount; childIndex++)
            {
                var child = go.transform.GetChild(childIndex);
                foreach (var owner in ValidateHierarchySteps(child.gameObject, results))
                {
                    yield return owner;
                }
            }
        }
    }
}