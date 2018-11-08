namespace VSS.TRex.Types
{
    /// <summary>
    ///  The direction the machine is moving in 
    /// </summary>
    public enum MachineDirection
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
}
