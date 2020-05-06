﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the Device details from a) cws and b) localDB
  /// </summary>
  public class GetDeviceByShortRaptorIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var shortRaptorId = CastRequestObjectTo<ShortRaptorId>(item, errorCode: 68);

      try
      {
        var deviceFromRepo = await deviceRepo.GetDevice(shortRaptorId.ShortRaptorAssetId);
        if (deviceFromRepo == null)
        {
          var message = "Unable to locate device in localDB";
          log.LogInformation($"GetDeviceByShortRaptorIdExecutor: {message}");
          return new DeviceDataSingleResult(code: 100, message: message, new DeviceData());
        }

        var deviceData = new DeviceData() { DeviceUID = deviceFromRepo.DeviceUID, ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetID };
        
        var deviceResponseModel = await cwsDeviceClient.GetDeviceByDeviceUid(new Guid(deviceFromRepo.DeviceUID), customHeaders);
        if (deviceResponseModel == null)
        {
          var message = "Unable to locate device by serialNumber in cws";
          log.LogInformation($"GetDeviceByShortRaptorIdExecutor: {message}");
          return new DeviceDataSingleResult(code: 101, message: message, deviceData);
        }

        deviceData.DeviceName = deviceResponseModel.DeviceName;
        deviceData.SerialNumber = deviceResponseModel.SerialNumber;

        // now get the customerId and 2xstatus
        // Note that this step may not be needed in future if/when WM can return these fields in cwsDeviceClient.GetDeviceBySerialNumber()
        var deviceAccountListDataResult = await cwsDeviceClient.GetAccountsForDevice(new Guid(deviceData.DeviceUID), customHeaders);
        if (deviceAccountListDataResult?.Accounts == null || !deviceAccountListDataResult.Accounts.Any())
        {
          var message = "Unable to locate any account for the device in cws";
          log.LogInformation($"GetDeviceByShortRaptorIdExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDataSingleResult(code: 102, message: message, deviceData);
        }

        log.LogInformation($"GetDeviceByShortRaptorIdExecutor: deviceAccountListDataResult {JsonConvert.SerializeObject(deviceAccountListDataResult)}");
        if (deviceAccountListDataResult.Accounts
          .Count(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0) > 1)
        {
          var message = "There is >1 active account for the device in cws";
          log.LogInformation($"GetDeviceByShortRaptorIdExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDataSingleResult(code: 103, message: message, deviceData);
        }

        var deviceCustomer = deviceAccountListDataResult.Accounts
          .FirstOrDefault(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0);
        deviceData.CustomerUID = deviceCustomer.Id;
        deviceData.RelationStatus = deviceCustomer.RelationStatus;
        deviceData.TccDeviceStatus = deviceCustomer.TccDeviceStatus;
        log.LogInformation($"GetDeviceByShortRaptorIdExecutor: deviceData {JsonConvert.SerializeObject(deviceData)}");
        return new DeviceDataSingleResult(deviceData); 
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 104 - 2000, "getDeviceByShortRaptorIdExecutor", e.Message);
      }

      return null;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}