using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TriInspector.Validator
{
    [HideMonoScript]
    [FilePath("ProjectSettings/TriValidatorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class TriValidatorSettings : ScriptableSingleton<TriValidatorSettings>
    {
        [PropertySpace]
        [InfoBox("The validator scans assets inside the included folders", TriMessageType.None)]
        [ValidateInput(nameof(ValidateIncludedFolders))]
        [ListDrawerSettings(AlwaysExpanded = true)]
        [SerializeField]
        private string[] includedFolders =
        {
            "Assets",
        };

        [PropertySpace]
        [InfoBox("Assets inside an excluded folder are always skipped, even when nested inside an included folder",
            TriMessageType.None)]
        [ValidateInput(nameof(ValidateExcludedFolders))]
        [ListDrawerSettings(AlwaysExpanded = true)]
        [SerializeField]
        private string[] excludedFolders = Array.Empty<string>();

        [PropertySpace]
        [InfoBox("Assets whose main type matches one of these type names are skipped without being loaded",
            TriMessageType.None)]
        [ListDrawerSettings(AlwaysExpanded = true)]
        [OnValueChanged(nameof(ResetTypeCache))]
        [SerializeField]
        private string[] excludedTypeNames =
        {
            "Texture",
            "Material",
            "Shader",
            "ComputeShader",
            "Mesh",
            "AudioClip",
            "VideoClip",
            "AnimationClip",
            "Font",
            "Sprite",
            "MonoScript",
            "DefaultAsset",
        };

        [PropertySpace]
        [InfoBox("Logs a per-type timing report to the console after each full-project scan",
            TriMessageType.None)]
        [SerializeField]
        private bool enableProfiler;

        [NonSerialized] private Dictionary<Type, bool> _excludedTypeCache;

        public bool EnableProfiler => enableProfiler;

        public void Save()
        {
            Save(true);
        }

        public void ResetTypeCache()
        {
            _excludedTypeCache?.Clear();
        }

        public bool IsExcludedType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            _excludedTypeCache ??= new Dictionary<Type, bool>();
            if (_excludedTypeCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var excluded = ComputeIsExcludedType(type);
            _excludedTypeCache.Add(type, excluded);
            return excluded;
        }

        private bool ComputeIsExcludedType(Type type)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (var typeName in excludedTypeNames)
                {
                    if (string.Equals(current.Name, typeName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Unsaved scenes have no asset path, so they stay in scope.
        public bool IsInScope(Scene scene)
        {
            return string.IsNullOrEmpty(scene.path) || IsInScope(scene.path);
        }

        public bool IsInScope(string assetPath)
        {
            return !IsExcluded(assetPath) && IsIncluded(assetPath);
        }

        private bool IsExcluded(string assetPath)
        {
            foreach (var folder in excludedFolders)
            {
                if (IsUnder(assetPath, folder))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIncluded(string assetPath)
        {
            foreach (var folder in includedFolders)
            {
                if (IsUnder(assetPath, folder))
                {
                    return true;
                }
            }

            return false;
        }

        private TriValidationResult ValidateIncludedFolders() => ValidateFolders(includedFolders);
        private TriValidationResult ValidateExcludedFolders() => ValidateFolders(excludedFolders);

        private static TriValidationResult ValidateFolders(string[] folders)
        {
            if (folders == null)
            {
                return TriValidationResult.Valid;
            }

            foreach (var folder in folders)
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    return TriValidationResult.Warning("Folder paths cannot be empty");
                }
            }

            foreach (var folder in folders)
            {
                if (folder != Normalize(folder))
                {
                    return TriValidationResult.Warning("Folder paths must be normalized: forward slashes, " +
                                                       "no trailing slash and no surrounding whitespace");
                }
            }

            return TriValidationResult.Valid;
        }

        private static bool IsUnder(string path, string folder)
        {
            if (path.Length < folder.Length)
            {
                return false;
            }

            if (!path.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return path.Length == folder.Length || path[folder.Length] == '/';
        }

        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Trim().Replace('\\', '/').TrimEnd('/');
        }
    }
}