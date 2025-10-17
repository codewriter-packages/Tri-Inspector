using TriInspector;

public class DynamicRangeAttribute : SliderAttribute
{
    public DynamicRangeAttribute() : base() { }
    public DynamicRangeAttribute(float min, float max) : base(min, max) { }
    public DynamicRangeAttribute(string minMemberName, string maxMemberName) : base(minMemberName, maxMemberName) { }
    public DynamicRangeAttribute(float min, string maxMemberName) : base(min, maxMemberName) { }
    public DynamicRangeAttribute(string minMemberName, float max) : base(minMemberName, max) { }
    public DynamicRangeAttribute(string minMaxMemberName) : base(minMaxMemberName) { }

}