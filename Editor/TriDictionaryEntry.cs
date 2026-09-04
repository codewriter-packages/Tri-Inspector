using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    [Serializable]
    public struct TriDictionaryEntry<TKey, TValue> : IEquatable<TriDictionaryEntry<TKey, TValue>>, ITriDictionaryEntry
    {
        [SerializeField] public TKey key;
        [SerializeField] public TValue value;

        public bool Equals(TriDictionaryEntry<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(key, other.key) &&
                   EqualityComparer<TValue>.Default.Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((TriDictionaryEntry<TKey, TValue>) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(key, value);
        }

        public static List<TriDictionaryEntry<TKey, TValue>> MakeList(Dictionary<TKey, TValue> dict)
        {
            var list = new List<TriDictionaryEntry<TKey, TValue>>();

            if (dict != null)
            {
                foreach (var (key, val) in dict)
                {
                    list.Add(new TriDictionaryEntry<TKey, TValue> {key = key, value = val});
                }
            }

            return list;
        }

        public static List<TriDictionaryEntry<TKey, TValue>> MakeListFromSerializedProperty(
            SerializedProperty serializedProperty)
        {
            var list = new List<TriDictionaryEntry<TKey, TValue>>();

            for (var i = 0; i < serializedProperty.arraySize; i++)
            {
                var element = serializedProperty.GetArrayElementAtIndex(i);

                list.Add(new TriDictionaryEntry<TKey, TValue>
                {
                    key = (TKey) element.FindPropertyRelative("key").boxedValue,
                    value = (TValue) element.FindPropertyRelative("value").boxedValue,
                });
            }

            return list;
        }

        public static void WriteToSerializedProperty(List<TriDictionaryEntry<TKey, TValue>> list,
            SerializedProperty serializedProperty)
        {
            var count = list?.Count ?? 0;

            serializedProperty.arraySize = count;

            if (list != null)
            {
                for (var i = 0; i < count; i++)
                {
                    var entry = list[i];
                    var element = serializedProperty.GetArrayElementAtIndex(i);

                    element.FindPropertyRelative("key").boxedValue = entry.key;
                    element.FindPropertyRelative("value").boxedValue = entry.value;
                }
            }

            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public static Dictionary<TKey, TValue> MakeDict(List<TriDictionaryEntry<TKey, TValue>> list,
            List<int> duplicateEntryIndices, List<int> nullKeyEntryIndices)
        {
            duplicateEntryIndices.Clear();
            nullKeyEntryIndices.Clear();

            var dict = new Dictionary<TKey, TValue>();

            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var entry = list[i];

                    if (IsNullKey(entry.key))
                    {
                        nullKeyEntryIndices.Add(i);
                        continue;
                    }

                    if (!dict.TryAdd(entry.key, entry.value))
                    {
                        duplicateEntryIndices.Add(i);
                    }
                }
            }

            return dict;
        }

        private static bool IsNullKey(TKey key)
        {
            return key is null || key is UnityEngine.Object unityKey && unityKey == null;
        }
    }

    public interface ITriDictionaryEntry
    {
    }
}