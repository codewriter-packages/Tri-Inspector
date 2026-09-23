using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class UnitAttribute : Attribute
    {
        public string unitToDisplay;

#region Base Units
        public const string Meter = "m";
        public const string Centimeter = "cm";
        public const string Millimeter = "mm";
        public const string Kilogram = "kg";
        public const string Second = "s";
        public const string Ampere = "A";
        public const string Kelvin = "K";
        public const string Mole = "mol";
        public const string Candela = "cd";
#endregion

#region Derived Units
        public const string Radian = "rad";
        public const string Degree = "°";
        public const string Gram = "g";
        public const string Liter = "L";
        public const string Hertz = "Hz";
        public const string Newton = "N";
        public const string Pascal = "Pa";
        public const string Joule = "J";
        public const string Watt = "W";
        public const string Coulomb = "C";
        public const string Volt = "V";
        public const string Ohm = "Ω";
        public const string Farad = "F";
        public const string Tesla = "T";
        public const string Celsius = "°C";
#endregion

#region Common Combinations
        public const string MeterPerSecond = "m/s";
        public const string MeterPerSquareSecond = "m/s²";
#endregion

#region Extended Physics Units
        public const string Lux = "lx";
        public const string Weber = "Wb";
        public const string Sievert = "Sv";
        public const string Henry = "H";
        public const string Siemens = "S";
        public const string Becquerel = "Bq";
        public const string Gray = "Gy";
        public const string Katal = "kat";
#endregion

        public UnitAttribute(string unit){
            unitToDisplay = unit;
        }
    }
}