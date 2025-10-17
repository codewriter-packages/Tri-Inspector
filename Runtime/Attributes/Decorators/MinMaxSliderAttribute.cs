using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float MinFixed { get; }
        public float MaxFixed { get; }

        public string MinMemberName { get; }
        public string MaxMemberName { get; }
        public string MinMaxMemberName { get; }

        public MinMaxSliderAttribute() : this(0f, 1f) { }
        public MinMaxSliderAttribute(float min, float max)
        {
            MinFixed = min;
            MaxFixed = max;
        }
        public MinMaxSliderAttribute(string minMemberName, string maxMemberName) : this()
        {
            MinMemberName = minMemberName;
            MaxMemberName = maxMemberName;
        }
        public MinMaxSliderAttribute(float min, string maxMemberName)
        {
            MinFixed = min;
            MaxFixed = 1;
            MaxMemberName = maxMemberName;
        }
        public MinMaxSliderAttribute(string minMemberName, float max)
        {
            MinFixed = 0;
            MaxFixed = max;
            MinMemberName = minMemberName;
        }
        public MinMaxSliderAttribute(string minMaxMemberName) : this()
        {
            MinMaxMemberName = minMaxMemberName;
        }
    }
}