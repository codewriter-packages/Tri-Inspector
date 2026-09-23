using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;

namespace TriInspector.Editor.Integrations.Odin
{
    public static class TriOdinUtility
    {
        private static readonly Dictionary<Assembly, bool> TriDrawnAssemblies = new Dictionary<Assembly, bool>();
        private static HashSet<Type> _triDrawnTypes;

        public static bool IsDrawnByTri(Type type)
        {
            if (_triDrawnTypes == null)
            {
                var list = TypeCache.GetTypesWithAttribute<DrawWithTriInspectorAttribute>();
                var array = new Type[list.Count];
                list.CopyTo(array, 0);
                _triDrawnTypes = new HashSet<Type>(array);
            }

            if (_triDrawnTypes.Contains(type))
            {
                return true;
            }

            var asm = type.Assembly;
            if (TriDrawnAssemblies.TryGetValue(asm, out var assemblyDrawnByTri))
            {
                return assemblyDrawnByTri;
            }

            assemblyDrawnByTri = asm.IsDefined<DrawWithTriInspectorAttribute>(false);
            TriDrawnAssemblies[asm] = assemblyDrawnByTri;
            return assemblyDrawnByTri;
        }
    }
}