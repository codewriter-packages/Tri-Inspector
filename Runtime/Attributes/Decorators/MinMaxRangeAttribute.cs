using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float MinFixed { get; }
        public float MaxFixed { get; }

        public string MinMemberName { get; }
        public string MaxMemberName { get; }
        public string MinMaxMemberName { get; }

        public MinMaxRangeAttribute() : this(0f, 1f) { }
        public MinMaxRangeAttribute(float min, float max)
        {
            MinFixed = min;
            MaxFixed = max;
        }
        public MinMaxRangeAttribute(string minMemberName, string maxMemberName) : this()
        {
            MinMemberName = minMemberName;
            MaxMemberName = maxMemberName;
        }
        public MinMaxRangeAttribute(float min, string maxMemberName)
        {
            MinFixed = min;
            MaxFixed = 1;
            MaxMemberName = maxMemberName;
        }
        public MinMaxRangeAttribute(string minMemberName, float max)
        {
            MinFixed = 0;
            MaxFixed = max;
            MinMemberName = minMemberName;
        }
        public MinMaxRangeAttribute(string minMaxMemberName) : this()
        {
            MinMaxMemberName = minMaxMemberName;
        }
    }
}