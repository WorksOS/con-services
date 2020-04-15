using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
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

    private readonly ICwsDeviceClient cwsDeviceClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeviceV1Controller(IConfigurationStore configStore, ICwsDeviceClient cwsDeviceClient)
      : base(configStore)
    {
      this.cwsDeviceClient = cwsDeviceClient;
    }

    /// <summary>
    /// Gets device by serialNumber
    ///  called by TFA AssetIdExecutor
    ///   a) retrieve from cws using serialNumber which must get AccountTRN and DeviceTRN
    ///   b) get from localDB shortRaptorAssetId so we can fill it into response
    ///     note that if it doesn't exist localDB it means that 
    ///     the user hasn't logged in to fill in our DB after adding the device to the account
    /// </summary>
    [Route("api/v1/device/serialnumber")]
    [HttpGet]
    public async Task<DeviceDataSingleResult> GetDeviceBySerialNumber([FromQuery]  string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)}");
      // todoMaverick executor and validation
      var deviceResponseModel = await cwsDeviceClient.GetDeviceBySerialNumber(serialNumber);
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      var deviceFromRepo = await DeviceRepo.GetDevice(deviceResponseModel.Id);

      var deviceDataResult = new DeviceDataSingleResult()
      {
        DeviceDescriptor = new DeviceData()
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
    [Route("api/v1/device/shortRaptorAssetId")]
    [HttpGet]
    public async Task<DeviceDataSingleResult> GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation($"{nameof(GetDevice)}");
      // todoMaverick executor and validation
      var deviceFromRepo = await DeviceRepo.GetDevice(shortRaptorAssetId); 
      
      var deviceResponseModel = await cwsDeviceClient.GetDeviceByDeviceUid(new Guid(deviceFromRepo.DeviceUID));
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      
      var deviceDataResult = new DeviceDataSingleResult()
      {
        DeviceDescriptor = new DeviceData()
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
    [Route("api/v1/device/{deviceUid}/projects")]
    [HttpGet]
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)}");

      // todoMaverick executor and validation
      var projectsFromCws = await cwsDeviceClient.GetProjectsForDevice(new Guid(deviceUid));
      if (cwsDeviceClient == null)
        throw new NotImplementedException();

      var projectDataResult = new ProjectDataResult();
      foreach(var projectCws in projectsFromCws.Projects)
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
      };      

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
      var assetIdDisplay = string.Join(", ", shortRaptorAssetIds ?? new List<long>());
      Logger.LogInformation($"{nameof(GetMatchingDevices)} Getting Devices for shortRaptorAssetIds: {assetIdDisplay}");

      var devices = await DeviceRepo.GetDevices(shortRaptorAssetIds);
      return Json(DeviceMatchingModel.FromDeviceList(devices));
    }

  }
}

