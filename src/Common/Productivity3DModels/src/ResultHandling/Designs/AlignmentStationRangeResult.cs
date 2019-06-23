﻿using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  public class AlignmentStationRangeResult : ContractExecutionResult
  {
    /// <summary>
    /// The start station for the alignment file
    /// </summary>
    [JsonProperty(PropertyName = "StartStation")]
    public double StartStation { get; private set; }

    /// <summary>
    /// The end station for the alignment file
    /// </summary>
    [JsonProperty(PropertyName = "EndStation")]
    public double EndStation { get; private set; }

    public AlignmentStationRangeResult(double startStation, double endStation)
    {
      StartStation = startStation;
      EndStation = endStation;
    }
  }
}