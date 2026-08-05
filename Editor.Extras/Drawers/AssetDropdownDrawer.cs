using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Utilities;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[assembly: RegisterTriAttributeDrawer(typeof(AssetDropdownDrawer<>), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class AssetDropdownDrawer<T> : TriAttributeDrawer<AssetDropdownAttribute>
    {
        private bool _showNoneElement;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (!typeof(Object).IsAssignableFrom(type))
            {
                return "AssetDropdown attribute can only be used on field with UnityEngine.Object type";
            }

            _showNoneElement = !propertyDefinition.Attributes.TryGet<RequiredAttribute>(out _);

            return base.Initialize(propertyDefinition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var dropdownElement = new TriDropdownVisualElement(property, EnumerateAssets, Attribute.Advanced);

            if (Attribute.HideNextDrawer)
            {
                return dropdownElement;
            }

            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            dropdownElement.style.flexGrow = 1;
            line.Add(dropdownElement);
            line.Add(next);
            return line;
        }

        private IEnumerable<ITriDropdownItem> EnumerateAssets(TriProperty property)
        {
            var assets = AssetDatabase.FindAssets(Attribute.Filter, Attribute.SearchInFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Object>)
                .Where(asset => asset is T)
                .Select(asset => (ITriDropdownItem) new TriDropdownItem<T>
                {
                    Text = Attribute.GetDisplayName(asset),
                    Value = (T) (object) asset,
                });

            if (_showNoneElement)
            {
                assets = assets.Prepend(new TriDropdownItem<T> {Text = "None", Value = default,});
            }

            return assets;
        }
    }
}