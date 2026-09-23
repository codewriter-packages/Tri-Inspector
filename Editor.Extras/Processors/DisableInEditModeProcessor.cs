using UnityEngine;

namespace TriInspector.Processors
{
    [RegisterTriPropertyDisableProcessor]
    public class DisableInEditModeProcessor : TriPropertyDisableProcessor<DisableInEditModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return Application.isPlaying == Attribute.Inverse;
        }
    }
}