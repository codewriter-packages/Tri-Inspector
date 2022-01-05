using System;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriDrawerAttribute : Attribute
    {
        public RegisterTriDrawerAttribute(Type drawerType, int order)
        {
            DrawerType = drawerType;
            Order = order;
        }

        public Type DrawerType { get; }
        public int Order { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriGroupDrawerAttribute : Attribute
    {
        public RegisterTriGroupDrawerAttribute(Type drawerType)
        {
            DrawerType = drawerType;
        }

        public Type DrawerType { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriPropertyHideProcessor : Attribute
    {
        public RegisterTriPropertyHideProcessor(Type processorType)
        {
            ProcessorType = processorType;
        }

        public Type ProcessorType { get; }
    }
    
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTriPropertyDisableProcessor : Attribute
    {
        public RegisterTriPropertyDisableProcessor(Type processorType)
        {
            ProcessorType = processorType;
        }

        public Type ProcessorType { get; }
    }
}