using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class ProjectStatisticsResult : BaseDataResult
  {
    /// <summary>
    /// Earlist time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    [JsonProperty(PropertyName = "startTime", Required = Required.Default)]
    public DateTime startTime;

    /// <summary>
    /// Latest time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    [JsonProperty(PropertyName = "endTime", Required = Required.Default)]
    public DateTime endTime;

    /// <summary>
    /// Size of spatial data cells in the project (the default value is 34cm)
    /// </summary>
    [JsonProperty(PropertyName = "cellSize", Required = Required.Default)]
    public double cellSize;

    /// <summary>
    /// The index origin offset from the absolute bottom left origin of the subgrid tree cartesian coordinate system to the centered origin of the cartesian
    /// grid coordinate system used in the project, and the centered origin cartesian coordinates of cell addresses.
    /// </summary>
    [JsonProperty(PropertyName = "indexOriginOffset", Required = Required.Default)]
    public int indexOriginOffset;

    /// <summary>
    /// The three dimensional extents of the project including both production and surveyed surface data.
    /// </summary>
    [JsonProperty(PropertyName = "extents", Required = Required.Default)]
    public BoundingBox3DGrid extents;

  }
}
