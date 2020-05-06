using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets Device details from a) cws and b) localDB
  /// </summary>
  public class GetDeviceBySerialExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deviceSerial = CastRequestObjectTo<DeviceSerial>(item, errorCode: 68);

      try
      {
        var deviceData = new DeviceData();

        var deviceResponseModel = await cwsDeviceClient.GetDeviceBySerialNumber(deviceSerial.SerialNumber, customHeaders);
        if (deviceResponseModel == null)
        {
          var message = "Unable to locate device by serialNumber in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message}");
          return new DeviceDataSingleResult(code: 100, message: message, deviceData); 
        }

        deviceData = new DeviceData() {DeviceUID = deviceResponseModel.Id, DeviceName = deviceResponseModel.DeviceName, SerialNumber = deviceResponseModel.SerialNumber};

        // store deviceUid and a shortRaptorAssetId will be assigned. Raptor may be obsolete, but this for now will allow various tests to pass
        await deviceRepo.StoreEvent(AutoMapperUtility.Automapper.Map<CreateDeviceEvent>(deviceData));

        var deviceFromRepo = await deviceRepo.GetDevice(deviceResponseModel.Id);
        if (deviceFromRepo == null)
        {
          var message = "Unable to locate device in localDB";
          log.LogInformation($"GetDeviceBySerialExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDataSingleResult(code: 101, message: message, deviceData);
        }

        deviceData.ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetID;

        // now get the customerId and 2xstatus
        // Note that this step may not be needed in future if/when WM can return these fields in cwsDeviceClient.GetDeviceBySerialNumber()
        var deviceAccountListDataResult = await cwsDeviceClient.GetAccountsForDevice(new Guid(deviceData.DeviceUID), customHeaders);
        if (deviceAccountListDataResult?.Accounts == null || !deviceAccountListDataResult.Accounts.Any())
        {
          var message = "Unable to locate any account for the device in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDataSingleResult(code: 102, message: message, deviceData); 
        }

        log.LogInformation($"GetDeviceBySerialExecutor: deviceAccountListDataResult {JsonConvert.SerializeObject(deviceAccountListDataResult)}");
        if (deviceAccountListDataResult.Accounts
          .Count(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0) > 1)
        {
          var message = "There is >1 active account for the device in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDataSingleResult(code: 103, message: message, deviceData);
        }

        var deviceCustomer = deviceAccountListDataResult.Accounts
          .FirstOrDefault(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0);
        deviceData.CustomerUID = deviceCustomer.Id;
        deviceData.RelationStatus = deviceCustomer.RelationStatus;
        deviceData.TccDeviceStatus = deviceCustomer.TccDeviceStatus;
        log.LogInformation($"GetDeviceBySerialExecutor: deviceData {JsonConvert.SerializeObject(deviceData)}");
        return new DeviceDataSingleResult(deviceData); 
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 104 - 2000, "getDeviceBySerialExecutor", e.Message);
      }

      return null;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
