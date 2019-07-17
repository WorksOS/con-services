using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionSurfaceExportRequest : CompactionExportRequest
  {
    /// <summary>
    /// Sets the tolerance to calculate TIN surfaces.
    /// </summary>
    /// <remarks>
    /// The value should be in meters.
    /// </remarks>
    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double? Tolerance { get; set; }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filter"></param>
    /// <param name="fileName"></param>
    /// <param name="tolerance"></param>
    public CompactionSurfaceExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      double tolerance)
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
      Tolerance = tolerance;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public new void Validate()
    {
      base.Validate();

    }
  }
}
