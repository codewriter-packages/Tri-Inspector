namespace TriInspector
{
    public abstract class TriCustomDrawer
    {
        internal int Order { get; set; }
        internal bool ApplyOnArrayElement { get; set; }

        public abstract TriElement CreateElementInternal(TriProperty property, TriElement next);
    }
}