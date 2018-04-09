namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The type of cvompaction sensor on the machine reporting compaction values
    /// </summary>
    public enum CompactionSensorType
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
