using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TriInspector.Types
{
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField, FormerlySerializedAs("classReference")]
        protected string _typeReference;
        
        protected Type _type;
        
        public Type Type
        {
            get => _type;
            set
            {
                _type = value;
                _typeReference = GetReferenceValue(value);
            }
        }
        
        public SerializableType()
        {
            
        }

        public SerializableType(string assemblyQualifiedTypeName)
        {
            Type = !string.IsNullOrEmpty(assemblyQualifiedTypeName) ? Type.GetType(assemblyQualifiedTypeName) : null;
        }

        public SerializableType(Type type)
        {
            Type = type;
        }
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_typeReference))
            {
                _type = Type.GetType(_typeReference);
                
                if (_type == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"'{_typeReference}' was referenced but class type was not found.");
#endif
                }
            }
            else
            {
                _type = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
        
        public override string ToString()
        {
            return Type != null ? Type.FullName : $"(None)";
        }
        
        public static string GetReferenceValue(Type type)
        {
            return type != null ? type.FullName + ", " + type.Assembly.GetName().Name : string.Empty;
        }

        public static Type GetReferenceType(string referenceValue)
        {
            return !string.IsNullOrEmpty(referenceValue) ? Type.GetType(referenceValue) : null;
        }
        
        public static implicit operator string(SerializableType typeReference) => typeReference._typeReference;

        public static implicit operator Type(SerializableType typeReference) => typeReference.Type;

        public static implicit operator SerializableType(Type type) => new SerializableType(type);
    }
}