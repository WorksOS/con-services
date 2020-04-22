using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Executors;
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
    [Route("api/v1/device/applicationcontext/serialnumber")]
    [HttpGet]
    public async Task<DeviceDataSingleResult> GetDeviceBySerialNumber([FromQuery]  string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      
      var deviceSerial = new DeviceSerial(serialNumber);
      deviceSerial.Validate();

      var deviceDataResult = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetDeviceBySerialExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders, 
            deviceRepo: DeviceRepo, cwsDeviceClient: CwsDeviceClient)
          .ProcessAsync(deviceSerial)) as DeviceDataSingleResult
      );

      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [Route("api/v1/device/applicationcontext/shortRaptorAssetId")]
    [HttpGet]
    public async Task<DeviceDataResult> GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation($"{nameof(GetDevice)}");
      // CCSSSCON-207 executor and validation
      var deviceFromRepo = await DeviceRepo.GetDevice(shortRaptorAssetId); 
      
      var deviceResponseModel = await cwsDeviceClient.GetDeviceByDeviceUid(new Guid(deviceFromRepo.DeviceUID), customHeaders);
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      
      var deviceDataResult = new DeviceDataResult()
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
    [Route("api/v1/device/applicationcontext/{deviceUid}/projects")]
    [HttpGet]
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)}");

      // CCSSSCON-207 executor and validation
      var projectsFromCws = await cwsDeviceClient.GetProjectsForDevice(new Guid(deviceUid), customHeaders);
      if (projectsFromCws == null)
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
    [HttpPost("api/v1/devices/applicationcontext/deviceuids")]
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
    [HttpPost("api/v1/devices/applicationcontext/shortRaptorAssetIds")]
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

