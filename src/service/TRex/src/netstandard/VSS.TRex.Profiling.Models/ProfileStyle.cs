namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// The style of production data profiling
  /// </summary>
  public enum ProfileStyle
  {
    /// <summary>
    /// A profile containing detailed cell pass analytics along the profile
    /// </summary>
    CellPasses,

    /// <summary>
    /// A profile containing elevations defining the summary volume along the profile
    /// </summary>
    SummaryVolume
  }
}
