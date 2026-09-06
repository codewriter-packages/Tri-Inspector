using UnityEngine;

namespace TriInspector.Processors
{
    [RegisterTriPropertyHideProcessor]
    public class HideInPlayModeProcessor : TriPropertyHideProcessor<HideInPlayModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return Application.isPlaying != Attribute.Inverse;
        }
    }
}