using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models
{
  /// <summary>
  /// Parameters for a tile generation request using Pegasus.
  /// Files must be located in the path {DataOceanRootFolder}/{CustomerUID}/{ProjectUID} in DataOcean.
  /// </summary>
  public class TileGenerationRequest
  {
    public Guid CustomerUid { get; set; }
    public Guid ProjectUid { get; set; }
    public Guid ImportedFileUid { get; set; }//Id of DXF file required for notification
    public string DataOceanRootFolder { get; set; }
    public string FileName { get; set; }

    public string DataOceanPath => $"{Path.DirectorySeparatorChar}{DataOceanRootFolder}{Path.DirectorySeparatorChar}{CustomerUid}{Path.DirectorySeparatorChar}{ProjectUid}";
    /// <summary>
    /// Validates all properties
    /// </summary>
    public virtual void Validate()
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
      if (string.IsNullOrEmpty(FileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing DXF/GeoTIFF file name"));
      }
    }
  }
}
