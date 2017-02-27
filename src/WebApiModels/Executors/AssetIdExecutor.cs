using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestUtility;
using VSS.Device.Data;
using VSS.Project.Data;
using VSS.Project.Service.Repositories;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.Models;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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

      if (request.projectId > 0)
      {
        LoadProjectAndProjectBasedSubs(request.projectId);

        if (project != null)
          LoadManual3DCustomerBasedSubs(project.CustomerUID);
      }


      //Special case: Allow manual import of tag file if user has manual 3D subscription.
      //ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
      //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.

      if (string.IsNullOrEmpty(request.radioSerial) || request.deviceType == (int)DeviceTypeEnum.MANUALDEVICE)
      {
        //Check for manual 3D subscription for customer, Only allowed to process tag file if project Id is > 0.
        //If ok then set asset Id to -1 so Raptor knows it's a John Doe machine and set serviceType machineLevel  to 15 
        if (project != null)
        {
          CheckForManual3DCustomerBasedSub(request.projectId, out legacyAssetId, out serviceType);
        }
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        // todo cache asset
        // todo use repo factory properly once interface available
        DeviceTypeEnum whatever = (DeviceTypeEnum)request.deviceType;

        var assetRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = assetRepo.GetAssociatedAsset(request.radioSerial, whatever.ToString()); //  (DeviceTypeEnum)request.deviceType);
        assetDevice = a.Result;
        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;

          // todo check subs
          //  LoadServiceViewCache(assetID);
          //  machineLevel = (int)GetProjectServiceType(assetID, projectID);
        }
        else
        {
          // todo check subs
          //  CheckForManual3D(projectID, out assetID, out machineLevel);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));

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

          // now get any project-based subs Landfill (23--> 19) and ProjectMonitoring (24 --> 20)
          projectSubs = p.Result.ToList()
            .Select(x => new SubscriptionData("", x.ProjectUID, x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
        }
      }
    }


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


    // 3dProjMon (16 --> 13)
    private static void LoadAssetBasedSubs(string assetUid)
    {
      //if (!string.IsNullOrEmpty(assetUid))
      //{
      //  var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
      //  var s = subsRepo.GetSubscriptionsByAsset(assetUid, DateTime.UtcNow.Date);
      //  assetSubs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.e3DProjectMonitoring)
      //    .Select(x => new SubscriptionData(x.AssetUID, "", x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
      //}
    }

    // these are CustomerBased and no legacyAssetID will be returned
    private void CheckForManual3DCustomerBasedSub(long projectId, out long legacyAssetId, out int serviceType)
    {
      legacyAssetId = -1;
      serviceType = (int)ServiceTypeEnumNG.Unknown;

      if (projectId > 0)
      {
        // log.IfDebug("OnGetAssetID: project ID non-zero so manual import for project - about to check for manual 3D subscription");

        if (customerSubs != null && customerSubs.Count() > 0)
        {
          // log.IfDebugFormat("OnGetAssetID: found {0} manual 3D subscriptions", manualViews.Count);

          legacyAssetId = -1;   //Raptor needs to know it's a John Doe machine i.e. not a VL asset
          serviceType = (int)ServiceTypeEnumCG.Manual3DProjectMonitoring;
        }
      }
    }
  }
}
    