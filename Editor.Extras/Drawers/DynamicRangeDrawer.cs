using System;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DynamicRangeAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class DynamicRangeAttributeDrawer : TriAttributeDrawer<DynamicRangeAttribute>
    {
        private ValueResolver<float> _minFloatResolver;
        private ValueResolver<int> _minIntResolver;
        private ValueResolver<float> _maxFloatResolver;
        private ValueResolver<int> _maxIntResolver;
        private ValueResolver<Vector2> _minMaxVector2Resolver;
        private ValueResolver<Vector2Int> _minMaxVector2IntResolver;

        private Type _valueType;

        private bool IsNumericType(Type type)
        {
            if (type == null) return false;
            return typeof(IConvertible).IsAssignableFrom(type) &&
                   type != typeof(string) &&
                   type != typeof(bool) &&
                   type != typeof(char);
        }

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _valueType = propertyDefinition.FieldType;
            if (!IsNumericType(_valueType))
            {
                return "[Range] attribute can only be used on numeric fields (like int, float, double, etc.).";
            }

            if (!string.IsNullOrEmpty(Attribute.MinMaxMemberName))
            {
                _minMaxVector2Resolver = ValueResolver.Resolve<Vector2>(propertyDefinition, Attribute.MinMaxMemberName);
                if (_minMaxVector2Resolver.TryGetErrorString(out var vector2Error))
                {
                    _minMaxVector2Resolver = null;
                    _minMaxVector2IntResolver = ValueResolver.Resolve<Vector2Int>(propertyDefinition, Attribute.MinMaxMemberName);
                    if (_minMaxVector2IntResolver.TryGetErrorString(out var vector2IntError))
                    {
                        return vector2IntError;
                    }
                }
                return TriExtensionInitializationResult.Ok;
            }

            if (!string.IsNullOrEmpty(Attribute.MinMemberName))
            {
                _minFloatResolver = ValueResolver.Resolve<float>(propertyDefinition, Attribute.MinMemberName);
                if (_minFloatResolver.TryGetErrorString(out var floatError))
                {
                    _minFloatResolver = null;
                    _minIntResolver = ValueResolver.Resolve<int>(propertyDefinition, Attribute.MinMemberName);
                    if (_minIntResolver.TryGetErrorString(out var intError))
                    {
                        return intError;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Attribute.MaxMemberName))
            {
                _maxFloatResolver = ValueResolver.Resolve<float>(propertyDefinition, Attribute.MaxMemberName);
                if (_maxFloatResolver.TryGetErrorString(out var floatError))
                {
                    _maxFloatResolver = null;
                    _maxIntResolver = ValueResolver.Resolve<int>(propertyDefinition, Attribute.MaxMemberName);
                    if (_maxIntResolver.TryGetErrorString(out var intError))
                    {
                        return intError;
                    }
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            double minLimit = _minMaxVector2Resolver?.GetValue(property, Vector2.zero).x ?? 
                              _minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).x ??
                              _minFloatResolver?.GetValue(property, Attribute.MinFixed) ??
                              _minIntResolver?.GetValue(property, (int) Attribute.MinFixed) ??
                              Attribute.MinFixed;

            double maxLimit = _minMaxVector2Resolver?.GetValue(property, Vector2.zero).y ??
                              _minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).y ?? 
                              _maxFloatResolver?.GetValue(property, Attribute.MaxFixed) ??
                              _maxIntResolver?.GetValue(property, (int) Attribute.MaxFixed) ??
                              Attribute.MaxFixed;

            if (minLimit > maxLimit) (minLimit, maxLimit) = (maxLimit, minLimit);

            var label = property.DisplayNameContent;

            double currentValue;
            try
            {
                currentValue = Convert.ToDouble(property.Value);
            }
            catch (Exception)
            {
                EditorGUI.LabelField(position, label.text, "Cannot convert value to a number.");
                return;
            }

            if (property.FieldType == typeof(int))
            {
                minLimit = Mathf.RoundToInt((float)minLimit);
                maxLimit = Mathf.RoundToInt((float)maxLimit);
            }

            // If clamping changed the value, update the property immediately.
            double clampedValue = Math.Clamp(currentValue, minLimit, maxLimit);
            if (Math.Abs(clampedValue - currentValue) > double.Epsilon)
            {
                property.SetValue(Convert.ChangeType(clampedValue, _valueType));
                currentValue = clampedValue;
            }

            EditorGUI.BeginChangeCheck();
            float sliderValue = EditorGUI.Slider(position, label, (float) currentValue, (float) minLimit, (float) maxLimit);

            if (EditorGUI.EndChangeCheck())
            {
                object finalValue = Convert.ChangeType(sliderValue, _valueType);
                property.SetValue(finalValue);
            }
        }

        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}