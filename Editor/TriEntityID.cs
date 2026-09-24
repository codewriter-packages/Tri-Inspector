using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector
{
    [PublicAPI]
    public struct TriEntityID : IEquatable<TriEntityID>
    {
#if UNITY_6000_4_OR_NEWER
        public EntityId entityId;

        public static explicit operator TriEntityID(EntityId e)
        {
            return new TriEntityID
            {
                entityId = e,
            };
        }

        [PublicAPI]
        public Object Obj => EditorUtility.EntityIdToObject(entityId);

        [PublicAPI]
        public static TriEntityID None => (TriEntityID) EntityId.None;
#else
        public int entityId;

        public static explicit operator TriEntityID(int e)
        {
            return new TriEntityID
            {
                entityId = e,
            };
        }

        [PublicAPI]
        public Object Obj => EditorUtility.InstanceIDToObject(entityId);

        [PublicAPI]
        public static TriEntityID None => (TriEntityID) 0;
#endif

        [PublicAPI]
        public GameObject GameObject => Obj as GameObject;

        public bool Equals(TriEntityID other) => entityId.Equals(other.entityId);

        public override bool Equals(object obj) => obj is TriEntityID other && Equals(other);

        public override int GetHashCode() => entityId.GetHashCode();

        public static bool operator ==(TriEntityID a, TriEntityID b) => a.Equals(b);

        public static bool operator !=(TriEntityID a, TriEntityID b) => !a.Equals(b);
    }
}
