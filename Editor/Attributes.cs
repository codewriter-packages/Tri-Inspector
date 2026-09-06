using System;
using JetBrains.Annotations;

namespace TriInspector
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTriValueDrawerAttribute : Attribute
    {
        public RegisterTriValueDrawerAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
        public bool ApplyOnArrayElement { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriAttributeDrawerAttribute : Attribute
    {
        public RegisterTriAttributeDrawerAttribute(Type drawerType, int order)
        {
            DrawerType = drawerType;
            Order = order;
        }

        public Type DrawerType { get; }
        public int Order { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTriGroupDrawerAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriPropertyHideProcessor : Attribute
    {
        public RegisterTriPropertyHideProcessor(Type processorType)
        {
            ProcessorType = processorType;
        }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriPropertyDisableProcessor : Attribute
    {
        public RegisterTriPropertyDisableProcessor(Type processorType)
        {
            ProcessorType = processorType;
        }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriValueValidatorAttribute : Attribute
    {
        public RegisterTriValueValidatorAttribute(Type validatorType)
        {
            ValidatorType = validatorType;
        }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriAttributeValidatorAttribute : Attribute
    {
        public RegisterTriAttributeValidatorAttribute(Type validatorType)
        {
            ValidatorType = validatorType;
        }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }
    
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTriTypeProcessorAttribute : Attribute
    {
        public RegisterTriTypeProcessorAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}