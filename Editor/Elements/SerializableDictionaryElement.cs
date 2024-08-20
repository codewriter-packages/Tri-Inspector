using TriInspector.Types;
using UnityEngine;

namespace TriInspector.Elements
{
    public class SerializableDictionaryElement : TriElement
    {
        private readonly TriValueDrawer<SerializableDictionary<object, object>> _drawer;
        private readonly TriValue<SerializableDictionary<object, object>> _propertyValue;
        private readonly TriProperty _keyValuePairsPropertyValue;
        private readonly TriElement _nextTriElement;
        
        public SerializableDictionaryElement(TriValueDrawer<SerializableDictionary<object, object>> drawer,
            TriValue<SerializableDictionary<object, object>> propertyValue, TriElement nextTriElement)
        {
            _drawer = drawer;
            _propertyValue = propertyValue;
            _keyValuePairsPropertyValue = propertyValue.Property.ChildrenProperties[0];
            _nextTriElement = nextTriElement;
            
            AddChild(nextTriElement);
        }

        protected bool IsValid()
        {
            for (var i = 0; i < _keyValuePairsPropertyValue.ArrayElementProperties.Count; i++)
            {
                var triProperty1 = _keyValuePairsPropertyValue.ArrayElementProperties[i];
                
                for (var j = i + 1; j < _keyValuePairsPropertyValue.ArrayElementProperties.Count; j++)
                {
                    var triProperty2 = _keyValuePairsPropertyValue.ArrayElementProperties[j];

                    if (triProperty1.Value != null && triProperty2.Value != null &&
                        triProperty1.Value.Equals(triProperty2.Value))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        public override float GetHeight(float width)
        {
            return _drawer.GetHeight(width, _propertyValue, _nextTriElement);
        }

        public override void OnGUI(Rect position)
        {
            if (!IsValid())
            {
                var oldColor = GUI.color;
                var newColor = Color.red;
         
                GUI.color = newColor;
                GUI.contentColor = newColor;

                _drawer.OnGUI(position, _propertyValue, _nextTriElement);

                GUI.color = oldColor;
                GUI.contentColor = oldColor;
            }
            else
            {
                _drawer.OnGUI(position, _propertyValue, _nextTriElement);
            }
        }
    }
}