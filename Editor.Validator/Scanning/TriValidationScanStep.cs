using System;

namespace TriInspector.Validator
{
    internal readonly struct TriValidationScanStep : IEquatable<TriValidationScanStep>
    {
        public readonly string info;
        public readonly int index;
        public readonly int total;

        public TriValidationScanStep(string info, int index, int total)
        {
            this.info = info;
            this.index = index;
            this.total = total;
        }

        public float Fraction => total > 0 ? (float) index / total : 1f;

        public bool Equals(TriValidationScanStep other)
        {
            return info == other.info && index == other.index && total == other.total;
        }

        public override bool Equals(object obj)
        {
            return obj is TriValidationScanStep other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(info, index, total);
        }
    }
}