using System.Collections.Generic;

namespace TriInspector.Elements
{
    public class TriPropertyValidationResultElement : TriElement
    {
        private readonly TriProperty _property;
        private IReadOnlyList<TriValidationResult> _validationResults;

        public TriPropertyValidationResultElement(TriProperty property)
        {
            _property = property;
        }

        public override bool Update()
        {
            var dirty = base.Update();

            dirty |= GenerateValidationResults();

            return dirty;
        }

        private bool GenerateValidationResults()
        {
            if (_property.ValidationResults == _validationResults)
            {
                return false;
            }

            _validationResults = _property.ValidationResults;

            RemoveAllChildren();

            foreach (var result in _validationResults)
            {
                AddChild(new TriInfoBoxElement(result.Message, result.MessageType));
            }

            return true;
        }
    }
}