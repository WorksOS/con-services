using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  public class AlignmentStationRangeResult : ContractExecutionResult, IMasterDataModel
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

    public List<string> GetIdentifiers() => new List<string>();
  }
}
