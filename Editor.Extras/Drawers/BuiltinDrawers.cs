using System;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriValueDrawer(typeof(IntegerDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(LongDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(UnsignedIntegerDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(UnsignedLongDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(BooleanDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(FloatDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(DoubleDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(StringDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(ColorDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Color32Drawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(LayerMaskDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(EnumDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Vector2Drawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Vector3Drawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Vector4Drawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(RectDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(AnimationCurveDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(BoundsDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(GradientDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Vector2IntDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(Vector3IntDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(RectIntDrawer), TriDrawerOrder.Fallback)]
[assembly: RegisterTriValueDrawer(typeof(BoundsIntDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class StringDrawer : BuiltinDrawerBase<string>
    {
        protected override BaseField<string> CreateField() => new TextField();
    }

    public class BooleanDrawer : BuiltinDrawerBase<bool>
    {
        protected override BaseField<bool> CreateField() => new Toggle();
    }

    public class IntegerDrawer : BuiltinDrawerBase<int>
    {
        protected override BaseField<int> CreateField() => new IntegerField();
    }

    public class LongDrawer : BuiltinDrawerBase<long>
    {
        protected override BaseField<long> CreateField() => new LongField();
    }

    public class UnsignedIntegerDrawer : BuiltinDrawerBase<uint>
    {
        protected override BaseField<uint> CreateField() => new UnsignedIntegerField();
    }

    public class UnsignedLongDrawer : BuiltinDrawerBase<ulong>
    {
        protected override BaseField<ulong> CreateField() => new UnsignedLongField();
    }

    public class FloatDrawer : BuiltinDrawerBase<float>
    {
        protected override BaseField<float> CreateField() => new FloatField();
    }

    public class DoubleDrawer : BuiltinDrawerBase<double>
    {
        protected override BaseField<double> CreateField() => new DoubleField();
    }

    public class ColorDrawer : BuiltinDrawerBase<Color>
    {
        protected override BaseField<Color> CreateField() => new ColorField();
    }

    public class Color32Drawer : BuiltinDrawerBase<Color32>
    {
        public override VisualElement CreateVisualElement(TriValue<Color32> propertyValue, VisualElement next)
        {
            var field = new ColorField();
            field.BindTri(propertyValue, v => v, v => v);
            return field;
        }
    }

    public class LayerMaskDrawer : BuiltinDrawerBase<LayerMask>
    {
        public override VisualElement CreateVisualElement(TriValue<LayerMask> propertyValue, VisualElement next)
        {
            var field = new LayerMaskField();
            field.BindTri(propertyValue, v => v.value, v => v);
            return field;
        }
    }

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

    public class Vector2Drawer : BuiltinDrawerBase<Vector2>
    {
        protected override BaseField<Vector2> CreateField() => new Vector2Field();
    }

    public class Vector3Drawer : BuiltinDrawerBase<Vector3>
    {
        protected override BaseField<Vector3> CreateField() => new Vector3Field();
    }

    public class Vector4Drawer : BuiltinDrawerBase<Vector4>
    {
        protected override BaseField<Vector4> CreateField() => new Vector4Field();
    }

    public class RectDrawer : BuiltinDrawerBase<Rect>
    {
        protected override BaseField<Rect> CreateField() => new RectField();
    }

    public class AnimationCurveDrawer : BuiltinDrawerBase<AnimationCurve>
    {
        protected override BaseField<AnimationCurve> CreateField() => new CurveField();
    }

    public class BoundsDrawer : BuiltinDrawerBase<Bounds>
    {
        protected override BaseField<Bounds> CreateField() => new BoundsField();
    }

    public class GradientDrawer : BuiltinDrawerBase<Gradient>
    {
        protected override BaseField<Gradient> CreateField() => new GradientField();
    }

    public class Vector2IntDrawer : BuiltinDrawerBase<Vector2Int>
    {
        protected override BaseField<Vector2Int> CreateField() => new Vector2IntField();
    }

    public class Vector3IntDrawer : BuiltinDrawerBase<Vector3Int>
    {
        protected override BaseField<Vector3Int> CreateField() => new Vector3IntField();
    }

    public class RectIntDrawer : BuiltinDrawerBase<RectInt>
    {
        protected override BaseField<RectInt> CreateField() => new RectIntField();
    }

    public class BoundsIntDrawer : BuiltinDrawerBase<BoundsInt>
    {
        protected override BaseField<BoundsInt> CreateField() => new BoundsIntField();
    }
}