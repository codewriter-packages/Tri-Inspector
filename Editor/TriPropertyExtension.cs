using JetBrains.Annotations;

namespace TriInspector
{
    public abstract class TriPropertyExtension
    {
        public bool? ApplyOnArrayElement { get; internal set; }

        [PublicAPI]
        public virtual string Initialize(TriPropertyDefinition propertyDefinition)
        {
            return null;
        }
    }
}