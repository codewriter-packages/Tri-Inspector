using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector;
using TriInspector.TypeProcessors;
using UnityEngine;

[assembly: RegisterTriTypeProcessor(typeof(TriRectOffsetTypeProcessor), 1)]

namespace TriInspector.TypeProcessors
{
    public class TriRectOffsetTypeProcessor : TriTypeProcessor
    {
        private static readonly string[] DrawnProperties = new[]
        {
            "left",
            "right",
            "top",
            "bottom",
        };

        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            if (type != typeof(RectOffset))
            {
                return;
            }

            for (var i = 0; i < DrawnProperties.Length; i++)
            {
                var propertyName = DrawnProperties[i];
                var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                var propertyDef = TriPropertyDefinition.CreateForPropertyInfo(i, propertyInfo);

                properties.Add(propertyDef);
            }
        }
    }
}