namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The range of permissible temperatures outside of which a warning is issued.
  /// This is copied from ...\RaptorServicesCommon\Models\TemperatureWarningLevels.cs 
  /// </summary>
  public class TemperatureWarningLevels
  {
    /// <summary>
    /// The mimumum permitted value in 10ths of a degree celcius. For example, 300 means 30.0°C.
    /// </summary>
    public ushort min { get; set; }

    /// <summary>
    /// The maximum permitted value in 10ths of a degree celcius. For example, 800 means 80.0°C.
    /// </summary>
    public ushort max { get; set; }
  }
}