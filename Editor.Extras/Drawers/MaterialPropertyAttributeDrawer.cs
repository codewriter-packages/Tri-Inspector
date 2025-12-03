using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: RegisterTriAttributeDrawer(typeof(MaterialPropertyAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class MaterialPropertyAttributeDrawer : TriAttributeDrawer<MaterialPropertyAttribute>
    {
        private MaterialPropertyHelper.ResolvedParams _resolvedParams;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvedParams = MaterialPropertyHelper.Initialize(propertyDefinition, Attribute);

            if (_resolvedParams.ErrorResult.IsError)
                return TriExtensionInitializationResult.Skip;

            if (propertyDefinition.FieldType != typeof(string) &&
                propertyDefinition.FieldType != typeof(int))
                return "[MaterialProperty] can only be used on 'string' or 'int' fields.";

            return TriExtensionInitializationResult.Ok;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var material = _resolvedParams.MaterialResolver.GetValue(property);
            var label = property.DisplayNameContent;
            var (allProperties, filteredProperties) = MaterialPropertyHelper.GetProperties(material, Attribute.PropertyType);

            if (property.ValueType == typeof(string))
            {
                DrawPopup(
                    position,
                    label,
                    property,
                    material,
                    (string)property.Value,
                    allProperties.Names,
                    filteredProperties.Names,
                    filteredProperties.DisplayNames,
                    filteredProperties.Types,
                    MaterialPropertyHelper.GetInvalidPropertyLabel
                );
            }
            else if (property.ValueType == typeof(int))
            {
                DrawPopup(
                    position,
                    label,
                    property,
                    material,
                    (int)property.Value,
                    allProperties.Hashes,
                    filteredProperties.Hashes,
                    filteredProperties.DisplayNames,
                    filteredProperties.Types,
                    MaterialPropertyHelper.GetInvalidPropertyLabel
                );
            }
        }

        private void DrawPopup<T>(
            Rect position,
            GUIContent label,
            TriProperty property,
            Material material,
            T currentValue,
            T[] allValues,
            T[] filteredValues,
            GUIContent[] displayNames,
            ShaderPropertyType[] types,
            Func<Material, T, string> invalidLabelFunc)
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
                displayList.Add(new GUIContent(invalidLabelFunc(material, currentValue)));
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
                string selectedName = selectedValue is string s ? s : MaterialPropertyHelper.ResolvePropertyName(material, selectedValue);

                property.SetValue(selectedValue);

                MaterialPropertyHelper.SaveSingleProperty(
                    material,
                    selectedName,
                    selectedType,
                    Shader.PropertyToID(selectedName)
                );
            }
        }
    }

    #region Helper Class

    internal static class MaterialPropertyHelper
    {
        internal class ResolvedParams
        {
            public ValueResolver<Material> MaterialResolver;
            public TriExtensionInitializationResult ErrorResult;
        }

        internal struct PropertyData
        {
            public GUIContent[] DisplayNames;
            public string[] Names;
            public int[] Hashes;
            public ShaderPropertyType[] Types;

            public static PropertyData Empty => new()
            {
                DisplayNames = new[] { new GUIContent("None") },
                Names = new string[] { null },
                Hashes = new int[] { 0 },
                Types = new ShaderPropertyType[] { (ShaderPropertyType)0 }
            };
        }

        public static ResolvedParams Initialize(TriPropertyDefinition propertyDefinition, MaterialPropertyAttribute attribute)
        {
            var resolved = new ResolvedParams
            {
                MaterialResolver = ValueResolver.Resolve<Material>(propertyDefinition, attribute.MaterialFieldName)
            };

            resolved.ErrorResult = resolved.MaterialResolver.TryGetErrorString(out var error)
                ? error
                : TriExtensionInitializationResult.Ok;

            return resolved;
        }

        private static string CacheEditorPrefsKey => "TriInspector.MaterialPropertyCache";

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
                        materials = new List<MaterialEntry>()
                    }
                }
            };
        }

        [Serializable]
        private class ProjectEntry
        {
            public int hash;
            public List<MaterialEntry> materials = new();
        }

        [Serializable]
        private class MaterialEntry
        {
            public string guid;
            public List<PropertyEntry> properties = new();
        }

        [Serializable]
        private class PropertyEntry
        {
            public int id;
            public string name;
            public ShaderPropertyType type;
        }

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
                Debug.LogWarning($"[TriInspector.MaterialProperty] Could not load cache: {e.Message}");
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
            Debug.Log("[TriInspector.MaterialProperty] Cache cleared.");
        }

        private static string GetMaterialShaderGuid(Material material)
        {
            if (material == null || material.shader == null)
                return null;

            var shaderPath = AssetDatabase.GetAssetPath(material.shader);
            if (string.IsNullOrEmpty(shaderPath))
                return null;

            return AssetDatabase.AssetPathToGUID(shaderPath);
        }

        public static void SaveSingleProperty(Material material, string name, ShaderPropertyType type, int id)
        {
            if (material == null || material.shader == null)
                return;

            string guid = GetMaterialShaderGuid(material);
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

            var shaderData = projectData.materials.FirstOrDefault(m => m.guid == guid);
            if (shaderData == null)
            {
                shaderData = new MaterialEntry { guid = guid };
                projectData.materials.Add(shaderData);
            }

            var validProps = EnumerateShaderProperties(material.shader).ToList();
            var validNames = validProps.Select(p => p.name).ToHashSet();
            var validIds = validProps.Select(p => p.id).ToHashSet();

            shaderData.properties.RemoveAll(p => !validNames.Contains(p.name) && !validIds.Contains(p.id));

            var entry = shaderData.properties.FirstOrDefault(p => p.id == id || p.name == name);
            if (entry == null)
            {
                shaderData.properties.Add(new PropertyEntry
                {
                    id = id,
                    name = name,
                    type = type
                });
            }
            else
            {
                entry.id = id;
                entry.name = name;
                entry.type = type;
            }

            string json = JsonUtility.ToJson(_globalCache, true);
            EditorPrefs.SetString(CacheEditorPrefsKey, json);
        }

        public static (PropertyData all, PropertyData filtered) GetProperties(Material material, ShaderPropertyType? filter)
        {
            if (material == null || material.shader == null)
                return (PropertyData.Empty, PropertyData.Empty);

            var props = EnumerateShaderProperties(material.shader).ToList();
            var all = BuildProperties(props);

            var filtered = !filter.HasValue
                  ? all
                  : BuildProperties(props.Where(p => p.type == filter.Value).ToList());

            return (all, filtered);
        }

        private static PropertyData BuildProperties(List<(string name, int id, ShaderPropertyType type)> props)
        {
            var displayNames = new List<GUIContent> { new("None") };
            var names = new List<string> { null };
            var hashes = new List<int> { 0 };
            var types = new List<ShaderPropertyType> { (ShaderPropertyType)0 };

            foreach (var p in props)
            {
                displayNames.Add(new GUIContent($"{p.name} ({p.type})", EditorGUIUtility.IconContent("TreeEditor.Material").image));
                names.Add(p.name);
                hashes.Add(p.id);
                types.Add(p.type);
            }

            return new PropertyData
            {
                DisplayNames = displayNames.ToArray(),
                Names = names.ToArray(),
                Hashes = hashes.ToArray(),
                Types = types.ToArray()
            };
        }

        private static IEnumerable<(string name, int id, ShaderPropertyType type)> EnumerateShaderProperties(Shader shader)
        {
            int count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                string name = shader.GetPropertyName(i);
                int id = Shader.PropertyToID(name);
                var type = shader.GetPropertyType(i);
                yield return (name, id, type);
            }
        }

        public static string ResolvePropertyName(Material material, object value)
        {
            if (material == null || material.shader == null)
                return value?.ToString();

            if (value is int id)
            {
                int count = material.shader.GetPropertyCount();
                for (int i = 0; i < count; i++)
                {
                    string propName = material.shader.GetPropertyName(i);
                    if (Shader.PropertyToID(propName) == id)
                        return propName;
                }
                return id.ToString();
            }

            return value?.ToString();
        }
        public static string GetInvalidPropertyLabel(Material material, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return "None";

            string guid = GetMaterialShaderGuid(material);
            if (string.IsNullOrEmpty(guid))
                return $"{propertyName} (Missing)";

            int projectHash = Application.dataPath.GetHashCode();
            LoadCache();

            var project = _globalCache?.projects?.FirstOrDefault(p => p.hash == projectHash);
            var shaderData = project?.materials?.FirstOrDefault(m => m.guid == guid);

            var match = shaderData?.properties?.FirstOrDefault(p => p.name == propertyName);

            return match != null
                ? $"{match.name} ({match.type})"
                : $"{propertyName} (Missing)";
        }

        public static string GetInvalidPropertyLabel(Material material, int id)
        {
            if (id == 0)
                return "None";

            string guid = GetMaterialShaderGuid(material);
            if (string.IsNullOrEmpty(guid))
                return $"Unknown ID ({id})";

            int projectHash = Application.dataPath.GetHashCode();
            LoadCache();

            var project = _globalCache?.projects?.FirstOrDefault(p => p.hash == projectHash);
            var shaderData = project?.materials?.FirstOrDefault(m => m.guid == guid);

            var match = shaderData?.properties?.FirstOrDefault(p => p.id == id);

            return match != null
                ? $"{match.name} ({match.type})"
                : $"Unknown ID ({id})";
        }
    }

    #endregion
}
