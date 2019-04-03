namespace VSS.TRex.Types
{
    /// <summary>
    /// The type of compaction sensor on the machine reporting compaction values
    /// </summary>
    public enum CompactionSensorType : byte
  {
        /// <summary>
        /// No sensor installed
        /// </summary>
        NoSensor,

        /// <summary>
        /// Standard MC024 compaction sensor
        /// </summary>
        MC024,

        /// <summary>
        /// Volkel compaction sensor
        /// </summary>
        Volkel,

        /// <summary>
        /// Sensor fitted as a part of Cat factory fit program
        /// </summary>
        CATFactoryFitSensor
    }
}
