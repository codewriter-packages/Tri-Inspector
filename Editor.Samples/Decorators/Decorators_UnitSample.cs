using System;
using System.Globalization;
using TriInspector;
using UnityEngine;

public class Decorators_UnitSample : ScriptableObject
{
    [Unit("My custom Unit")]
    public float freeTextUnit;

    [Unit("$" + nameof(GetDynamicUnit))]
    public float dynamicUnit;

    [Unit(UnitAttribute.Meter)]
    public float lengthInMeters;

    [Unit(UnitAttribute.Centimeter)]
    public float lengthInCentimeters;

    [Unit(UnitAttribute.Millimeter)]
    public float lengthInMillimeters;

    [Unit(UnitAttribute.Kilogram)]
    public float massInKilograms;

    [Unit(UnitAttribute.Gram)]
    public float massInGrams;

    [Unit(UnitAttribute.Second)]
    public float timeInSeconds;

    [Unit(UnitAttribute.Ampere)]
    public float electricCurrentInAmperes;

    [Unit(UnitAttribute.Kelvin)]
    public float temperatureInKelvins;

    [Unit(UnitAttribute.Celsius)]
    public float temperatureInCelsius;

    [Unit(UnitAttribute.Mole)]
    public float amountOfSubstanceInMoles;

    [Unit(UnitAttribute.Candela)]
    public float luminousIntensityInCandelas;

    [Unit(UnitAttribute.Radian)]
    public float angleInRadians;

    [Unit(UnitAttribute.Degree)]
    public float angleInDegrees;

    [Unit(UnitAttribute.Liter)]
    public float volumeInLiters;

    [Unit(UnitAttribute.Hertz)]
    public float frequencyInHertz;

    [Unit(UnitAttribute.Newton)]
    public float forceInNewtons;

    [Unit(UnitAttribute.Pascal)]
    public float pressureInPascals;

    [Unit(UnitAttribute.Joule)]
    public float energyInJoules;

    [Unit(UnitAttribute.Watt)]
    public float powerInWatts;

    [Unit(UnitAttribute.Coulomb)]
    public float electricChargeInCoulombs;

    [Unit(UnitAttribute.Volt)]
    public float electricPotentialInVolts;

    [Unit(UnitAttribute.Ohm)]
    public float resistanceInOhms;

    [Unit(UnitAttribute.Farad)]
    public float capacitanceInFarads;

    [Unit(UnitAttribute.Tesla)]
    public float magneticFluxDensityInTeslas;

    [Unit(UnitAttribute.Lux)]
    public float illuminanceInLux;

    [Unit(UnitAttribute.Weber)]
    public float magneticFluxInWebers;

    [Unit(UnitAttribute.Sievert)]
    public float doseEquivalentInSieverts;

    [Unit(UnitAttribute.Henry)]
    public float inductanceInHenrys;

    [Unit(UnitAttribute.Siemens)]
    public float conductanceInSiemens;

    [Unit(UnitAttribute.Becquerel)]
    public float radioactivityInBecquerels;

    [Unit(UnitAttribute.Gray)]
    public float absorbedDoseInGrays;

    [Unit(UnitAttribute.Katal)]
    public float catalyticActivityInKatals;

    [Unit(UnitAttribute.MeterPerSecond)]
    public float speedInMetersPerSecond;

    [Unit(UnitAttribute.MeterPerSquareSecond)]
    public float accelerationInMetersPerSquareSecond;

    private string GetDynamicUnit()
    {
        return DateTime.Now.ToString(CultureInfo.CurrentCulture);
    }
}