using System;
using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace TriInspector.Drawers
{
    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class StringDrawer : BuiltinDrawerBase<string>
    {
        protected override BaseField<string> CreateField() => new TextField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class BooleanDrawer : BuiltinDrawerBase<bool>
    {
        protected override BaseField<bool> CreateField() => new Toggle();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class IntegerDrawer : BuiltinDrawerBase<int>
    {
        protected override BaseField<int> CreateField() => new IntegerField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class LongDrawer : BuiltinDrawerBase<long>
    {
        protected override BaseField<long> CreateField() => new LongField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class UnsignedIntegerDrawer : BuiltinDrawerBase<uint>
    {
        protected override BaseField<uint> CreateField() => new UnsignedIntegerField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class UnsignedLongDrawer : BuiltinDrawerBase<ulong>
    {
        protected override BaseField<ulong> CreateField() => new UnsignedLongField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class FloatDrawer : BuiltinDrawerBase<float>
    {
        protected override BaseField<float> CreateField() => new FloatField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class DoubleDrawer : BuiltinDrawerBase<double>
    {
        protected override BaseField<double> CreateField() => new DoubleField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class ColorDrawer : BuiltinDrawerBase<Color>
    {
        protected override BaseField<Color> CreateField() => new ColorField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Color32Drawer : BuiltinDrawerBase<Color32>
    {
        public override VisualElement CreateVisualElement(TriValue<Color32> propertyValue, VisualElement next)
        {
            var field = new ColorField();
            field.BindTri(propertyValue, v => v, v => v);
            return field;
        }
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class LayerMaskDrawer : BuiltinDrawerBase<LayerMask>
    {
        public override VisualElement CreateVisualElement(TriValue<LayerMask> propertyValue, VisualElement next)
        {
            var field = new LayerMaskField();
            field.BindTri(propertyValue, v => v.value, v => v);
            return field;
        }
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class EnumDrawer : BuiltinDrawerBase<Enum>
    {
        public override VisualElement CreateVisualElement(TriValue<Enum> propertyValue, VisualElement next)
        {
            var enumType = propertyValue.Property.FieldType;
            var current = propertyValue.SmartValue ?? (Enum) Enum.ToObject(enumType, 0);

            BaseField<Enum> field = enumType.IsDefined(typeof(FlagsAttribute), false)
                ? new EnumFlagsField(current)
                : new EnumField(current);

            field.BindTri(propertyValue);
            return field;
        }
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Vector2Drawer : BuiltinDrawerBase<Vector2>
    {
        protected override BaseField<Vector2> CreateField() => new Vector2Field();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Vector3Drawer : BuiltinDrawerBase<Vector3>
    {
        protected override BaseField<Vector3> CreateField() => new Vector3Field();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Vector4Drawer : BuiltinDrawerBase<Vector4>
    {
        protected override BaseField<Vector4> CreateField() => new Vector4Field();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class RectDrawer : BuiltinDrawerBase<Rect>
    {
        protected override BaseField<Rect> CreateField() => new RectField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class AnimationCurveDrawer : BuiltinDrawerBase<AnimationCurve>
    {
        protected override BaseField<AnimationCurve> CreateField() => new CurveField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class BoundsDrawer : BuiltinDrawerBase<Bounds>
    {
        protected override BaseField<Bounds> CreateField() => new BoundsField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class GradientDrawer : BuiltinDrawerBase<Gradient>
    {
        protected override BaseField<Gradient> CreateField() => new GradientField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Vector2IntDrawer : BuiltinDrawerBase<Vector2Int>
    {
        protected override BaseField<Vector2Int> CreateField() => new Vector2IntField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class Vector3IntDrawer : BuiltinDrawerBase<Vector3Int>
    {
        protected override BaseField<Vector3Int> CreateField() => new Vector3IntField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class RectIntDrawer : BuiltinDrawerBase<RectInt>
    {
        protected override BaseField<RectInt> CreateField() => new RectIntField();
    }

    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class BoundsIntDrawer : BuiltinDrawerBase<BoundsInt>
    {
        protected override BaseField<BoundsInt> CreateField() => new BoundsIntField();
    }
}