using System.IO;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get DXF linework for an alignment file.
  /// </summary>
  public class AlignmentLineworkResult : ContractExecutionResult
  {
    public AlignmentLineworkResult(Stream dxfData)
    {
      DxfData = dxfData;
    }

    public Stream DxfData { get; set; }
  }
}
