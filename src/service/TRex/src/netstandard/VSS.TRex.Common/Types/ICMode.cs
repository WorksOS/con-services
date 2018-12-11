namespace VSS.TRex.Types
{
  /// <summary>
  /// Hold Intelligent Compaction mode information
  /// </summary>
  public static class ICModeFlags
  {
    /// <summary>
    /// Null value for an MC024 sensor
    /// </summary>
    public const byte IC_UNKNOWN_INVALID_MC0243_SENSOR_FLAG = 0x8;

    /// <summary>
    /// Vibration On value for Volkel sensor type.
    /// </summary>
    public const byte IC_VOLKEL_SENSOR_VIBRATION_ON_MASK = 0xf;

    /// <summary>
    /// Value for auto-vibration state.
    /// </summary>
    public const byte IC_TEMPERATURE_AUTO_VIBRATION_STATE_MASK = 0x03;

    /// <summary>
    /// Value for vibration state.
    /// </summary>
    public const byte IC_TEMPERATURE_VIBRATION_STATE_MASK = 0x04;

    /// <summary>
    /// Value for vibration state bits shift.
    /// </summary>
    public const byte IC_TEMPERATURE_VIBRATION_STATE_SHIFT = 2;

  }
}
