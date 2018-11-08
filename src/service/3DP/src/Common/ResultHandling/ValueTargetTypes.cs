namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// Specifies what the summary value represents in terms of the target
  /// </summary>
  public enum ValueTargetType
  {
    /// <summary>
    /// No value for this type of data for this cell
    /// </summary>
    NoData = -1,

    /// <summary>
    /// Value is above target
    /// </summary>
    AboveTarget = 0,

    /// <summary>
    /// Value is on target
    /// </summary>
    OnTarget = 1,

    /// <summary>
    /// Value is below target
    /// </summary>
    BelowTarget = 2
  }
}