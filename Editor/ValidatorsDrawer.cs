using TriInspector.Elements;

namespace TriInspector
{
    internal class ValidatorsDrawer : TriCustomDrawer
    {
        public override TriElement CreateElementInternal(TriProperty property, TriElement next)
        {
            if (!property.HasValidators)
            {
                return next;
            }

            var element = new TriElement();
            element.AddChild(new TriPropertyValidationResultElement(property));
            element.AddChild(next);
            return element;
        }
    }
}