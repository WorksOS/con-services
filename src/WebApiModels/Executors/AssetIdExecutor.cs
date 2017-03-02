using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
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
        LoadProject(request.projectId);
        
        if (project != null)
        {
          // not used in getAssetID? LoadProjectAndProjectBasedSubs(request.projectId);
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


    private void CheckForManual3DCustomerBasedSub(long legacyProjectId, out long legacyAssetId, out int serviceType)
    {
      // these are CustomerBased and no legacyAssetID will be returned
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
    