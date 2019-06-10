namespace VSS.TRex.Common.Types
{
    /// <summary>
    ///  The direction the machine is moving in 
    /// </summary>
    public enum MachineDirection : byte
  {
        /// <summary>
        /// Machine is moving in machine defined forward direction
        /// </summary>
        Forward,

        /// <summary>
        /// Machine is moving in machine defined reveres direction
        /// </summary>
        Reverse,

        /// <summary>
        /// Machine direction is null or unknown
        /// </summary>
        Unknown
    }

    public static class MachineDirectionConsts
    {
      public const int MACHINE_DIRECTION_MIN_VALUE = (int) MachineDirection.Forward;
      public const int MACHINE_DIRECTION_MAX_VALUE = (int) MachineDirection.Unknown;
    }
}
