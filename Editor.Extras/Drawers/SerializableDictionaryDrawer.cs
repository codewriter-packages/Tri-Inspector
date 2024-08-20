using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.Types;

[assembly: RegisterTriValueDrawer(typeof(SerializableDictionaryDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class SerializableDictionaryDrawer : TriValueDrawer<SerializableDictionary<object, object>>
    {
        public override TriElement CreateElement(TriValue<SerializableDictionary<object, object>> propertyValue, TriElement next)
        {
            return new SerializableDictionaryElement(this, propertyValue, next);
        }
    }
}