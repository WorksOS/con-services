using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Nighthawk.MasterDataSync.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM;
using System.Collections.Generic;


namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class AssetSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly Uri AssetApiEndPointUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;

    public AssetSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;      

      if (string.IsNullOrWhiteSpace(configurationManager.GetAppSetting("AssetService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Asset api URL value cannot be empty");

      AssetApiEndPointUri = new Uri(_configurationManager.GetAppSetting("AssetService.WebAPIURI") + "/asset");
    }

    public override bool Process(ref bool isServiceStopped)
    {
      bool isDataProcessed = false;
      if (LockTaskState(_taskName, _taskTimeOutInterval))
      {
        isDataProcessed = ProcessSync(ref isServiceStopped);
        UnLockTaskState(_taskName);
      }
      return isDataProcessed;
    }

    public override bool ProcessSync(ref bool isServiceStopped)
    {
      //MasterData Insertion
      var lastProcessedId = GetLastProcessedId(_taskName);
      var lastInsertUtc = GetLastInsertUTC(_taskName);
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;

      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, lastInsertUtc, saveLastUpdateUtcFlag, ref isServiceStopped);

      //MasterData Updation
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastInsertUtc, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    //Any assets insertutc value is greater than the bookmark value,then it considered for createassetevent
    private bool ProcessInsertionRecords(long? lastProcessedId, DateTime? lastInsertUtc, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;

          Log.IfInfo(string.Format(
            "Started Processing CreateAssetEvent. LastProcessedId : {0} , LastInsertedUTC : {1}", lastProcessedId,
            lastInsertUtc));

          TaskState customerTaskState = (from m in opCtx.MasterDataSyncReadOnly
                                         where m.TaskName == StringConstants.CustomerTask
                                         select new TaskState() { lastProcessedId = m.LastProcessedID ?? Int32.MinValue, InsertUtc = m.LastInsertedUTC }).FirstOrDefault();
          if (customerTaskState != null)
          {
            var assetsList = (from a in opCtx.AssetReadOnly
              join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
              where ((a.InsertUTC == lastInsertUtc && a.AssetID > lastProcessedId) || a.InsertUTC > lastInsertUtc) &&
                    a.UpdateUTC <= currentUtc
              join c in opCtx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID into CustomerSS
							from ct in CustomerSS.DefaultIfEmpty()
              join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on ct.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
              from crt in customerRelationshipSubset.DefaultIfEmpty()
              join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
              from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
							let cId = ct == null ? 0 : (ct.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? (crct.ID == null ? 0 : crct.ID) : ct.ID)
              orderby a.InsertUTC, a.AssetID
              select new
              {
                a.AssetUID,
                a.AssetID,
                a.Name,
                a.SerialNumberVIN,
                a.fk_MakeCode,
                a.Model,
                a.ProductFamilyName,
                OwningCustomerUID = ct == null ? null : (ct.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : ct.CustomerUID),
                CustomerID = cId > customerTaskState.lastProcessedId ? -1 : cId,
                a.IconID,
                a.EquipmentVIN,
                a.ManufactureYear,
                d.DeviceUID,
                d.OwnerBSSID,
                a.InsertUTC
              }).Take(BatchSize).ToList();


            if (assetsList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for creation event", _taskName));
              return false;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey,StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var asset in assetsList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (asset.CustomerID == -1)
              {
                Log.IfInfo("The required CustomerUID's CreateEvent has not been processed yet..");
                return true;
              }

              var createAsset = new CreateAssetEvent
              {
                AssetUID = (Guid)asset.AssetUID,
                LegacyAssetID = asset.AssetID,
                AssetName = asset.Name,
                SerialNumber = asset.SerialNumberVIN,
                MakeCode = asset.fk_MakeCode,
                Model = asset.Model,
                OwningCustomerUID = asset.OwningCustomerUID,
                AssetType = asset.ProductFamilyName,
                IconKey = asset.IconID,
                EquipmentVIN = asset.EquipmentVIN,
                ModelYear = asset.ManufactureYear,
                ActionUTC = currentUtc
              };

              var svcResponseForAssetCreation = ProcessServiceRequestAndResponse(createAsset, _httpRequestWrapper, AssetApiEndPointUri, requestHeader, HttpMethod.Post);
              Log.IfInfo("Create asset "+asset.AssetUID + " returned " + svcResponseForAssetCreation.StatusCode);
              switch (svcResponseForAssetCreation.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = asset.AssetID;
                  lastInsertUtc = asset.InsertUTC;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponseForAssetCreation = ProcessServiceRequestAndResponse(createAsset, _httpRequestWrapper, AssetApiEndPointUri, requestHeader, HttpMethod.Post);
                  if (svcResponseForAssetCreation.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = asset.AssetID;
                    lastInsertUtc = asset.InsertUTC;
                  }
                  break;
                case HttpStatusCode.Conflict:
                  var assetUid = svcResponseForAssetCreation.Content.ReadAsStringAsync().Result;
                  Guid assetGuid;
                  if (Guid.TryParse(assetUid, out assetGuid))
                  {
                    opCtx.Asset.First(x => x.AssetID == asset.AssetID).AssetUID = assetGuid;
                    string mk_Sno = string.Concat(createAsset.MakeCode, "_", createAsset.SerialNumber);
                    var assetRef = opCtx.AssetReference.Where(x => x.Value == mk_Sno).ToList();
                    if (assetRef.Any())
                    {
                      Log.InfoFormat("updating asset Reference with Value {0} with AssetUID {1}", mk_Sno, assetUid);
                      assetRef.ForEach(e => e.UID = assetGuid);
                    }
                    opCtx.SaveChanges();
                    Log.InfoFormat("updating asset with id {0} with AssetUID {1}", asset.AssetID, assetUid);
                  }
                  else
                  {
                    Log.WarnFormat("Duplicate create asset. No asset uid was returned / Incorrect AssetUID {0}",assetUid);
                  }
                  lastProcessedId = asset.AssetID;
                  lastInsertUtc = asset.InsertUTC;
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createAsset) + "\n ");
                  lastProcessedId = asset.AssetID;
                  lastInsertUtc = asset.InsertUTC;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas Device service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. ID = {1}", svcResponseForAssetCreation.StatusCode, asset.AssetID));
                  return true;
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Creation Event {1} \n {2}", _taskName, e.Message,
            e.StackTrace));
        }
        finally
        {

          //Saving last update utc if it is not set
          if (saveLastUpdateUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastProcessedId != Int32.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastInsertUtc;
            opCtx.SaveChanges();
            Log.IfInfo(
              string.Format("Completed Processing CreateAssetEvent. LastProcessedId : {0} , LastInsertedUTC : {1}",
                lastProcessedId, lastInsertUtc));
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastInsertUtc, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing UpdateAssetEvent. LastProcessedId : {0} , LastInsertedUTC : {1},LastUpdatedUTC : {2}", lastProcessedId, lastInsertUtc, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;

          var assets = (from a in opCtx.AssetReadOnly
                        join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                        join customer in opCtx.CustomerReadOnly on d.OwnerBSSID equals customer.BSSID
                        join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on customer.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
                        from crt in customerRelationshipSubset.DefaultIfEmpty()
                        join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
                        from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
                        where
                        ((a.InsertUTC == lastInsertUtc && a.AssetID <= lastProcessedId) || a.InsertUTC < lastInsertUtc)
                          && a.UpdateUTC <= currentUtc && a.UpdateUTC > lastUpdateUtc
                        orderby a.InsertUTC, a.AssetID
                        select new {
                          a.AssetUID,
                          a.AssetID,
                          a.Name,
                          a.SerialNumberVIN,
                          a.fk_MakeCode,
                          a.Model,
                          a.ProductFamilyName,
                          OwningCustomerUID = customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : customer.CustomerUID,
                          a.IconID,
                          a.EquipmentVIN,
                          a.ManufactureYear,
                          d.DeviceUID,
                          d.OwnerBSSID,
                          a.InsertUTC,
                          a.UpdateUTC
                        }).Take(BatchSize).ToList();

          if (assets.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for updation", currentUtc, _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var asset in assets.OrderBy(a => a.UpdateUTC).ThenBy(a => a.AssetID))
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            var updateAsset = new UpdateAssetEvent
            {
              AssetUID = (Guid)asset.AssetUID,
              OwningCustomerUID = asset.OwningCustomerUID,
              AssetName = asset.Name,
              LegacyAssetID = asset.AssetID,
              Model = asset.Model,
              AssetType = asset.ProductFamilyName,
              IconKey = asset.IconID,
              EquipmentVIN = asset.EquipmentVIN,
              ModelYear = asset.ManufactureYear,
              ActionUTC = asset.UpdateUTC
            };

            var svcResponse = ProcessServiceRequestAndResponse(updateAsset, _httpRequestWrapper, AssetApiEndPointUri,
              requestHeader, HttpMethod.Put);
            Log.IfInfo("Update asset "+asset.AssetUID + " returned " + svcResponse.StatusCode);

            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = asset.UpdateUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(updateAsset, _httpRequestWrapper, AssetApiEndPointUri,
                  requestHeader, HttpMethod.Put);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = asset.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateAsset));
                lastUpdateUtc = asset.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Asset service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data Payload : {1}.", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateAsset)));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} updation event {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing UpdateAssetEvent. LastProcessedId : {0} , LastInsertUTC : {1} LastUpdateUTC : {2}", lastProcessedId, lastInsertUtc, lastUpdateUtc));
        }
      }
      return true;
    }
  }
}
