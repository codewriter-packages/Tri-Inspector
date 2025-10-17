using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    public static partial class TriEditorGUI
    {
        public static void DrawMinMaxSlider(Rect rect, ref float xValue, ref float yValue, float minValue, float maxValue)
        {
            var fieldWidth = EditorGUIUtility.fieldWidth;
            var minFieldRect = new Rect(rect.xMin, rect.y, fieldWidth, rect.height);
            var maxFieldRect = new Rect(rect.xMax - fieldWidth, rect.y, fieldWidth, rect.height);

            //set slider rect between min and max fields + additional padding
            const float spacing = 8.0f;
            var sliderRect = Rect.MinMaxRect(minFieldRect.xMax + spacing,
                                             rect.yMin,
                                             maxFieldRect.xMin - spacing,
                                             rect.yMax);

            EditorGUI.BeginChangeCheck();
            xValue = EditorGUI.FloatField(minFieldRect, xValue);
            yValue = EditorGUI.FloatField(maxFieldRect, yValue);
            EditorGUI.MinMaxSlider(sliderRect, ref xValue, ref yValue, minValue, maxValue);

            //values validation (xValue can't be higher than yValue etc.)
            xValue = Mathf.Clamp(xValue, minValue, Mathf.Min(maxValue, yValue));
            yValue = Mathf.Clamp(yValue, Mathf.Max(minValue, xValue), maxValue);
        }
        public static void DrawMinMaxSlider(Rect rect, string label, ref float xValue, ref float yValue, float minValue, float maxValue)
        {
            DrawMinMaxSlider(rect, new GUIContent(label), ref xValue, ref yValue, minValue, maxValue);
        }
        public static void DrawMinMaxSlider(Rect rect, GUIContent label, ref float xValue, ref float yValue, float minValue, float maxValue)
        {
            rect = EditorGUI.PrefixLabel(rect, label);
            DrawMinMaxSlider(rect, ref xValue, ref yValue, minValue, maxValue);
        }
    }
}
