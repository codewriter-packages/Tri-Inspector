using Editor.Extras.Processors;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyDisableProcessor(typeof(DisableInPlayModeProcessor))]
[assembly: RegisterTriPropertyDisableProcessor(typeof(EnableInPlayModeProcessor))]

namespace Editor.Extras.Processors
{
    public class DisableInPlayModeProcessor : TriPropertyDisableProcessor<DisableInPlayModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return Application.isPlaying;
        }
    }

    public class EnableInPlayModeProcessor : TriPropertyDisableProcessor<EnableInPlayModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return !Application.isPlaying;
        }
    }
}