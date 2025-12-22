using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class TabAttribute : Attribute
    {
        public TabAttribute(string tab, int row = 0)
        {
            TabName = tab;
            Row = row;
        }

        public string TabName { get; }
        public int Row {get;}
    }
}