using System;

namespace TriInspector
{
    [Flags]
    public enum TriTargetPropertyType
    {
        Self = 1 << 0,
        ArrayElements = 1 << 2,
        SelfAndArrayElements = Self | ArrayElements,
    }
}