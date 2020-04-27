using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the Device details from a) cws and b) localDB
  /// </summary>
  public class GetDeviceBySerialExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deviceSerial = CastRequestObjectTo<DeviceSerial>(item, errorCode: 68);
        
      var deviceDataSingleResult = new DeviceDataSingleResult();
      try
      {
        var deviceResponseModel = await cwsDeviceClient.GetDeviceBySerialNumber(deviceSerial.SerialNumber, customHeaders);
        if (deviceResponseModel != null)
        {
          var deviceFromRepo = await deviceRepo.GetDevice(deviceResponseModel.Id);
          if (deviceFromRepo != null)
            deviceDataSingleResult.DeviceDescriptor = new DeviceData()
            {
              CustomerUID = deviceResponseModel.AccountId,
              DeviceUID = deviceResponseModel.Id,
              DeviceName = deviceResponseModel.DeviceName,
              SerialNumber = deviceResponseModel.SerialNumber,
              Status = deviceResponseModel.Status,
              ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetID
            };
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "deviceController", e.Message);
      }
      return deviceDataSingleResult;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
