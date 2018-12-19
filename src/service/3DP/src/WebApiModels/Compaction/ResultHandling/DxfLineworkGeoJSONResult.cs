using ASNodeDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get boundaries from a DXF linework file.
  /// </summary>
  public class DxfLineworkGeoJSONResult : ContractExecutionResult
  {
    public TWGS84LineworkBoundary[] LineworkBoundaries { get; }

    public DxfLineworkGeoJSONResult(TASNodeErrorStatus code, string message, TWGS84LineworkBoundary[] lineworkBoundaries)
    {
      LineworkBoundaries = lineworkBoundaries;
      Message = message;
      Code = (int)code;
    }
  }
}
