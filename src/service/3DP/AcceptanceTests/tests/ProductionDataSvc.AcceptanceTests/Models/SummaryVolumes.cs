namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a summary volumes request.
  /// </summary>
  public class SummaryVolumesParameters : SummaryParametersBase
  {
    /// <summary>
    /// The type of volume computation to be performed as a summary volumes request
    /// </summary>
    public VolumesType volumeCalcType { get; set; }

    /// <summary>
    /// The descriptor of the design surface to be used as the base or earliest surface for design-filter volumes
    /// </summary>
    public DesignDescriptor baseDesignDescriptor { get; set; }

    /// <summary>
    /// The descriptor of the design surface to be used as the top or latest surface for filter-design volumes
    /// </summary>
    public DesignDescriptor topDesignDescriptor { get; set; }

    /// <summary>
    /// Sets the cut tolerance to calculate Summary Volumes.
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    public double? CutTolerance { get; set; }

    /// <summary>
    /// Sets the fill tolerance to calculate Summary Volumes.
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    public double? FillTolerance { get; set; }
  }
}
