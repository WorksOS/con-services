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
    protected static ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();

    /// <summary>
    /// Validate the device name
    /// </summary>
    /// <param name="deviceName"></param>
    public static void ValidateDeviceName(string deviceName)
    {
      if (deviceName == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(128),
            projectErrorCodesProvider.FirstNameWithOffset(128)));
      }
    }
  }
}
