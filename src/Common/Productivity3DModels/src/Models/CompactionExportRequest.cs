using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.FIlters;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionExportRequest : TRexBaseRequest
  {
    /// <summary>
    /// The name of the exported data file.
    /// </summary>
    /// <remarks>
    /// The value should contain only ASCII characters.
    /// </remarks>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; set; }


    /// <summary>
    /// Default private constructor.
    /// </summary>
    public CompactionExportRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

  }
}
