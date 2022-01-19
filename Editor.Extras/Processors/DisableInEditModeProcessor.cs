using TriInspector.Processors;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyDisableProcessor(typeof(DisableInEditModeProcessor))]
[assembly: RegisterTriPropertyDisableProcessor(typeof(EnableInEditModeProcessor))]

namespace TriInspector.Processors
{
    public class DisableInEditModeProcessor : TriPropertyDisableProcessor<DisableInEditModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return !Application.isPlaying;
        }
    }

    public class EnableInEditModeProcessor : TriPropertyDisableProcessor<EnableInEditModeAttribute>
    {
        public override bool IsDisabled(TriProperty property)
        {
            return Application.isPlaying;
        }
    }
}