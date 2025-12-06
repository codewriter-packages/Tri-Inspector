#if TRI_MODULE_ANIMATION

using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(AnimatorParameterAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class AnimatorParameterAttributeDrawer : TriAttributeDrawer<AnimatorParameterAttribute>
    {
        private AnimatorParameterHelper.ResolvedParams _resolvedParams;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvedParams = AnimatorParameterHelper.Initialize(propertyDefinition, Attribute);

            if (_resolvedParams.ErrorResult.IsError)
            {
                return TriExtensionInitializationResult.Skip;
            }

            if (propertyDefinition.FieldType != typeof(string) &&
                propertyDefinition.FieldType != typeof(int))
            {
                return "[AnimationParameter] can only be used on 'string' or 'int' fields.";
            }

            return TriExtensionInitializationResult.Ok;
        }
        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var animator = _resolvedParams.AnimatorResolver.GetValue(property);
            var label = property.DisplayNameContent;
            var (allParameters, allFilteredParameters) = AnimatorParameterHelper.GetParameters(animator, Attribute.ParameterType);

            if (property.ValueType == typeof(string))
            {
                DrawPopup(
                    position,
                    label,
                    property,
                    animator,
                    (string)property.Value,
                    allParameters.Names,
                    allFilteredParameters.Names,
                    allFilteredParameters.DisplayNames,
                    allFilteredParameters.Types,
                    AnimatorParameterHelper.GetInvalidParameterLabel
                );
            }
            else if (property.ValueType == typeof(int))
            {
                DrawPopup(
                    position,
                    label,
                    property,
                    animator,
                    (int)property.Value,
                    allParameters.Hashes,
                    allFilteredParameters.Hashes,
                    allFilteredParameters.DisplayNames,
                    allFilteredParameters.Types,
                    AnimatorParameterHelper.GetInvalidParameterLabel
                );
            }
        }
        private void DrawPopup<T>(
            Rect position,
            GUIContent label,
            TriProperty property,
            Animator animator,
            T currentValue,
            T[] allValues,
            T[] filteredValues,
            GUIContent[] displayNames,
            AnimatorControllerParameterType[] types,
            Func<Animator, T, string> invalidLabelFunc)
        {
            bool isEmpty = EqualityComparer<T>.Default.Equals(currentValue, default);
            bool exists = allValues.Contains(currentValue);
            bool matchesType = filteredValues.Contains(currentValue);
            bool isValid = isEmpty || (exists && matchesType);

            var displayList = new List<GUIContent>(displayNames);
            var valueList = new List<T>(filteredValues);

            int currentIndex = 0;

            if (!isValid)
            {
                displayList.Add(new GUIContent(invalidLabelFunc(animator, currentValue)));
                valueList.Add(currentValue);
                currentIndex = valueList.Count - 1;
            }
            else
            {
                currentIndex = Array.IndexOf(filteredValues, currentValue);
                if (currentIndex < 0) currentIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label, currentIndex, displayList.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex < 0 || newIndex >= valueList.Count)
                    return;

                T selectedValue = valueList[newIndex];
                var selectedType = types[Mathf.Min(newIndex, types.Length - 1)];
                string selectedName = selectedValue is string s ? s : AllParametersName(animator, selectedValue);

                property.SetValue(selectedValue);

                AnimatorParameterHelper.SaveSingleParameter(
                    animator,
                    selectedName,
                    selectedType,
                    Animator.StringToHash(selectedName)
                );
            }
        }

        private string AllParametersName(Animator animator, object value)
        {
            if (value is int hash)
                return animator.parameters.FirstOrDefault(p => p.nameHash == hash)?.name ?? hash.ToString();
            return value?.ToString();
        }
    }

    #region Helper Class

    internal static class AnimatorParameterHelper
    {
        internal class ResolvedParams
        {
            public ValueResolver<Animator> AnimatorResolver;
            public TriExtensionInitializationResult ErrorResult;
        }

        internal struct ParameterData
        {
            public GUIContent[] DisplayNames;
            public string[] Names;
            public int[] Hashes;
            public AnimatorControllerParameterType[] Types;

            public static ParameterData Empty => new()
            {
                DisplayNames = new[] { new GUIContent("None") },
                Names = new string[] { null },
                Hashes = new[] { 0 },
                Types = new AnimatorControllerParameterType[] { 0 }
            };
        }
        
        [Serializable]
        private class CacheData
        {
            public List<ProjectEntry> projects = new();

            public static CacheData Empty => new()
            {
                projects = new List<ProjectEntry>()
                {
                    new()
                    {
                        hash = Application.dataPath.GetHashCode(),
                        animators = new List<AnimatorEntry>()
                    }
                }
            };
        }
        [Serializable]
        private class ProjectEntry
        {
            public int hash;
            public List<AnimatorEntry> animators = new();
        }

        [Serializable]
        private class AnimatorEntry
        {
            public string guid;
            public List<ParameterEntry> parameters = new();
        }

        [Serializable]
        private class ParameterEntry
        {
            public int hash;
            public string name;
            public AnimatorControllerParameterType type;
        }

        private static string CacheEditorPrefsKey => $"TriInspector.AnimationParameterCache";

        private static CacheData _globalCache;

        private static void LoadCache()
        {
            if (_globalCache != null) return;

            string json = EditorPrefs.GetString(CacheEditorPrefsKey, null);

            try
            {
                if (!string.IsNullOrEmpty(json))
                    _globalCache = JsonUtility.FromJson<CacheData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TriInspector.AnimationParameter] Could not load parameter cache: {e.Message}");
            }
            finally
            {
                _globalCache ??= CacheData.Empty;
            }
        }
        private static void ClearCacheMenu()
        {
            EditorPrefs.DeleteKey(CacheEditorPrefsKey);
            _globalCache = null;
            Debug.Log("[TriInspector.AnimatorProperty] Cache cleared.");
        }
        public static void SaveSingleParameter(Animator animator, string name, AnimatorControllerParameterType type, int hash)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            string guid = GetAnimatorGuid(animator);
            if (string.IsNullOrEmpty(guid))
                return;

            int currentProjectHash = Application.dataPath.GetHashCode();

            LoadCache();

            var projectData = _globalCache.projects.FirstOrDefault(p => p.hash == currentProjectHash);
            if (projectData == null)
            {
                projectData = new ProjectEntry { hash = currentProjectHash };
                _globalCache.projects.Add(projectData);
            }

            var animatorData = projectData.animators.FirstOrDefault(a => a.guid == guid);
            if (animatorData == null)
            {
                animatorData = new AnimatorEntry { guid = guid };
                projectData.animators.Add(animatorData);
            }

            var validParameterNames = animator.parameters.Select(p => p.name).ToHashSet();
            var validParameterHashes = animator.parameters.Select(p => p.nameHash).ToHashSet();
            animatorData.parameters.RemoveAll(p =>
                !validParameterNames.Contains(p.name) && !validParameterHashes.Contains(p.hash));

            var foundEntry = animatorData.parameters.FirstOrDefault(e => e.hash == hash || e.name == name);

            foundEntry ??= new ParameterEntry();
            foundEntry.hash = hash;
            foundEntry.name = name;
            foundEntry.type = type;

            if (!animatorData.parameters.Contains(foundEntry))
                animatorData.parameters.Add(foundEntry);

            string json = JsonUtility.ToJson(_globalCache, true);
            EditorPrefs.SetString(CacheEditorPrefsKey, json);
        }
        public static ResolvedParams Initialize(TriPropertyDefinition propertyDefinition,
            AnimatorParameterAttribute attribute)
        {
            var resolved = new ResolvedParams();
            resolved.AnimatorResolver = ValueResolver.Resolve<Animator>(propertyDefinition,
                attribute.AnimatorFieldName);

            resolved.ErrorResult = resolved.AnimatorResolver.TryGetErrorString(out var error)
                ? error
                : TriExtensionInitializationResult.Ok;

            return resolved;
        }
        public static (ParameterData allParameters, ParameterData allFilteredParameters) GetParameters(Animator animator, AnimatorControllerParameterType? filter)
        {
            ParameterData allParameters = ParameterData.Empty;
            ParameterData allFilteredParameters = ParameterData.Empty;

            if (animator == null || animator.runtimeAnimatorController == null)
                return (allParameters, allFilteredParameters);

            var parameters = animator.parameters;
            if (parameters == null || parameters.Length == 0)
                return (allParameters, allFilteredParameters);

            allParameters = BuildParameters(parameters);

            if (!filter.HasValue)
                allFilteredParameters = allParameters;
            else
                allFilteredParameters = BuildParameters(parameters.Where(p => p.type == filter.Value).ToArray());


            return (allParameters, allFilteredParameters);
        }
        public static string GetInvalidParameterLabel(Animator animator, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return "None";

            string guid = GetAnimatorGuid(animator);
            if (string.IsNullOrEmpty(guid))
                return $"{parameterName} (Missing)";

            int projectHash = Application.dataPath.GetHashCode();
            LoadCache();

            var project = _globalCache?.projects?.FirstOrDefault(p => p.hash == projectHash);
            var animatorData = project?.animators?.FirstOrDefault(a => a.guid == guid);
            var match = animatorData?.parameters?.FirstOrDefault(p => p.name == parameterName);

            return match != null
                ? $"{parameterName} ({match.type})"
                : $"{parameterName} (Missing)";
        }
        public static string GetInvalidParameterLabel(Animator animator, int parameterHash)
        {
            if (parameterHash == 0)
                return "None";

            string guid = GetAnimatorGuid(animator);
            if (string.IsNullOrEmpty(guid))
                return $"Unknown Hash ({parameterHash})";

            int projectHash = Application.dataPath.GetHashCode();
            LoadCache();

            var project = _globalCache?.projects?.FirstOrDefault(p => p.hash == projectHash);
            var animatorData = project?.animators?.FirstOrDefault(a => a.guid == guid);
            var match = animatorData?.parameters?.FirstOrDefault(p => p.hash == parameterHash);

            return match != null
                ? $"{match.name} ({match.type})"
                : $"Unknown Hash ({parameterHash})";
        }
        private static string GetAnimatorGuid(Animator animator)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return null;

            var controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (string.IsNullOrEmpty(controllerPath))
                return null;

            return AssetDatabase.AssetPathToGUID(controllerPath);
        }

        private static ParameterData BuildParameters(AnimatorControllerParameter[] parameters)
        {
            var displayNames = new List<GUIContent>();
            var names = new List<string>();
            var hashes = new List<int>();
            var types = new List<AnimatorControllerParameterType>();

            displayNames.Add(new GUIContent("None"));
            names.Add(null);
            hashes.Add(0);
            types.Add(0);

            foreach (var param in parameters)
            {
                displayNames.Add(new GUIContent($"{param.name} ({param.type})", EditorGUIUtility.IconContent("UnityEditor.Graphs.AnimatorControllerTool").image));
                names.Add(param.name);
                hashes.Add(param.nameHash);
                types.Add(param.type);
            }

            return new ParameterData
            {
                DisplayNames = displayNames.ToArray(),
                Names = names.ToArray(),
                Hashes = hashes.ToArray(),
                Types = types.ToArray()
            };
        }

    }

    #endregion
}

#endif