using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Internal;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob.Models
{
  /// <summary>
  /// Parameters for a DXF tile generation request using Pegasus.
  /// Files must be located in the path {DataOceanRootFolder}/{CustomerUid}/{ProjectUid} in DataOcean.
  /// </summary>
  public class AssetStatusRequest
  {
    public List<string> AssetIdentifier { get; set; }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (AssetIdentifier.Exists(string.IsNullOrWhiteSpace))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing asset identifier"));
      }
    }
  }
}
