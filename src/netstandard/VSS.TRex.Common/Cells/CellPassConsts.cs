using System;
using VSS.TRex.Types;

namespace VSS.TRex.Common.CellPasses
{
  public static class CellPassConsts
  {
    /// <summary>
    /// Null GPS tolerance value
    /// </summary>
    public const ushort NullGPSTolerance = UInt16.MaxValue;

    /// <summary>
    /// Null machine speed value
    /// </summary>
    public const ushort NullMachineSpeed = Consts.NullMachineSpeed;

    /// <summary>
    /// Null Pass Count value
    /// </summary>
    public const ushort NullPassCountValue = UInt16.MinValue;

    /// <summary>
    /// Conversion ratio between temperature in whole degrees and tenths of degress reported by some measurements from machines
    /// </summary>
    public const short MaterialTempValueRatio = 10;

    /// <summary>
    /// Value representing a minimum material temperature encoded as an IEEE ushort
    /// </summary>
    public const ushort MinMaterialTempValue = 0;

    /// <summary>
    /// Value representing a maximum temperature value, that may be reported, encoded as an IEEE ushort
    /// </summary>
    public const ushort MaxMaterialTempValue = 4095;

    // Value representing a null material temperature encoded as an IEEE ushort
    public const ushort NullMaterialTemperatureValue = MaxMaterialTempValue + 1;

    /// <summary>
    /// Null machine ID. This is the null site model machine reference ID, not the null Guid for machines
    /// </summary>
    //public const long NullMachineID = 0;
    public const short NullInternalSiteModelMachineIndex = short.MinValue;

    public static DateTime NullTime = DateTime.MinValue;

    /// <summary>
    /// Null GPSMode value
    /// </summary>
    public const GPSMode NullGPSMode = GPSMode.NoGPS;

    /// <summary>
    /// NUll height (NEE Elevation) value. This is an IEEE Single (Float) value
    /// </summary>
    public const float NullHeight = Consts.NullHeight;

    /// <summary>
    /// Null CCV value
    /// </summary>
    public const short NullCCV = short.MaxValue;

    /// <summary>
    /// Maximum Pass Count value
    /// </summary>
    public const ushort MaxPassCountValue = ushort.MaxValue;

    /// <summary>
    /// Null radio correction latency value
    /// </summary>
    public const byte NullRadioLatency = byte.MaxValue; // This is the same value as kSVOAsBuiltNullRadio

    /// <summary>
    /// Null Resonance Meter Value
    /// </summary>
    public const short NullRMV = short.MaxValue;

    /// <summary>
    /// Null vibratory drum vibration frequency value
    /// </summary>
    public const ushort NullFrequency = ushort.MaxValue;

    /// <summary>
    /// Null vibratory drum amplitude value
    /// </summary>
    public const ushort NullAmplitude = ushort.MaxValue;

    /// <summary>
    /// Null Machine Drive Power compaction value
    /// </summary>
    public const short NullMDP = short.MaxValue;

    /// <summary>
    /// Null Caterpillar Compaction Algorithm value
    /// </summary>
    public const byte NullCCA = byte.MaxValue;

    /// <summary>
    /// Null machine type value
    /// </summary>
    public const byte MachineTypeNull = 0;

    /// <summary>
    /// Null value for the Volkel compaction sensor measurement range (defined as int, but null is byte.MaxValue)
    /// </summary>
    public const int NullVolkelMeasRange = byte.MaxValue;

    /// <summary>
    /// The null value for the Volkel compaction machine measurement util range 
    /// </summary>
    public const int NullVolkelMeasUtilRange = -1;

    /// <summary>
    /// The null value for machine gear
    /// </summary>
    public const MachineGear NullMachineGear = MachineGear.Null;

    /// <summary>
    /// Null layer ID value
    /// </summary>
    public const ushort NullLayerID = ushort.MaxValue;

    /// <summary>
    /// Null 3D sonic sensor value
    /// </summary>
    public const byte Null3DSonic = byte.MaxValue;

    /// <summary>
    /// Null value for target lift thickness override value specified from a machine
    /// </summary>
    public const float NullOverridingTargetLiftThicknessValue = Consts.NullHeight;

    /// <summary>
    /// Null value for a Universal Tranverse Mercatore zone reference
    /// </summary>
    public const byte NullUTMZone = 0;
  }
}
