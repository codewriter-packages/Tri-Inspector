using System;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(MinMaxSliderAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class MinMaxSliderAttributeDrawer : TriAttributeDrawer<MinMaxSliderAttribute>
    {
        private ValueResolver<float> _minFloatResolver;
        private ValueResolver<int> _minIntResolver;
        private ValueResolver<float> _maxFloatResolver;
        private ValueResolver<int> _maxIntResolver;
        private ValueResolver<Vector2> _minMaxVector2Resolver;
        private ValueResolver<Vector2Int> _minMaxVector2IntResolver;

        private bool _isVector2Int;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var fieldType = propertyDefinition.FieldType;
            if (fieldType != typeof(Vector2) && fieldType != typeof(Vector2Int))
            {
                return "[MinMaxRange] attribute can only be used on Vector2 or Vector2Int fields.";
            }
            _isVector2Int = fieldType == typeof(Vector2Int);

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
                if (_minFloatResolver.TryGetErrorString(out _))
                {
                    _minFloatResolver = null;
                    _minIntResolver = ValueResolver.Resolve<int>(propertyDefinition, Attribute.MinMemberName);
                    if (_minIntResolver.TryGetErrorString(out var error)) return error;
                }
            }

            if (!string.IsNullOrEmpty(Attribute.MaxMemberName))
            {
                _maxFloatResolver = ValueResolver.Resolve<float>(propertyDefinition, Attribute.MaxMemberName);
                if (_maxFloatResolver.TryGetErrorString(out _))
                {
                    _maxFloatResolver = null;
                    _maxIntResolver = ValueResolver.Resolve<int>(propertyDefinition, Attribute.MaxMemberName);
                    if (_maxIntResolver.TryGetErrorString(out var error)) return error;
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            float minLimit = _minMaxVector2Resolver?.GetValue(property, Vector2.zero).x ??
                             _minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).x ??
                             _minFloatResolver?.GetValue(property, Attribute.MinFixed) ??
                             _minIntResolver?.GetValue(property, (int) Attribute.MinFixed) ??
                             Attribute.MinFixed;

            float maxLimit = _minMaxVector2Resolver?.GetValue(property, Vector2.zero).y ??
                             _minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).y ??
                             _maxFloatResolver?.GetValue(property, Attribute.MaxFixed) ??
                             _maxIntResolver?.GetValue(property, (int) Attribute.MaxFixed) ??
                             Attribute.MaxFixed;

            if (minLimit > maxLimit) (minLimit, maxLimit) = (maxLimit, minLimit);

            float xValue, yValue;
            if (_isVector2Int)
            {
                var val = (Vector2Int) property.Value;
                xValue = val.x;
                yValue = val.y;
                minLimit = Mathf.RoundToInt(minLimit);
                maxLimit = Mathf.RoundToInt(maxLimit);
            }
            else
            {
                var val = (Vector2) property.Value;
                xValue = val.x;
                yValue = val.y;
            }

            float clampedX = xValue;
            float clampedY = yValue;

            if (clampedX > clampedY) (clampedX, clampedY) = (clampedY, clampedX);
            clampedX = Mathf.Clamp(clampedX, minLimit, clampedY);
            clampedY = Mathf.Clamp(clampedY, clampedX, maxLimit);

            // If clamping changed the value, update the property immediately.
            if (Math.Abs(clampedX - xValue) > 0.001f || Math.Abs(clampedY - yValue) > 0.001f)
            {
                xValue = clampedX;
                yValue = clampedY;

                if (_isVector2Int)
                    property.SetValue(new Vector2Int(Mathf.RoundToInt(xValue), Mathf.RoundToInt(yValue)));
                else
                    property.SetValue(new Vector2(xValue, yValue));
            }

            var label = property.DisplayNameContent;
            //EditorGUI.BeginChangeCheck();
            var controlRect = EditorGUI.PrefixLabel(position, label);

            TriEditorGUI.DrawMinMaxSlider(controlRect, ref xValue, ref yValue, minLimit, maxLimit);

            if (EditorGUI.EndChangeCheck())
            {
                // The slider itself ensures values are within the min/max limits.
                // We just need to apply the final result.
                if (_isVector2Int)
                {
                    var newVec = new Vector2Int(Mathf.RoundToInt(xValue), Mathf.RoundToInt(yValue));
                    property.SetValue(newVec);
                }
                else
                {
                    var newVec = new Vector2(xValue, yValue);
                    property.SetValue(newVec);
                }
            }
        }

        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}