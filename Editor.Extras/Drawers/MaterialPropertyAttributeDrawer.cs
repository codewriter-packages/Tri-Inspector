using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(MaterialPropertyAttributeDrawer), TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class MaterialPropertyAttributeDrawer : TriAttributeDrawer<MaterialPropertyAttribute>
    {
        private MaterialPropertyHelper.ResolvedParams _resolvedParams;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvedParams = MaterialPropertyHelper.Initialize(propertyDefinition, Attribute);

            if (_resolvedParams.ErrorResult.IsError)
            {
                return TriExtensionInitializationResult.Skip;
            }

            if (propertyDefinition.FieldType != typeof(string) &&
                propertyDefinition.FieldType != typeof(int))
            {
                return "[MaterialProperty] can only be used on 'string' or 'int' fields.";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            VisualElement dropdown = property.FieldType == typeof(int)
                ? new TriDropdownVisualElement<int>(property, GetIntItems, useAdvancedDropdown: true)
                : new TriDropdownVisualElement<string>(property, GetStringItems, useAdvancedDropdown: true);

            dropdown.TrackPropertyValueChanged(property, CacheSelectedProperty);
            return dropdown;
        }

        private IEnumerable<ITriDropdownItem> GetStringItems(TriProperty property)
        {
            var material = _resolvedParams.MaterialResolver.GetValue(property);
            var (all, filtered) = MaterialPropertyHelper.GetProperties(material, Attribute.PropertyType);
            return BuildItems(material, (string) property.Value,
                all.Names, filtered.Names, filtered.DisplayNames,
                MaterialPropertyHelper.GetInvalidPropertyLabel);
        }

        private IEnumerable<ITriDropdownItem> GetIntItems(TriProperty property)
        {
            var material = _resolvedParams.MaterialResolver.GetValue(property);
            var (all, filtered) = MaterialPropertyHelper.GetProperties(material, Attribute.PropertyType);
            return BuildItems(material, (int) property.Value!,
                all.Hashes, filtered.Hashes, filtered.DisplayNames,
                MaterialPropertyHelper.GetInvalidPropertyLabel);
        }

        private static IEnumerable<ITriDropdownItem> BuildItems<T>(
            Material material,
            T currentValue,
            T[] allValues,
            T[] filteredValues,
            GUIContent[] displayNames,
            Func<Material, T, string> invalidLabelFunc)
        {
            var items = new List<ITriDropdownItem>(filteredValues.Length + 1);
            for (var i = 0; i < filteredValues.Length; i++)
            {
                items.Add(new TriDropdownItem<T> {Text = displayNames[i].text, Value = filteredValues[i]});
            }

            // If the stored value isn't a live parameter of the right type (and isn't the empty/None value),
            // append it so the closed-state label and menu still show it via GetInvalidPropertyLabel.
            var isEmpty = EqualityComparer<T>.Default.Equals(currentValue, default);
            var isValid = isEmpty || (allValues.Contains(currentValue) && filteredValues.Contains(currentValue));
            if (!isValid)
            {
                items.Add(new TriDropdownItem<T>
                    {Text = invalidLabelFunc(material, currentValue), Value = currentValue});
            }

            return items;
        }

        private void CacheSelectedProperty(TriProperty property)
        {
            var material = _resolvedParams.MaterialResolver.GetValue(property);
            if (material == null || material.shader == null)
            {
                return;
            }

            var (all, _) = MaterialPropertyHelper.GetProperties(material, null);

            int index;
            if (property.ValueType == typeof(string))
            {
                var name = (string) property.Value;
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                index = Array.IndexOf(all.Names, name);
            }
            else if (property.ValueType == typeof(int))
            {
                var id = (int) property.Value!;
                if (id == 0)
                {
                    return;
                }

                index = Array.IndexOf(all.Hashes, id);
            }
            else
            {
                return;
            }

            // index 0 is the "None" entry; -1 means the value is not a live property.
            if (index <= 0)
            {
                return;
            }

            MaterialPropertyHelper.SaveSingleProperty(material, all.Names[index], all.Types[index], all.Hashes[index]);
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
