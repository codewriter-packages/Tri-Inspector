using UnityEngine;

namespace TriInspector.Processors
{
    [RegisterTriPropertyHideProcessor]
    public class HideInEditModeProcessor : TriPropertyHideProcessor<HideInEditModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return Application.isPlaying == Attribute.Inverse;
        }
    }
}