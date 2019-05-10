using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectUidExecutor : RequestExecutorContainer
  {
    ///  <summary>
    ///  Processes the get project Uid request and finds the Uid of the project corresponding to the given location and devices Customer and relavant subscriptions.
    ///  </summary>
    ///  <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectUidResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectUidRequest;
      string projectUid = string.Empty;
      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();

      // get the owningCustomer of the device-Asset
      var assetDevice =
        await dataRepository.LoadAssetDevice(request.radioSerial, ((DeviceTypeEnum) request.deviceType).ToString());

      // If fails on SNM940 try as again SNM941 
      if (assetDevice == null && (DeviceTypeEnum) request.deviceType == DeviceTypeEnum.SNM940)
      {
        log.LogDebug($"{nameof(ProjectUidExecutor)}: Failed for SNM940 trying again as Device Type SNM941");
        assetDevice = await dataRepository.LoadAssetDevice(request.radioSerial, DeviceTypeEnum.SNM941.ToString());
      }

      if (assetDevice == null)
      {
        log.LogDebug($"{nameof(ProjectUidExecutor)}: Unable to find device-asset association.");
        return ProjectUidHelper.FormatResult(projectUid, 33);
      }

      log.LogDebug($"{nameof(ProjectUidExecutor)}: Loaded assetDevice {JsonConvert.SerializeObject(assetDevice)}");

      // get the owningCustomer 3dpm subscriptions
      var assetSubs = await dataRepository.LoadAssetSubs(assetDevice.AssetUID, request.timeOfPosition);
      log.LogDebug($"{nameof(ProjectUidExecutor)}: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");

      //  standard 2d / 3d project aka construction project
      //    must have valid assetID, which must have a 3d sub.
      if (assetSubs != null && assetSubs.Any())
      {
        var standardProjects = await dataRepository.GetStandardProject(assetDevice.OwningCustomerUID, request.latitude,
          request.longitude, request.timeOfPosition);
        var standardProjectsList = standardProjects as IList<Project.Abstractions.Models.DatabaseModels.Project> ?? standardProjects.ToList();
        if (standardProjectsList.Any())
        {
          potentialProjects.AddRange(standardProjectsList);
          log.LogDebug($"{nameof(ProjectUidExecutor)}: Loaded standardProjects which lat/long is within {JsonConvert.SerializeObject(standardProjectsList)}");
        }
        else
        {
          log.LogDebug($"{nameof(ProjectUidExecutor)}: No standardProjects loaded");
        }
      }

      // ProjectMonitoring project
      //  assetCustomer must have a PM sub
      var pmProjects = await dataRepository.GetProjectMonitoringProject(assetDevice.OwningCustomerUID,
        request.latitude, request.longitude, request.timeOfPosition,
        (int) ProjectType.ProjectMonitoring, (int) ServiceTypeEnum.ProjectMonitoring);
      var pmProjectsList = pmProjects as IList<Project.Abstractions.Models.DatabaseModels.Project> ?? pmProjects.ToList();
      if (pmProjectsList.Any())
      {
        potentialProjects.AddRange(pmProjectsList);
        log.LogDebug($"{nameof(ProjectUidExecutor)}: Loaded pmProjects which lat/long is within {JsonConvert.SerializeObject(pmProjectsList)}");
      }
      else
      {
        log.LogDebug($"{nameof(ProjectUidExecutor)}: No pmProjects loaded");
      }

      // Landfill project
      //   assetCustomer must have a Landfill sub
      var landfillProjects = await dataRepository.GetProjectMonitoringProject(assetDevice.OwningCustomerUID,
        request.latitude, request.longitude, request.timeOfPosition,
        (int) ProjectType.LandFill, (int) ServiceTypeEnum.Landfill);
      var landfillProjectsList = landfillProjects as IList<Project.Abstractions.Models.DatabaseModels.Project> ?? landfillProjects.ToList();
      if (landfillProjectsList.Any())
      {
        potentialProjects.AddRange(landfillProjectsList);
        log.LogDebug($"{nameof(ProjectUidExecutor)}: Loaded landfillProjects which lat/long is within {JsonConvert.SerializeObject(landfillProjectsList)}");
      }
      else
      {
        log.LogDebug($"{nameof(ProjectUidExecutor)}: No landfillProjects loaded");
      }

      int uniqueCode = 0;
      var projects = potentialProjects.ToList();
      switch (projects.Count)
      {
        case 0:
          uniqueCode = 29;
          break;
        case 1:
          uniqueCode = 0;
          projectUid = projects[0].ProjectUID;
          break;
        default:
          uniqueCode = 32;
          break;
      }

      log.LogDebug($"{nameof(ProjectUidExecutor)}: returning uniqueCode: {uniqueCode} projectUid {projectUid}.");
      return ProjectUidHelper.FormatResult(projectUid, uniqueCode);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
