using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class InlineButtonAttribute : Attribute
    {
        public InlineButtonAttribute(string name)
        {
            Name = name;
        }

        public InlineButtonAttribute(string label, string name, string isChecked = null) : this(name)
        {
            ButtonLabel = label;
            IsChecked = isChecked;
        }

        public string Name { get; set; }

        /// <summary>
        /// Optional custom label for the button. If not set, the method name is used.
        /// </summary>
        public string ButtonLabel { get; set; }

        /// <summary>
        /// Optional bool member that drives the button's active/checked state.
        /// </summary>
        public string IsChecked { get; set; }

        /// <summary>
        /// Width of the inline button in pixels. Default is 60.
        /// </summary>
        public float ButtonWidth { get; set; }
    }
}