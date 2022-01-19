using TriInspector.Processors;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyHideProcessor(typeof(HideInEditModeProcessor))]
[assembly: RegisterTriPropertyHideProcessor(typeof(ShowInEditModeProcessor))]

namespace TriInspector.Processors
{
    public class HideInEditModeProcessor : TriPropertyHideProcessor<HideInEditModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return !Application.isPlaying;
        }
    }

    public class ShowInEditModeProcessor : TriPropertyHideProcessor<ShowInEditModeAttribute>
    {
        public override bool IsHidden(TriProperty property)
        {
            return Application.isPlaying;
        }
    }
}