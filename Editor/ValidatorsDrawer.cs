using TriInspector.VisualElements;
using UnityEngine.UIElements;

namespace TriInspector
{
    internal class ValidatorsDrawer : TriCustomDrawer
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            if (!property.HasValidators)
            {
                return next;
            }

            return new TriValidationResultsVisualElement(property, next);
        }
    }
}