using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestUtility;
using VSS.Device.Data;
using VSS.Project.Data;
using VSS.Project.Service.Repositories;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{

  public class AssetIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get asset request and finds the id of the asset corresponding to the given tagfile radio serial number.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetAssetIdResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetAssetIdRequest request = item as GetAssetIdRequest;

      long legacyAssetId = -1;
      int serviceType = 0;
      bool result = false;

      // legacyProjectId can exist with and without a radioSerial so set this up early
      if (request.projectId > 0)
      {
        LoadProjectAndProjectBasedSubs(request.projectId);
        
        if (project != null)
        {
          LoadManual3DCustomerBasedSubs(project.CustomerUID);
          log.LogDebug("AssetIdExecutor: Retrieved Project CustomerSubs {0}", JsonConvert.SerializeObject(customerSubs));
        }
      }


      //Special case: Allow manual import of tag file if user has manual 3D subscription.
      //ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
      //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
      if (string.IsNullOrEmpty(request.radioSerial) || request.deviceType == (int)DeviceTypeEnum.MANUALDEVICE)
      {
        //Check for manual 3D subscription for the projects customer, Only allowed to process tag file if legacyProjectId is > 0.
        //If ok then set asset Id to -1 so Raptor knows it's a John Doe machine and set serviceType machineLevel to 18 "Manual 3D PM"
        if (project != null)
        {
          log.LogDebug("AssetIdExecutor: Going to check CustomerSubs. No radioSerial provided. projectId {0}", request.projectId);
          CheckForManual3DCustomerBasedSub(request.projectId, out legacyAssetId, out serviceType);
        }
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        LoadAssetDevice(request.radioSerial, ((DeviceTypeEnum)request.deviceType).ToString());

        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;

          // check subs
          LoadAssetSubs(assetDevice.AssetUID);

          // todo OwningCustomerUID should always be present, but bug in MD airlift, most are missing.
          LoadManual3DCustomerBasedSubs(assetDevice.OwningCustomerUID);
          log.LogDebug("AssetIdExecutor: Retrieved Asset CustomerSubs {0} for OwningCustomerUID {1}", JsonConvert.SerializeObject(customerSubs), assetDevice.OwningCustomerUID);

          serviceType = GetMostSignificantServiceType(assetDevice.AssetUID, (project != null ? project.ProjectUID : null));
        }
        else
        {
          log.LogDebug("AssetIdExecutor: Going to check CustomerSubs. No AssetDevice found. projectId {0}", request.projectId);
          CheckForManual3DCustomerBasedSub(request.projectId, out legacyAssetId, out serviceType);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));
      log.LogDebug("AssetIdExecutor: All done. result {0} legacyAssetId {1} serviceType {2}", result, legacyAssetId, serviceType);

      try
      {
        return GetAssetIdResult.CreateGetAssetIdResult(result, legacyAssetId, serviceType);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get legacy asset id"));
      }

    }


    #region caching
    private class SubscriptionData
    {
      public string assetUId { get; set; }
      public string projectUid { get; set; }
      public string customerUid { get; set; }
      public int serviceTypeId;
      public int startKeyDate { get; set; }
      public int endKeyDate { get; set; }


      public SubscriptionData(string assetUId, string projectUid, string customerUid, int serviceTypeID, DateTime? startKeyDate, DateTime? endKeyDate)
      {
        this.assetUId = assetUId;
        this.projectUid = projectUid;
        this.customerUid = customerUid;
        this.serviceTypeId = serviceTypeID;
        this.startKeyDate = startKeyDate == null ? DateTimeExtensions.KeyDate(DateTime.MinValue) : DateTimeExtensions.KeyDate(startKeyDate.Value);
        this.endKeyDate = endKeyDate == null ? DateTimeExtensions.NullKeyDate : DateTimeExtensions.KeyDate(endKeyDate.Value);

      }
    }

    Project.Data.Models.Project project = null;
    AssetDeviceIds.Data.ExtendedModels.AssetDeviceIds assetDevice = null;
    IEnumerable<SubscriptionData> projectSubs = null;
    IEnumerable<SubscriptionData> customerSubs = null;
    IEnumerable<SubscriptionData> assetSubs = null;

    // todo
    //private static TimeSpan cacheLife = new TimeSpan(10, 0);
    //private static MemoryCache projectCache = null;
    //private static MemoryCache projectBasedSubscriptionCache = null;
    //private static MemoryCache customerBasedSubscriptionCache = null;
    //private static MemoryCache assetBasedSubscriptionCache = null;


    #endregion caching


    private void LoadProjectAndProjectBasedSubs(long legacyProjectId)
    {
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectAndSubscriptions(legacyProjectId, DateTime.UtcNow.Date);

        if (p.Result != null && p.Result.ToList().Count() > 0)
        {
          project = p.Result.ToList()[0];
          log.LogDebug("AssetIdExecutor: Loaded project {0}", JsonConvert.SerializeObject(project));

          // now get any project-based subs Landfill (23--> 19) and ProjectMonitoring (24 --> 20)
          // todo I don't believe these are used in CG and won't be here
          projectSubs = p.Result.ToList()
            .Select(x => new SubscriptionData("", x.ProjectUID, x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
          log.LogDebug("AssetIdExecutor: Loaded projectSubs {0}", JsonConvert.SerializeObject(projectSubs));
        }
      }
    }

    private void LoadAssetDevice(string radioSerial, string deviceType)
    {
      // todo cache and use repoFactory when complete
      if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
      {
        var deviceRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = deviceRepo.GetAssociatedAsset(radioSerial, deviceType);
        assetDevice = a.Result;
      }      log.LogDebug("AssetIdExecutor: Loaded AssetDevice {0}", JsonConvert.SerializeObject(assetDevice));

    }

    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    private void LoadManual3DCustomerBasedSubs(string customerUid)
    {
      if (!string.IsNullOrEmpty(customerUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date);
        customerSubs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.Manual3DProjectMonitoring)
           .Select(x => new SubscriptionData("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
      }
    }


    // asset:3dProjMon (16 --> 13) 
    //  todo waiting for AssetSubs to be implemented in MDConsumer
    private void LoadAssetSubs(string assetUid)
    {
      if (!string.IsNullOrEmpty(assetUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        //var s = subsRepo.GetSubscriptionsByAsset(assetUid, DateTime.UtcNow.Date);
        //assetSubs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.e3DProjectMonitoring)
        //  .Select(x => new SubscriptionData(x.AssetUID, "", x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
      }
      log.LogDebug("AssetIdExecutor: NOT IMPLEMENTED AssetSubs {0}", JsonConvert.SerializeObject(assetSubs));
    }

    // these are CustomerBased and no legacyAssetID will be returned
    private void CheckForManual3DCustomerBasedSub(long legacyProjectId, out long legacyAssetId, out int serviceType)
    {
      legacyAssetId = -1;
      serviceType = (int)ServiceTypeEnumNG.Unknown;

      if (legacyProjectId > 0)
      {
        log.LogDebug("AssetIdExecutor: project ID non-zero so manual import for project - about to check for manual 3D subscription. legacyProjectId {0}", legacyProjectId);
      
        if (customerSubs != null && customerSubs.Count() > 0)
        {
          log.LogDebug("AssetIdExecutor: found {0} manual 3D for the project customer", JsonConvert.SerializeObject(customerSubs));
          legacyAssetId = -1;   //Raptor needs to know it's a John Doe machine i.e. not a VL asset
          serviceType = (int)ServiceTypeEnumCG.Manual3DProjectMonitoring;
        }
      }
    }

    private int GetMostSignificantServiceType(string assetUID, string projectUID)
    {
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset ID {0} and project ID {1}", assetUID, projectUID);

      ServiceTypeEnumNG serviceType = ServiceTypeEnumNG.Unknown;

      IEnumerable<SubscriptionData> subs = new List<SubscriptionData>();
      if (customerSubs != null) subs = customerSubs;
      if (assetSubs != null)
      {
        if (subs == null)
          subs = assetSubs;
        else
          subs.Concat(assetSubs);
      }
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() subs being checked {0}", JsonConvert.SerializeObject(subs));

      if (subs.Count() > 0)
      {
        //Look for highest level machine subscription which is current
        int utcNowKeyDate = DateTime.UtcNow.KeyDate();
        foreach (SubscriptionData sub in subs)
        {
          switch ((ServiceTypeEnumNG)sub.serviceTypeId)
          {
            // Manual3d is least significant
            case ServiceTypeEnumNG.Manual3DProjectMonitoring:
              if (serviceType != ServiceTypeEnumNG.e3DProjectMonitoring)
                serviceType = ServiceTypeEnumNG.Manual3DProjectMonitoring;
              break;

            // 3D PM is most significant
            // if 3D asset-based, the assets customer must be the same as the Projects customer 
            case ServiceTypeEnumNG.e3DProjectMonitoring:
              if (serviceType != ServiceTypeEnumNG.e3DProjectMonitoring)
              {
                //Allow manual tag file import for customer who has the 3D subscription for the asset
                //and allow automatic tag file processing in all cases (can't tell customer for automatic)

                //log.IfDebugFormat("GetProjectServiceType found ServiceTypeEnum.e3DProjectMonitoring for asset ID {0}", assetID);
                if (sub.customerUid == project.CustomerUID)
                {
                  serviceType = ServiceTypeEnumNG.e3DProjectMonitoring;
                }
              }
              break;
            default:
              break;
          }
        }
      }

      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset ID {0} and project ID {1}, returning serviceTypeNG {2} actually serviceTypeCG {3}", assetUID, projectUID, serviceType, ConvertServiceTypeNGtoCG(serviceType));
      return (int)(ConvertServiceTypeNGtoCG(serviceType));
    }

    private ServiceTypeEnumCG ConvertServiceTypeNGtoCG(ServiceTypeEnumNG serviceTypeNG)
    {
      return (ServiceTypeEnumCG)ServiceTypeEnumCG.Parse(typeof(ServiceTypeEnumCG), serviceTypeNG.ToString());
    }
  }
}
    