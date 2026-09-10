using System;
using System.Collections.Generic;

namespace TriInspector
{
    public readonly struct TriArray<T> where T : class
    {
        private readonly List<T> _list;
        private readonly T[] _array;

        public TriArray(List<T> list)
        {
            _list = list;
            _array = null;
        }

        public TriArray(T[] array)
        {
            _array = array;
            _list = null;
        }

        public Enumerator GetEnumerator() => new Enumerator(_list, _array);

        public int Count => _list?.Count ?? _array?.Length ?? 0;

        public T this[int index] => _list != null ? _list[index] : _array[index];

        public bool TryGet<TTyped>(out TTyped result) where TTyped : class, T
        {
            if (_list != null)
            {
                foreach (var attribute in _list)
                {
                    if (attribute is TTyped typed)
                    {
                        result = typed;
                        return true;
                    }
                }

                result = null;
                return false;
            }

            if (_array != null)
            {
                foreach (var attribute in _array)
                {
                    if (attribute is TTyped typed)
                    {
                        result = typed;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public static implicit operator TriArray<T>(T[] array) => new(array);
        public static implicit operator TriArray<T>(List<T> list) => new(list);

        public struct Enumerator
        {
            private readonly List<T> _list;
            private readonly T[] _array;
            private int _index;

            internal Enumerator(List<T> list, T[] arr)
            {
                _list = list;
                _array = list == null ? arr ?? Array.Empty<T>() : null;
                _index = 0;
                Current = null;
            }

            public T Current { get; private set; }

            public bool MoveNext()
            {
                if (_list != null)
                {
                    if (_index < _list.Count)
                    {
                        Current = _list[_index++];
                        return true;
                    }

                    return false;
                }

                if (_index < _array.Length)
                {
                    Current = _array[_index++];
                    return true;
                }

                return false;
            }
        }
    }
}