using System.IO;
using DesignProfilerDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get DXF linework for an alignment file.
  /// </summary>
  public class AlignmentLineworkResult : ContractExecutionResult
  {
    public AlignmentLineworkResult(TDesignProfilerRequestResult result, string message, Stream dxfData)
    {
      Code = (int) result;
      Message = message;
      DxfData = dxfData;
    }

    public Stream DxfData { get; set; }
  }
}
