#if TRI_UNITY_PIPELINE

using Unity.Pipeline.Commands;

namespace TriInspector.Validator
{
    internal static class TriValidatorPipelineCommands
    {
        [CliCommand("tri", "validate_project",
            "Run Tri Inspector validation across every in-scope asset and scene in the project and " +
            "return a JSON report of all errors and warnings.")]
        public static string ValidateProject()
        {
            return TriValidator.ValidateProjectAsJson(prettyPrint: false);
        }

        [CliCommand("tri", "validate_scenes",
            "Run Tri Inspector validation across every currently open scene and return a JSON report " +
            "of all errors and warnings.")]
        public static string ValidateScenes()
        {
            return TriValidator.ValidateOpenedScenesAsJson(prettyPrint: false);
        }

        [CliCommand("tri", "validate_asset",
            "Run Tri Inspector validation on a single asset identified by its GlobalObjectId (the " +
            "globalId field in the report) and return a JSON report.")]
        public static string ValidateAsset(
            [CliArg("global_id",
                "GlobalObjectId of the asset to validate, e.g. GlobalObjectId_V1-1-<guid>-<fileId>-0.",
                Required = true)]
            string globalId)
        {
            return TriValidator.ValidateAssetAsJson(globalId, prettyPrint: false);
        }
    }
}

#endif
