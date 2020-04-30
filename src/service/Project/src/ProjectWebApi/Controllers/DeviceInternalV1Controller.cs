using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
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
  public class DeviceInternalV1Controller : ProjectBaseController
  {
    private readonly ICwsDeviceClient cwsDeviceClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeviceInternalV1Controller(ICwsDeviceClient cwsDeviceClient)
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
    [HttpGet("internal/v1/device/serialnumber")]
    public async Task<DeviceDataSingleResult> GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      
      var deviceSerial = new DeviceSerial(serialNumber);
      deviceSerial.Validate();

      var deviceDataResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetDeviceBySerialExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            headers: customHeaders, 
            deviceRepo: DeviceRepo, cwsDeviceClient: CwsDeviceClient)
          .ProcessAsync(deviceSerial)) as DeviceDataSingleResult;

      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [HttpGet("internal/v1/device/shortRaptorAssetId")]
    public async Task<DeviceDataSingleResult> GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation(nameof(GetDevice));
      // CCSSSCON-207 executor and validation
      var deviceFromRepo = await DeviceRepo.GetDevice(shortRaptorAssetId);

      var deviceResponseModel = await cwsDeviceClient.GetDeviceByDeviceUid(new Guid(deviceFromRepo.DeviceUID), customHeaders);
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
    [HttpGet("internal/v1/device/{deviceUid}/projects")]
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)}");

      // CCSSSCON-207 executor and validation
      var projectsFromCws = await cwsDeviceClient.GetProjectsForDevice(new Guid(deviceUid), customHeaders);
      if (projectsFromCws == null)
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
    /// Gets accounts with their association (if any) to the device
    ///    todoMaverick this is a temporary kludge until cws/profileX get around a bug in profileX
    /// </summary>
    [HttpGet("internal/v1/device/{deviceUid}/accounts")]
    public async Task<DeviceCustomerListDataResult> GetAccountsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetAccountsForDevice)}");
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets account with its association to the device
    ///    this will be the final fix, once cws/profileX makes this possibel
    /// </summary>
    [HttpGet("internal/v1/device/{deviceUid}/account")]
    public async Task<DeviceCustomerListDataResult> GetAccountForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetAccountForDevice)}");
      throw new NotImplementedException();
    }  
  }
}
