namespace VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models
{
  /// <summary>
  ///   Describes VL projectResponse
  /// </summary>
  public class ProjectDescriptor
  {
    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
    /// </value>
    public bool isArchived { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is landfill.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is landfill; otherwise, <c>false</c>.
    /// </value>
    public bool isLandFill { get; set; }
  }
}