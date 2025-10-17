using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class SliderAttribute : Attribute
    {
        public float MinFixed { get; }
        public float MaxFixed { get; }

        public string MinMemberName { get; }
        public string MaxMemberName { get; }
        public string MinMaxMemberName { get; }

        public SliderAttribute() : this(0f, 1f) { }

        public SliderAttribute(float min, float max)
        {
            MinFixed = min;
            MaxFixed = max;
        }

        public SliderAttribute(string minMemberName, string maxMemberName) : this()
        {
            MinMemberName = minMemberName;
            MaxMemberName = maxMemberName;
        }

        public SliderAttribute(float min, string maxMemberName)
        {
            MinFixed = min;
            MaxFixed = 1;
            MaxMemberName = maxMemberName;
        }

        public SliderAttribute(string minMemberName, float max)
        {
            MinFixed = 0;
            MaxFixed = max;
            MinMemberName = minMemberName;
        }
        public SliderAttribute(string minMaxMemberName) : this()
        {
            MinMaxMemberName = minMaxMemberName;
        }
    }
}