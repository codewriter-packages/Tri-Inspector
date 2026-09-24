using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    [PublicAPI]
    public static class TriEntityIDExtensions
    {
        [PublicAPI]
        public static TriEntityID GetTriEntityID(this Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return (TriEntityID) obj.GetEntityId();
#else
            return (TriEntityID) obj.GetInstanceID();
#endif
        }

        [PublicAPI]
        public static TriEntityID GetObjectReferenceTriEntityID(this SerializedProperty property)
        {
#if UNITY_6000_4_OR_NEWER
            return (TriEntityID) property.objectReferenceEntityIdValue;
#else
            return (TriEntityID) property.objectReferenceInstanceIDValue;
#endif
        }

        [PublicAPI]
        public static bool IsAssetPreviewLoading(this Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return AssetPreview.IsLoadingAssetPreview(obj.GetEntityId());
#else
            return AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID());
#endif
        }

#if UNITY_6000_4_OR_NEWER
        public static TriEntityID EntityID(this ChangeGameObjectOrComponentPropertiesEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID EntityID(this ChangeGameObjectStructureEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID EntityID(this ChangeGameObjectStructureHierarchyEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID EntityID(this CreateGameObjectHierarchyEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID EntityID(this ChangeGameObjectParentEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID PreviousParentEntityID(this ChangeGameObjectParentEventArgs e)
            => (TriEntityID) e.previousParentEntityId;

        public static TriEntityID ParentEntityID(this DestroyGameObjectHierarchyEventArgs e)
            => (TriEntityID) e.parentEntityId;

        public static TriEntityID EntityID(this ChangeChildrenOrderEventArgs e)
            => (TriEntityID) e.entityId;

        public static TriEntityID EntityID(this ChangeAssetObjectPropertiesEventArgs e)
            => (TriEntityID) e.entityId;
#else
        public static TriEntityID EntityID(this ChangeGameObjectOrComponentPropertiesEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID EntityID(this ChangeGameObjectStructureEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID EntityID(this ChangeGameObjectStructureHierarchyEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID EntityID(this CreateGameObjectHierarchyEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID EntityID(this ChangeGameObjectParentEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID PreviousParentEntityID(this ChangeGameObjectParentEventArgs e)
            => (TriEntityID) e.previousParentInstanceId;

        public static TriEntityID ParentEntityID(this DestroyGameObjectHierarchyEventArgs e)
            => (TriEntityID) e.parentInstanceId;

        public static TriEntityID EntityID(this ChangeChildrenOrderEventArgs e)
            => (TriEntityID) e.instanceId;

        public static TriEntityID EntityID(this ChangeAssetObjectPropertiesEventArgs e)
            => (TriEntityID) e.instanceId;
#endif
    }
}