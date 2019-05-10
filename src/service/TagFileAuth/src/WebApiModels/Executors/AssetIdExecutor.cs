using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

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
      long legacyAssetId = -1;
      var serviceType = 0;
      var result = false;

      Project.Abstractions.Models.DatabaseModels.Project project = null;
      IEnumerable<Subscriptions> projectCustomerSubs = new List<Subscriptions>();
      IEnumerable<Subscriptions> assetCustomerSubs = new List<Subscriptions>();
      IEnumerable<Subscriptions> assetSubs = new List<Subscriptions>();

      // legacyProjectId can exist with and without a radioSerial so set this up early
      if (request.projectId > 0)
      {
        project = await dataRepository.LoadProject(request.projectId);
        log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          projectCustomerSubs =
            await dataRepository.LoadManual3DCustomerBasedSubs(project.CustomerUID, DateTime.UtcNow);
          log.LogDebug(
            $"{nameof(AssetIdExecutor)}: Loaded projectsCustomerSubs? {JsonConvert.SerializeObject(projectCustomerSubs)}");
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
          log.LogDebug($"{nameof(AssetIdExecutor)}: Failed for SNM940. Trying again as Device Type SNM941");
          assetDevice = await dataRepository.LoadAssetDevice(request.radioSerial, DeviceTypeEnum.SNM941.ToString());
        }

        log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded assetDevice? {JsonConvert.SerializeObject(assetDevice)}");

        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;
          assetSubs = await dataRepository.LoadAssetSubs(assetDevice.AssetUID, DateTime.UtcNow);
          log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");

          // should this append to any assetCustomerSubs which may have come from Project.CustomerUID above?
          assetCustomerSubs =
            await dataRepository.LoadManual3DCustomerBasedSubs(assetDevice.OwningCustomerUID, DateTime.UtcNow);
          log.LogDebug(
            $"{nameof(AssetIdExecutor)}: Loaded assetsCustomerSubs? {JsonConvert.SerializeObject(assetCustomerSubs)}");

          serviceType = GetMostSignificantServiceType(assetDevice.AssetUID, project, projectCustomerSubs,
            assetCustomerSubs, assetSubs);
        }
        else
        {
          CheckForManual3DCustomerBasedSub(request.projectId, projectCustomerSubs, out legacyAssetId, out serviceType);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));
      return GetAssetIdResult.CreateGetAssetIdResult(result, legacyAssetId, serviceType);
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
      log.LogDebug(
        $"{nameof(CheckForManual3DCustomerBasedSub)}: legacyProjectId {legacyProjectId} serviceType {serviceType}");

      if (legacyProjectId > 0)
      {
        if (projectCustomerSubs != null && projectCustomerSubs.Any())
        {
          legacyAssetId = -1; //Raptor needs to know it's a John Doe machine i.e. not a VL asset
          serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").CGEnum;
        }
      }

      log.LogDebug(
        $"{nameof(CheckForManual3DCustomerBasedSub)}: CheckForManual3DCustomerBasedSub(). legacyAssetId {legacyAssetId} serviceType {serviceType}");
    }

    private int GetMostSignificantServiceType(string assetUid,
      Project.Abstractions.Models.DatabaseModels.Project project,
      IEnumerable<Subscriptions> projectCustomerSubs, IEnumerable<Subscriptions> assetCustomerSubs,
      IEnumerable<Subscriptions> assetSubs)
    {
      log.LogDebug($"{nameof(GetMostSignificantServiceType)}: asset UID {assetUid}");

      var serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum;

      IEnumerable<Subscriptions> subs = new List<Subscriptions>();
      if (projectCustomerSubs != null)
      {
        var projectCustomerSubscriptions = projectCustomerSubs.ToList();
        if (projectCustomerSubscriptions.Any()) subs = subs.Concat(projectCustomerSubscriptions.Select(s => s));
      }

      if (assetCustomerSubs != null)
      {
        var assetCustomerSubscriptions = assetCustomerSubs.ToList();
        if (assetCustomerSubscriptions.Any()) subs = subs.Concat(assetCustomerSubscriptions.Select(s => s));
      }

      if (assetSubs != null)
      {
        var assetSubscriptions = assetSubs.ToList();
        if (assetSubscriptions.Any()) subs = subs.Concat(assetSubscriptions.Select(s => s));
      }

      log.LogDebug($"{nameof(GetMostSignificantServiceType)}: subs being checked {JsonConvert.SerializeObject(subs)}");

      var subscriptions = subs.ToList();
      if (subscriptions.Any())
      {
        //Look for highest level machine subscription which is current
        foreach (var sub in subscriptions)
        {
          // Manual3d is least significant
          if (sub.serviceTypeId == (int) ServiceTypeEnum.Manual3DProjectMonitoring)
          {
            if (serviceType != (int) ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              log.LogDebug(
                $"{nameof(GetMostSignificantServiceType)}: found ServiceTypeEnum.Manual3DProjectMonitoring for asset UID {assetUid}");
              serviceType = (int) ServiceTypeEnum.Manual3DProjectMonitoring;
            }
          }

          // 3D PM is most significant
          // if 3D asset-based, the assets customer must be the same as the Projects customer 
          if (sub.serviceTypeId == (int) ServiceTypeEnum.ThreeDProjectMonitoring)
          {
            if (serviceType != (int) ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              //Allow manual tag file import for customer who has the 3D subscription for the asset
              //and allow automatic tag file processing in all cases (can't tell customer for automatic)
              log.LogDebug(
                $"{nameof(GetMostSignificantServiceType)}: found ServiceTypeEnum.e3DProjectMonitoring for asset {assetUid} sub.customerUid {sub.customerUid}");
              if (project == null || sub.customerUid == project.CustomerUID)
              {
                serviceType = (int) ServiceTypeEnum.ThreeDProjectMonitoring;
                break;
              }
            }
          }
        }
      }

      var cGenServiceTypeId = serviceTypeMappings.serviceTypes.Find(st => st.NGEnum == serviceType).CGEnum;
      log.LogDebug(
        $"{nameof(GetMostSignificantServiceType)}: for asset {assetUid} , returning serviceTypeNG {serviceType} actually serviceTypeCG (i.e Raptor) {cGenServiceTypeId}");

      return cGenServiceTypeId;
    }
  }
}
