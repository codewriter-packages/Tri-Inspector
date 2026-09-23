using System;
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class AssetDropdownAttribute : Attribute
    {
        public string Filter { get; }
        public string[] SearchInFolders { get; }
        public bool HideNextDrawer { get; set; } = true;
        public bool Advanced { get; set; } = true;

        public AssetDropdownAttribute(string filter, string[] searchInFolders = null)
        {
            Filter = filter;
            SearchInFolders = searchInFolders;
        }

        public virtual string GetDisplayName(Object asset)
        {
            return asset.name;
        }
    }
}