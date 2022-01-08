using System;

namespace TriInspector
{
    [Flags]
    public enum TriTargetPropertyType
    {
        Self = 1 << 0,
        Array = 1 << 2,
        SelfAndArray = Self | Array,
    }
}