namespace VSS.TRex.Types
{
    /// <summary>
    /// The known information about if the blade is in contact with the ground, and the 
    /// mechanism used to report that state
    /// </summary>
    public enum OnGroundState
    {
        No,
        YesLegacy,
        YesMachineConfig,
        YesMachineHardware,
        YesMachineSoftware,
        YesRemoteSwitch,
        Unknown
    }
}
