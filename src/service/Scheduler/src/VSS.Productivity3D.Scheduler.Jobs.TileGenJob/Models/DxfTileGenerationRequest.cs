using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models
{
  /// <summary>
  /// Parameters for a DXF tile generation request using Pegasus.
  /// Files must be located in the path {DataOceanRootFolder}/{CustomerUID}/{ProjectUID} in DataOcean.
  /// </summary>
  public class DxfTileGenerationRequest : TileGenerationRequest
  {
    public string DcFileName { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
 
      if (string.IsNullOrEmpty(DcFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing coordinate system file name"));
      }
    }
  }
}
