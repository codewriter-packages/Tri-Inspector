using Editor.Extras.Processors;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyHideProcessor(typeof(HideInPlayModeProcessor))]
[assembly: RegisterTriPropertyHideProcessor(typeof(ShowInPlayModeProcessor))]

namespace Editor.Extras.Processors
{
    public class HideInPlayModeProcessor : TriPropertyHideProcessor<HideInPlayModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return Application.isPlaying;
        }
    }

    public class ShowInPlayModeProcessor : TriPropertyHideProcessor<ShowInPlayModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return !Application.isPlaying;
        }
    }
}