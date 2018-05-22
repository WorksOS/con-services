using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
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
      IEnumerable<Project> potentialProjects = new List<Project>();

      log.LogDebug($"ProjectUidExecutor: Going to process request {JsonConvert.SerializeObject(request)}" );

      // get the owningCustomer of the device-Asset
      var assetDevice =
        await dataRepository.LoadAssetDevice(request.radioSerial, ((DeviceTypeEnum)request.deviceType).ToString());

      // If fails on SNM940 try as again SNM941 
      if (assetDevice == null && (DeviceTypeEnum)request.deviceType == DeviceTypeEnum.SNM940)
      {
        log.LogDebug("ProjectUidExecutor: Failed for SNM940 trying again as Device Type SNM941");
        assetDevice = await dataRepository.LoadAssetDevice(request.radioSerial, DeviceTypeEnum.SNM941.ToString());
      }
      if (assetDevice == null)
      {
        log.LogDebug("ProjectUidExecutor: Unable to find device-asset association.");
        return GetProjectUidResult.CreateGetProjectUidResult(projectUid, 33);
      }

      log.LogDebug($"ProjectUidExecutor: Loaded assetDevice {JsonConvert.SerializeObject(assetDevice)}");

      // get the owningCustomer 3dpm subscriptions
      var assetSubs = await dataRepository.LoadAssetSubs(assetDevice.AssetUID, request.timeOfPosition);
      log.LogDebug($"ProjectUidExecutor: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");

      //  standard 2d / 3d project aka construction project
      //    must have valid assetID, which must have a 3d sub.
      List<Project> enumerable;
      if (assetSubs != null && assetSubs.Any())
      {
        var standardProjects = await dataRepository.GetStandardProject(assetDevice.OwningCustomerUID, request.latitude,
          request.longitude, request.timeOfPosition);
        enumerable = standardProjects.ToList();
        if (standardProjects != null && enumerable.Any())
        {
          potentialProjects = potentialProjects.Concat(enumerable);
          log.LogDebug($"ProjectUidExecutor: Loaded standardProjects which lat/long is within {JsonConvert.SerializeObject(enumerable)}");
        }
        else
        {
          log.LogDebug("ProjectUidExecutor: No standardProjects loaded");
        }
      }

      // ProjectMonitoring project
      //  assetCustomer must have a PM sub
      var pmProjects = await dataRepository.GetProjectMonitoringProject(assetDevice.OwningCustomerUID,
        request.latitude, request.longitude, request.timeOfPosition,
        (int)ProjectType.ProjectMonitoring,
        (serviceTypeMappings.serviceTypes.Find(st => st.name == "Project Monitoring").NGEnum));
      enumerable = pmProjects.ToList();
      if (pmProjects != null && enumerable.Any())
      {
        potentialProjects = potentialProjects.Concat(enumerable);
        log.LogDebug($"ProjectUidExecutor: Loaded pmProjects which lat/long is within {JsonConvert.SerializeObject(enumerable)}");
      }
      else
      {
        log.LogDebug("ProjectUidExecutor: No pmProjects loaded");
      }

      // Landfill project
      //   assetCustomer must have a Landfill sub
      var landfillProjects = await dataRepository.GetProjectMonitoringProject(assetDevice.OwningCustomerUID,
        request.latitude, request.longitude, request.timeOfPosition,
        (int)ProjectType.LandFill, (serviceTypeMappings.serviceTypes.Find(st => st.name == "Landfill").NGEnum));
      enumerable = landfillProjects.ToList();
      if (landfillProjects != null && enumerable.Any())
      {
        potentialProjects = potentialProjects.Concat(enumerable);
        log.LogDebug($"ProjectUidExecutor: Loaded landfillProjects which lat/long is within { JsonConvert.SerializeObject(enumerable)}");
      }
      else
      {
        log.LogDebug("ProjectUidExecutor: No landfillProjects loaded");
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

      log.LogDebug($"ProjectUidExecutor: returning uniqueCode: {uniqueCode} projectUid {projectUid}.");

      try
      {
        return GetProjectUidResult.CreateGetProjectUidResult(projectUid, uniqueCode);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          GetProjectUidResult.CreateGetProjectUidResult(projectUid, 35));
      }
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
