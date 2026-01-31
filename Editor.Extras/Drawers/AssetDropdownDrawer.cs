using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(AssetDropdownDrawer<>), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class AssetDropdownDrawer<T> : TriAttributeDrawer<AssetDropdownAttribute>
    {
        private bool showNoneElement;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (!typeof(Object).IsAssignableFrom(type))
            {
                return "AssetDropdown attribute can only be used on field with UnityEngine.Object type";
            }

            showNoneElement = !propertyDefinition.Attributes.TryGet<RequiredAttribute>(out _);

            return base.Initialize(propertyDefinition);
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            var dropdownElement = new TriDropdownElement(property, EnumerateAssets, Attribute.Advanced);

            if (Attribute.HideNextDrawer)
            {
                return dropdownElement;
            }

            var line = new TriHorizontalGroupElement();
            line.AddChild(dropdownElement);
            line.AddChild(base.CreateElement(property, next));
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

            if (showNoneElement)
            {
                assets = assets.Prepend(new TriDropdownItem<T> {Text = "None", Value = default,});
            }

            return assets;
        }
    }
}