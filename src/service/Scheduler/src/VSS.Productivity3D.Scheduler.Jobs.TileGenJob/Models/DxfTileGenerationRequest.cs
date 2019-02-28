using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models
{
  /// <summary>
  /// Parameters for a DXF tile generation request using Pegasus.
  /// Files must be located in the path {DataOceanRootFolder}/{CustomerUid}/{ProjectUid} in DataOcean.
  /// </summary>
  public class DxfTileGenerationRequest
  {
    public Guid CustomerUid { get; set; }
    public Guid ProjectUid { get; set; }
    public Guid ImportedFileUid { get; set; }//Id of DXF file required for notification
    public string DataOceanRootFolder { get; set; }
    public string DxfFileName { get; set; }
    public string DcFileName { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (CustomerUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing customer uid"));
      }
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project uid"));
      }
      if (ImportedFileUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing imported file uid"));
      }
      if (string.IsNullOrEmpty(DataOceanRootFolder))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing root folder"));
      }
      if (string.IsNullOrEmpty(DxfFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing DXF file name"));
      }
      if (string.IsNullOrEmpty(DcFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing coordinate system file name"));
      }
    }
  }
}
