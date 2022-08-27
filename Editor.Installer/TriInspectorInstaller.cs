using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor.Installer
{
    internal static class TriInspectorInstaller
    {
        private const string BridgeSourcesPath =
            "Packages/com.codewriter.triinspector/Unity.InternalAPIEditorBridge.012";

        private const string BridgeAssemblyPath = "Assets/Plugins/TriInspector/Unity.InternalAPIEditorBridge.012";
        private const string BridgeAssemblyFileName = "Unity.InternalAPIEditorBridge.012.asmdef";
        private const string BridgeScriptFileName = "Unity.InternalAPIEditorBridge.012.cs";

        private const string BridgeAssemblyContent = @"{
    ""name"": ""Unity.InternalAPIEditorBridge.012"",
    ""includePlatforms"": [
    ""Editor""
    ]
}";

        private const string BridgeScriptContent = "";

        private static readonly bool TriInspectorDefined =
#if TRI_INSPECTOR
            true;
#else
            false;
#endif

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (TriInspectorDefined)
            {
                return;
            }

            if (!Directory.Exists(BridgeAssemblyPath))
            {
                Directory.CreateDirectory(BridgeAssemblyPath);

                var bridgeAsmFile = Path.Combine(BridgeAssemblyPath, BridgeAssemblyFileName);
                var bridgeScriptFile = Path.Combine(BridgeAssemblyPath, BridgeScriptFileName);

                File.WriteAllText(bridgeAsmFile, BridgeAssemblyContent);
                File.WriteAllText(bridgeScriptFile, BridgeScriptContent);

                AssetDatabase.ImportAsset(bridgeAsmFile, ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(bridgeScriptFile, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                var group = EditorUserBuildSettings.selectedBuildTargetGroup;
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var newDefines = string.Join(";", defines.Split(';').Append("TRI_INSPECTOR").ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                AssetDatabase.ImportAsset(BridgeSourcesPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}