using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Validates all device event data sent to the Web API
  /// </summary>
  public class DeviceDataValidator
  {
    private static readonly ProjectErrorCodesProvider _projectErrorCodesProvider = new ProjectErrorCodesProvider();

    public static void ValidateProjectUid(Guid projectUid)
    {
      if (projectUid == Guid.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(_projectErrorCodesProvider.GetErrorNumberwithOffset(128),
            _projectErrorCodesProvider.FirstNameWithOffset(137)));
    }

    public static void ValidateEarliestOfInterestUtc(DateTime? earliestOfInterestUtc)
    {
      if (earliestOfInterestUtc != null && earliestOfInterestUtc > DateTime.UtcNow)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(_projectErrorCodesProvider.GetErrorNumberwithOffset(138),
            _projectErrorCodesProvider.FirstNameWithOffset(138)));
      }
    }

    public static void ValidateDeviceName(string deviceName)
    {
      if (deviceName == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(_projectErrorCodesProvider.GetErrorNumberwithOffset(128),
            _projectErrorCodesProvider.FirstNameWithOffset(128)));
      }
    }
  }
}
