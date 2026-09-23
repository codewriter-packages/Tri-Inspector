using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class DictionaryDrawerSettings : CollectionDrawerSettingsAttribute
    {
        public string KeyLabel { get; set; }
        public string ValueLabel { get; set; }
        public float KeyColumnSize { get; set; }
        public DictionaryLayout Layout { get; set; }
    }
}