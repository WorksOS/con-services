namespace VSS.TRex.Types
{
    /// <summary>
    /// Machine gear selected at the time of cell pass measurement
    /// </summary>
    public enum MachineGear : byte
  {
        Neutral,
        Forward,
        Reverse,
        SensorFailedDeprecated,
        Forward2,
        Forward3,
        Forward4,
        Forward5,
        Reverse2,
        Reverse3,
        Reverse4,
        Reverse5,
        Park,
        SensorFailed,
        Null = 255
    }

    public static class MachineGearConsts
    {
    public const int MACHINE_GEAR_MIN_VALUE = (int) MachineGear.Neutral;
    public const int MACHINE_GEAR_MAX_VALUE = (int) MachineGear.SensorFailed;
  }
}
