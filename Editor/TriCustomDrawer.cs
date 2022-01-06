namespace TriInspector
{
    public abstract class TriCustomDrawer
    {
        internal int Order { get; set; }
        internal TriTargetPropertyType Target { get; set; }

        public abstract TriElement CreateElementInternal(TriProperty property, TriElement next);
    }
}