using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TriInspector.Types
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        protected struct KeyValuePair
        {
            [SerializeReference]
            private TKey _key;
            [SerializeReference]
            private TValue _value;

            public KeyValuePair(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }

            public TKey Key
            {
                get => _key;
                set => _key = value;
            }

            public TValue Value
            {
                get => _value;
                set => _value = value;
            }

            public override bool Equals(object obj)
            {
                if (obj is KeyValuePair keyValuePair && _key != null && keyValuePair._key != null)
                {
                    return keyValuePair._key.Equals(_key);
                }
                
                return base.Equals(obj);
            }

            public bool Equals(KeyValuePair other)
            {
                return EqualityComparer<TKey>.Default.Equals(_key, other._key) && EqualityComparer<TValue>.Default.Equals(_value, other._value);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_key, _value);
            }
        }
        
        [SerializeField, TableList(AlwaysExpanded = true), HideLabel]
        protected List<KeyValuePair> _keyValuePairs = new ();

        protected readonly Dictionary<TKey, TValue> _dictionary = new ();
        protected readonly Dictionary<TKey, int> _indexByKey = new ();

        [SerializeField, HideInInspector]
        protected bool _error;

        protected internal bool Error => _error;

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;
                
                if (_indexByKey.ContainsKey(key))
                {
                    var index = _indexByKey[key];
                    
                    _keyValuePairs[index] = new KeyValuePair(key, value);
                }
                else
                {
                    _keyValuePairs.Add(new KeyValuePair(key, value));
                    
                    _indexByKey.Add(key, _keyValuePairs.Count - 1);
                }
            }
        }

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> source)
        {
            foreach (var pair in source)
            {
                Add(pair.Key, pair.Value);
            }
        }
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _dictionary.Clear();
            _indexByKey.Clear();
            
            _error = false;

            for (int i = 0; i < _keyValuePairs.Count; i++)
            {
                var key = _keyValuePairs[i].Key;
                
                if (key != null && !ContainsKey(key))
                {
                    _dictionary.Add(key, _keyValuePairs[i].Value);
                    _indexByKey.Add(key, i);
                }
                else
                {
                    _error = true;
                }
            }
        }
        
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
        {
            if (_dictionary.TryGetValue(pair.Key, out var value))
            {
                return EqualityComparer<TValue>.Default.Equals(value, pair.Value);
            }
            
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
        {
            if (_dictionary.TryGetValue(pair.Key, out var value))
            {
                var isEqual = EqualityComparer<TValue>.Default.Equals(value, pair.Value);
                
                if (isEqual)
                {
                    return Remove(pair.Key);
                }
            }

            return false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            var collection = (ICollection)_dictionary;
            
            collection.CopyTo(array, index);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        
        protected void UpdateIndexes(int removedIndex)
        {
            for (var i = removedIndex; i < _keyValuePairs.Count; i++)
            {
                var key = _keyValuePairs[i].Key;
                
                _indexByKey[key]--;
            }
        }
        
        public void Add(TKey key, TValue value)
        {
            _keyValuePairs.Add(new KeyValuePair(key, value));
            _dictionary.Add(key, value);
            _indexByKey.Add(key, _keyValuePairs.Count - 1);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                var index = _indexByKey[key];
                
                _keyValuePairs.RemoveAt(index);
                
                UpdateIndexes(index);
                
                _indexByKey.Remove(key);
                
                return true;
            }
            
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            _keyValuePairs.Clear();
            _dictionary.Clear();
            _indexByKey.Clear();
        }
        
        public Dictionary<TKey, TValue> BuildNativeDictionary()
        {
            return new Dictionary<TKey, TValue>(_dictionary);
        }
        
        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDictionary)
        {
            return serializableDictionary._dictionary;
        }

        public static implicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            return new SerializableDictionary<TKey, TValue>(dictionary);
        }
    }
}