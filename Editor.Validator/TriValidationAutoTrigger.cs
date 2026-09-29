using UnityEngine;

namespace TriInspector.Validator
{
    internal enum TriValidationAutoTrigger
    {
        [InspectorName("Revalidate modified objects on live changes")]
        LiveChanges,

        [InspectorName("Revalidate selected objects on selection change")]
        Selection,

        [InspectorName("Revalidate prefab on prefab stage opened")]
        PrefabStageOpen,

        [InspectorName("Revalidate prefab on prefab stage saved")]
        PrefabStageSave,

        [InspectorName("Revalidate scene on scene opened")]
        SceneOpen,

        [InspectorName("Revalidate scene on scene saved")]
        SceneSave,

        [InspectorName("Revalidate known issues on domain reload")]
        KnownIssuesOnDomainReload,
    }
}