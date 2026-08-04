using UnityEngine;

namespace TriInspector
{
    public abstract class TriPropertyOverrideContext
    {
        public abstract bool TryGetDisplayName(TriProperty property, out GUIContent displayName);
    }
}
