using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    public struct TriAssetValidationResult
    {
        public string assetPath;

        public string label;
        public Object asset;

        // Set for full-content assets (ScriptableObjects): the per-issue breakdown shown as child rows.
        public List<TriObjectValidationResult> objects;

        // Set for count-only leaves (scene assets, prefabs), which carry only rolled-up counts and no
        // expandable child rows. When set, names the leaf node kind to emit (SceneAsset or Prefab).
        public TriValidationNodeKind? countOnlyKind;
        public int errorCount;
        public int warningCount;
    }
}