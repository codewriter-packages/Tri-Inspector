using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class DynamicRangeAttribute : Attribute
    {
        public float MinFixed { get; }
        public float MaxFixed { get; }

        public string MinMemberName { get; }
        public string MaxMemberName { get; }
        public string MinMaxMemberName { get; }

        public DynamicRangeAttribute() : this(0f, 1f) { }

        public DynamicRangeAttribute(float min, float max)
        {
            MinFixed = min;
            MaxFixed = max;
        }

        public DynamicRangeAttribute(string minMemberName, string maxMemberName) : this()
        {
            MinMemberName = minMemberName;
            MaxMemberName = maxMemberName;
        }

        public DynamicRangeAttribute(float min, string maxMemberName)
        {
            MinFixed = min;
            MaxFixed = 1;
            MaxMemberName = maxMemberName;
        }

        public DynamicRangeAttribute(string minMemberName, float max)
        {
            MinFixed = 0;
            MaxFixed = max;
            MinMemberName = minMemberName;
        }
        public DynamicRangeAttribute(string minMaxMemberName) : this()
        {
            MinMaxMemberName = minMaxMemberName;
        }
    }
}