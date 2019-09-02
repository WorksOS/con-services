﻿using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The representation of a elevation statistics request
  /// </summary>
  public class CompactionElevationRangeRequest : ProjectID
  {
    /// <summary>
    /// The filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public CompactionFilter filter { get; private set; }
  }
}
