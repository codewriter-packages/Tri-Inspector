using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float MinFixed { get; } = 0f;
        public float MaxFixed { get; } = 1f;

        public string MinMemberName { get; }
        public string MaxMemberName { get; }
        public string MinMaxMemberName { get; }

        public bool AutoClamp { get; }


        public MinMaxSliderAttribute() { }
        public MinMaxSliderAttribute(float min, float max)
        {
            MinFixed = min;
            MaxFixed = max;
        }
        public MinMaxSliderAttribute(string minMemberName, string maxMemberName, bool autoClamp = false) : this()
        {
            MinMemberName = minMemberName;
            MaxMemberName = maxMemberName;
            AutoClamp = autoClamp;
        }
        public MinMaxSliderAttribute(float min, string maxMemberName, bool autoClamp = false)
        {
            MinFixed = min;
            MaxMemberName = maxMemberName;
            AutoClamp = autoClamp;
        }
        public MinMaxSliderAttribute(string minMemberName, float max, bool autoClamp = false)
        {
            MaxFixed = max;
            MinMemberName = minMemberName;
            AutoClamp = autoClamp;
        }
        public MinMaxSliderAttribute(string minMaxMemberName, bool autoClamp = false) : this()
        {
            MinMaxMemberName = minMaxMemberName;
            AutoClamp = autoClamp;
        }
    }
}