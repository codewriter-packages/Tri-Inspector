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

            return new TriValidatorsElement(property, next);
        }
    }
}
