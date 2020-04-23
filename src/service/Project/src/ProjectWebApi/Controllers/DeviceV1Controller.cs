using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Internal;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Customer controller v1
  ///     for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class DeviceV1Controller : ProjectBaseController
  {
    private readonly ICwsDeviceClient _cwsDeviceClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeviceV1Controller(ICwsDeviceClient cwsDeviceClient)
    {
      this._cwsDeviceClient = cwsDeviceClient;
    }

    /// <summary>
    /// Gets device by serialNumber
    ///  called by TFA AssetIdExecutor
    ///   a) retrieve from cws using serialNumber which must get AccountTRN and DeviceTRN
    ///   b) get from localDB shortRaptorAssetId so we can fill it into response
    ///     note that if it doesn't exist localDB it means that 
    ///     the user hasn't logged in to fill in our DB after adding the device to the account
    /// </summary>
    [HttpGet("api/v1/device/serialnumber")]
    public async Task<DeviceDataSingleResult> GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation(nameof(GetDeviceBySerialNumber));
      // CCSSSCON-207 executor and validation
      var deviceResponseModel = await _cwsDeviceClient.GetDeviceBySerialNumber(serialNumber);
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      var deviceFromRepo = await DeviceRepo.GetDevice(deviceResponseModel.Id);

      var deviceDataResult = new DeviceDataSingleResult
      {
        DeviceDescriptor = new DeviceData
        {
          CustomerUID = deviceResponseModel.AccountId,
          DeviceUID = deviceResponseModel.Id,
          DeviceName = deviceResponseModel.DeviceName,
          SerialNumber = deviceResponseModel.SerialNumber,
          Status = deviceResponseModel.Status,
          ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetID
        }
      };

      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [HttpGet("api/v1/device/shortRaptorAssetId")]
    public async Task<DeviceDataSingleResult> GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation(nameof(GetDevice));
      // CCSSSCON-207 executor and validation
      var deviceFromRepo = await DeviceRepo.GetDevice(shortRaptorAssetId);

      var deviceResponseModel = await _cwsDeviceClient.GetDeviceByDeviceUid(new Guid(deviceFromRepo.DeviceUID));
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      var deviceDataResult = new DeviceDataSingleResult
      {
        DeviceDescriptor = new DeviceData
        {
          CustomerUID = deviceResponseModel.AccountId,
          DeviceUID = deviceResponseModel.Id,
          DeviceName = deviceResponseModel.DeviceName,
          SerialNumber = deviceResponseModel.SerialNumber,
          Status = deviceResponseModel.Status,
          ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetID
        }
      };

      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [HttpGet("api/v1/device/{deviceUid}/projects")]
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation(nameof(GetProjectsForDevice));

      // CCSSSCON-207 executor and validation
      var projectsFromCws = await _cwsDeviceClient.GetProjectsForDevice(new Guid(deviceUid));
      if (_cwsDeviceClient == null)
        throw new NotImplementedException();

      var projectDataResult = new ProjectDataResult();
      foreach (var projectCws in projectsFromCws.Projects)
      {
        var project = await ProjectRepo.GetProject(projectCws.projectId);

        //// use WorksOS data rather than cws as it is the source of truth
        if (project != null)
        {
          if (string.Compare(project.CustomerUID, projectCws.accountId, true) != 0)
            Logger.LogError($"{nameof(GetProjectsForDevice)} project account differs between WorksOS and WorksManager: projectId: {project.ProjectUID} WorksOS customer: {project.CustomerUID}  CWS account: {projectCws.accountId}");
          else
            projectDataResult.ProjectDescriptors.Add(AutoMapperUtility.Automapper.Map<ProjectData>(project));
        }
      }

      return projectDataResult;
    }

    /// <summary>
    /// Get a list of device Uid/Id matches for Uids supplied
    /// </summary>
    [HttpPost("api/v1/devices/deviceuids")]
    [ProducesResponseType(typeof(DeviceMatchingModel), 200)]
    public async Task<IActionResult> GetMatchingDevices([FromBody] List<Guid> deviceUids)
    {
      var deviceUidDisplay = string.Join(", ", deviceUids ?? new List<Guid>());
      Logger.LogInformation($"{nameof(GetMatchingDevices)} Getting Devices for deviceUids: {deviceUidDisplay}");

      var devices = await DeviceRepo.GetDevices(deviceUids);
      return Json(DeviceMatchingModel.FromDeviceList(devices));
    }

    /// <summary>
    /// Get a list of device Uid/Id matches for Ids supplied
    /// </summary>
    [HttpPost("api/v1/devices/shortRaptorAssetIds")]
    [ProducesResponseType(typeof(List<DeviceMatchingModel>), 200)]
    public async Task<IActionResult> GetMatchingDevices([FromBody] List<long> shortRaptorAssetIds)
    {
      var deviceIdsDisplay = string.Join(", ", shortRaptorAssetIds ?? new List<long>());
      Logger.LogInformation($"{nameof(GetMatchingDevices)} Getting Devices for shortRaptorAssetIds: {deviceIdsDisplay}");

      var devices = await DeviceRepo.GetDevices(shortRaptorAssetIds);
      return Json(DeviceMatchingModel.FromDeviceList(devices));
    }

    /// <summary>
    /// Get location data for a given set of Devices.
    /// </summary>
    [HttpPost("api/v1/devices/location")]
    public IActionResult GetDeviceLocationData([FromBody] List<Guid> deviceIds)
    {
      var deviceIdsDisplay = string.Join(", ", deviceIds ?? new List<Guid>());
      Logger.LogInformation($"{nameof(GetMatchingDevices)} Getting Device location data for: {deviceIdsDisplay}");

      var assets = MockDeviceRepository.GetAssets(deviceIds);

      var resultSet = new List<AssetLocationData>(assets.Count);

      foreach (var asset in assets)
      {
        resultSet.Add(new AssetLocationData
        {
          AssetUid = Guid.Parse(asset.AssetUID),
          AssetIdentifier = asset.EquipmentVIN,
          AssetSerialNumber = asset.SerialNumber,
          AssetType = asset.AssetType,
          LocationLastUpdatedUtc = asset.LastActionedUtc,
          MachineName = asset.Name,
          Latitude = 0,
          Longitude = 0,
        });
      }

      Logger.LogInformation($"Returning location data for {resultSet.Count} Assets.");
      return Json(resultSet);
    }
  }
}
