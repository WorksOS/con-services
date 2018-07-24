using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;
using VSS.MasterData.Repositories.DBModels;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets a legacyAssetId and/or serviceType for the requested radioSerial and/or legacyProjectId.
  /// </summary>
  public class AssetIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get asset request and finds the id of the asset corresponding to the given tagfile radio serial number.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetAssetIdResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetAssetIdRequest;
      log.LogDebug("AssetIdExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      long legacyAssetId = -1;
      var serviceType = 0;
      var result = false;

      Project project = null;
      IEnumerable<Subscriptions> projectCustomerSubs = new List<Subscriptions>();
      IEnumerable<Subscriptions> assetCustomerSubs = new List<Subscriptions>();
      IEnumerable<Subscriptions> assetSubs = new List<Subscriptions>(); 

      // legacyProjectId can exist with and without a radioSerial so set this up early
      if (request.projectId > 0)
      {
        project = await dataRepository.LoadProject(request.projectId);
        log.LogDebug("AssetIdExecutor: Loaded project? {0}", JsonConvert.SerializeObject(project));

        if (project != null)
        {
          projectCustomerSubs = await dataRepository.LoadManual3DCustomerBasedSubs(project.CustomerUID, DateTime.UtcNow);
          log.LogDebug("AssetIdExecutor: Loaded projectsCustomerSubs? {0}", JsonConvert.SerializeObject(projectCustomerSubs));
        }
      }


      //Special case: Allow manual import of tag file if user has manual 3D subscription.
      //ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
      //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
      if (string.IsNullOrEmpty(request.radioSerial) || request.deviceType == (int) DeviceTypeEnum.MANUALDEVICE)
      {
        //Check for manual 3D subscription for the projects customer, Only allowed to process tag file if legacyProjectId is > 0.
        //If ok then set asset Id to -1 so Raptor knows it's a John Doe machine and set serviceType machineLevel to 18 "Manual 3D PM"
        if (project != null)
        {
          CheckForManual3DCustomerBasedSub(request.projectId, projectCustomerSubs, out legacyAssetId, out serviceType);
        }
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        var assetDevice =
          await dataRepository.LoadAssetDevice(request.radioSerial, ((DeviceTypeEnum) request.deviceType).ToString());

        // special case in CGen US36833 If fails on DT SNM940 try as again SNM941 
        if (assetDevice == null && (DeviceTypeEnum) request.deviceType == DeviceTypeEnum.SNM940)
        {
          log.LogDebug("AssetIdExecutor: Failed for SNM940 trying again as Device Type SNM941");
          assetDevice = await dataRepository.LoadAssetDevice(request.radioSerial, DeviceTypeEnum.SNM941.ToString());
        }
        log.LogDebug("AssetIdExecutor: Loaded assetDevice? {0}", JsonConvert.SerializeObject(assetDevice));

        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;
          assetSubs = await dataRepository.LoadAssetSubs(assetDevice.AssetUID, DateTime.UtcNow);
          log.LogDebug("AssetIdExecutor: Loaded assetSubs? {0}", JsonConvert.SerializeObject(assetSubs));

          // should this append to any assetCustomerSubs which may have come from Project.CustomerUID above?
          assetCustomerSubs = await dataRepository.LoadManual3DCustomerBasedSubs(assetDevice.OwningCustomerUID, DateTime.UtcNow);
          log.LogDebug("AssetIdExecutor: Loaded assetsCustomerSubs? {0}", JsonConvert.SerializeObject(assetCustomerSubs));

          serviceType = GetMostSignificantServiceType(assetDevice.AssetUID, project, projectCustomerSubs, assetCustomerSubs, assetSubs);
          log.LogDebug(
            "AssetIdExecutor: after GetMostSignificantServiceType(). AssetUID {0} project{1} custSubs {2} assetSubs {3}",
            assetDevice.AssetUID, JsonConvert.SerializeObject(project), JsonConvert.SerializeObject(projectCustomerSubs),
            JsonConvert.SerializeObject(assetSubs));
        }
        else
        {
          CheckForManual3DCustomerBasedSub(request.projectId, projectCustomerSubs, out legacyAssetId, out serviceType);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));
      log.LogDebug("AssetIdExecutor: All done. result {0} legacyAssetId {1} serviceType {2}", result, legacyAssetId,
        serviceType);

      try
      {
        return GetAssetIdResult.CreateGetAssetIdResult(result, legacyAssetId, serviceType);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, 
            ContractExecutionStatesEnum.InternalProcessingError, 15));
      }
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }


    private void CheckForManual3DCustomerBasedSub(long legacyProjectId,
              IEnumerable<Subscriptions> projectCustomerSubs, 
              out long legacyAssetId, out int serviceType)
    {
      // these are CustomerBased and no legacyAssetID will be returned
      legacyAssetId = -1;
      serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").CGEnum;
      log.LogDebug("AssetIdExecutor: CheckForManual3DCustomerBasedSub(). projectId {0} custSubs {1}", legacyProjectId, JsonConvert.SerializeObject(projectCustomerSubs));

      if (legacyProjectId > 0)
      {
        log.LogDebug("AssetIdExecutor: project ID non-zero so manual import for project - about to check for manual 3D subscription. legacyProjectId {0}", legacyProjectId);

        if (projectCustomerSubs != null && projectCustomerSubs.Any())
        {
          legacyAssetId = -1;   //Raptor needs to know it's a John Doe machine i.e. not a VL asset
          serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").CGEnum;
        }
      }
      log.LogDebug("AssetIdExecutor: CheckForManual3DCustomerBasedSub(). legacyAssetId {0} serviceType {1}", legacyAssetId, serviceType);
    }

    private int GetMostSignificantServiceType(string assetUID, Project project,
      IEnumerable<Subscriptions> projectCustomerSubs, IEnumerable<Subscriptions> assetCustomerSubs, IEnumerable<Subscriptions> assetSubs)
    {
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset UID {0} and project UID {1}", assetUID, JsonConvert.SerializeObject(project));

      var serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum;

      IEnumerable<Subscriptions> subs = new List<Subscriptions>();
      if (projectCustomerSubs != null && projectCustomerSubs.Any()) subs = subs.Concat(projectCustomerSubs.Select(s => s));
      if (assetCustomerSubs != null && assetCustomerSubs.Any()) subs = subs.Concat(assetCustomerSubs.Select(s => s));
      if (assetSubs != null && assetSubs.Any()) subs = subs.Concat(assetSubs.Select(s => s));

      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() subs being checked {0}", JsonConvert.SerializeObject(subs));

      if (subs.Any())
      {
        //Look for highest level machine subscription which is current
        foreach (var sub in subs)
        {
          // Manual3d is least significant
          if (sub.serviceTypeId == serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").NGEnum)
          {
            if (serviceType != serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
            {
              log.LogDebug("AssetIdExecutor: GetProjectServiceType found ServiceTypeEnum.Manual3DProjectMonitoring for asset UID {0}", assetUID);
              serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").NGEnum;
            }
          }
          // 3D PM is most significant
          // if 3D asset-based, the assets customer must be the same as the Projects customer 
          if (sub.serviceTypeId == serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
          {
            if (serviceType != serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
            {
              //Allow manual tag file import for customer who has the 3D subscription for the asset
              //and allow automatic tag file processing in all cases (can't tell customer for automatic)
              log.LogDebug($"AssetIdExecutor: GetProjectServiceType found ServiceTypeEnum.e3DProjectMonitoring for asset UID {assetUID} sub.customerUid {sub.customerUid}" );
              if (project == null || sub.customerUid == project.CustomerUID)
              {
                serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum;
                break; 
              }
            }
          }
        }
      }

      var CGenServiceTypeID = serviceTypeMappings.serviceTypes.Find(st => st.NGEnum == serviceType).CGEnum;
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset ID {0}, returning serviceTypeNG {1} actually serviceTypeCG {2}", assetUID, serviceType, CGenServiceTypeID);
      return CGenServiceTypeID;
    }
  }
}
