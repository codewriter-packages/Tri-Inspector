using UnityEngine;

namespace TriInspector.Processors
{
    [RegisterTriPropertyDisableProcessor]
    public class DisableInPlayModeProcessor : TriPropertyDisableProcessor<DisableInPlayModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return Application.isPlaying != Attribute.Inverse;
        }
    }
}