using Newtonsoft.Json;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get boundaries from a DXF linework file.
  /// </summary>
  public class DxfLineworkFileResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "lineworkBoundaries", Required = Required.Default)]
    public TWGS84LineworkBoundary[] LineworkBoundaries { get; private set; }

    public static DxfLineworkFileResult Create(int code, string message, TWGS84LineworkBoundary[] lineworkBoundaries)
    {
      return new DxfLineworkFileResult
      {
        LineworkBoundaries = lineworkBoundaries,
        Message = message,
        Code = code
      };
    }
  }
}
