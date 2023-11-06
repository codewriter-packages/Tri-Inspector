﻿using UnityEngine;

namespace TriInspector.Elements
{
    public class TriLabelElement : TriElement
    {
        private readonly GUIContent _label;

        public TriLabelElement(string label, string tooltip = "")
        {
            _label = new GUIContent(label, tooltip);
        }

        public TriLabelElement(GUIContent label)
        {
            _label = label;
        }

        public override float GetHeight(float width)
        {
            return GUI.skin.label.CalcHeight(_label, width);
        }

        public override void OnGUI(Rect position)
        {
            GUI.Label(position, _label);
        }
    }
}