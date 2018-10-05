namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Load/dump location of a cycle for unified productivity.
  /// </summary>
  public class LoadDumpLocation
  {
    /// <summary>
    /// Latitude of load event in degrees
    /// </summary>
    public double loadLatitude { get; set; }
    /// <summary>
    /// Longitude of load event in degrees
    /// </summary>
    public double loadLongitude { get; set; }
    /// <summary>
    /// Latitude of dump event in degrees
    /// </summary>
    public double dumpLatitude { get; set; }
    /// <summary>
    /// Longitude of dump event in degrees
    /// </summary>
    public double dumpLongitude { get; set; }
  }
}
