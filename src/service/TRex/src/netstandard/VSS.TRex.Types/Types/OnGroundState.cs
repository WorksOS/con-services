namespace VSS.TRex.Types
{
    /// <summary>
    /// The known information about if the blade is in contact with the ground, and the 
    /// mechanism used to report that state
    /// </summary>
    public enum OnGroundState : byte
    {
        No,
        YesLegacy,
        YesMachineConfig,
        YesMachineHardware,
        YesMachineSoftware,
        YesRemoteSwitch,
        Unknown
    }

    public static class OnGroundStateConsts
  {
      public const int ON_GROUND_STATE_MIN_VALUE = (int)OnGroundState.No;
      public const int ON_GROUND_STATE_MAX_VALUE = (int)OnGroundState.Unknown;
    }
}
