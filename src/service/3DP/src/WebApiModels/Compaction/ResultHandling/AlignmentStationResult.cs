using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class AlignmentStationResult : ContractExecutionResult
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

    public static AlignmentStationResult CreateAlignmentOffsetResult(double startStation, double endStation)
    {
      return new AlignmentStationResult() {StartStation = startStation, EndStation = endStation };
    }

  }
}
