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
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Nighthawk.MasterDataSync.Interfaces;


namespace VSS.Nighthawk.MasterDataSync.Implementation
{
    public class AssetSyncProcessor : SyncProcessorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Uri _assetApiEndPointUri;
        private readonly Uri _assetOwnerDetailsApiEndPointUri;
        private readonly Uri _associateDeviceApiEndPointUri;
        private readonly string _taskName;
        private readonly IHttpRequestWrapper _httpRequestWrapper;
        private List<KeyValuePair<string, string>> _requestHeaders;

        public AssetSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
          : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
        {
            _taskName = taskName;
            _httpRequestWrapper = httpRequestWrapper;

            if (string.IsNullOrWhiteSpace(configurationManager.GetAppSetting("AssetService.WebAPIURI")))
                throw new ArgumentNullException("Uri", "Asset api URL value cannot be empty");
            {
                _assetApiEndPointUri = new Uri(ConfigurationManager.GetAppSetting("AssetService.WebAPIURI") + "/asset");
                _assetOwnerDetailsApiEndPointUri = new Uri(ConfigurationManager.GetAppSetting("AssetService.WebAPIURI") + "/assetownerdetails");
                _associateDeviceApiEndPointUri = new Uri(ConfigurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/associatedeviceasset");
            }
        }

        public override bool Process(ref bool isServiceStopped)
        {
            bool isDataProcessed = false;
            if (LockTaskState(_taskName, TaskTimeOutInterval))
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
            var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastInsertUtc.GetValueOrDefault(), lastUpdateUtc, ref isServiceStopped);
            return (isCreateEventProcessed || isUpdateEventProcessed);
        }

        //Any assets insertutc value is greater than the bookmark value,then it considered for createassetevent
        private bool ProcessInsertionRecords(long? lastProcessedId, DateTime? lastInsertUtc, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
        {
            var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds);
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    lastProcessedId = lastProcessedId ?? Int32.MinValue; 

                    Log.IfInfo($"Started Processing CreateAssetEvent. LastProcessedId : {lastProcessedId} , LastInsertedUTC : {lastInsertUtc}");

                    // Publish CreateAsset, CreateWorkDefinition, AssociateDeviceAsset and AssetOwner events.
                    var assetList = (from a in opCtx.AssetReadOnly
                            join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                    where ((a.InsertUTC == lastInsertUtc && a.AssetID > lastProcessedId) || a.InsertUTC > lastInsertUtc) && a.InsertUTC <= currentUtc
                            join dass in opCtx.CustomerReadOnly on d.OwnerBSSID equals dass.BSSID into dealerOrAccountSubSet
                                from dealerOrAccount in dealerOrAccountSubSet.DefaultIfEmpty()
                            join crss in opCtx.CustomerRelationshipReadOnly.Where(e =>e.fk_CustomerRelationshipTypeID == (int) CustomerRelationshipTypeEnum.TCSCustomer) 
                                                 on dealerOrAccount.ID equals crss.fk_ClientCustomerID into customerRelationSubset
                                from cr in customerRelationSubset.DefaultIfEmpty()
                                    join css in opCtx.CustomerReadOnly on cr.fk_ParentCustomerID equals css.ID into customerSubset
                                        from customer in customerSubset.DefaultIfEmpty()
                            join drss in opCtx.CustomerRelationshipReadOnly.Where(e =>e.fk_CustomerRelationshipTypeID == (int) CustomerRelationshipTypeEnum.TCSDealer) 
                                                 on dealerOrAccount.ID equals drss.fk_ClientCustomerID into dealerRelationSubset
                                from dr in dealerRelationSubset.DefaultIfEmpty()
                                    join dss in opCtx.CustomerReadOnly on dr.fk_ParentCustomerID equals dss.ID into dealerSubset
                                        from dealer in dealerSubset.DefaultIfEmpty()
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
								OwningCustomerUID = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account?(customer == null ? dealer.CustomerUID: customer.CustomerUID) : dealer.CustomerUID),
                                CustomerUID = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? ((customer == null) ? null : customer.CustomerUID) : null,
                                CustomerName = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? ((customer == null) ? null : customer.Name) : null,
                                AccountUID = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.CustomerUID : null,
                                AccountName = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.Name : null,
                                NetworkCustomerCode = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.NetworkCustomerCode : null,
                                DealerAccountCode = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.DealerAccountCode : null,
                                DealerUID = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? dealerOrAccount.CustomerUID : ((dealer == null) ? Guid.Empty : dealer.CustomerUID),
                                DealerName = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? dealerOrAccount.Name : ((dealer == null) ? null : dealer.Name),
                                NetworkDealerCode = dealerOrAccount == null ? null : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? dealerOrAccount.NetworkDealerCode : ((dealer == null) ? null : dealer.NetworkDealerCode),
                                a.IconID,
                                a.EquipmentVIN,
                                a.ManufactureYear,
                                d.DeviceUID,
                                d.OwnerBSSID,
                                a.InsertUTC
                            }
                        ).Take(BatchSize).ToList();

                        if (assetList.Count < 1)
                        {
                            Log.IfInfo($"No {_taskName} data left for creation event.");
                            return true;
                        }

                        _requestHeaders = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

                        if (_requestHeaders.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey,StringConstants.InvalidValue)))
                        {
                            return true;
                        }

                        foreach (var asset in assetList)
                        {
                            if (isServiceStopped)
                            {
                                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                                break;
                            }

                            var  assetEvent = new CreateAssetEvent
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
                                ActionUTC = DateTime.UtcNow
                            };
                            var svcResponse = ProcessServiceRequestAndResponse<CreateAssetEvent>(assetEvent, _httpRequestWrapper, _assetApiEndPointUri, _requestHeaders, HttpMethod.Post);
                            
                            if (svcResponse.StatusCode == HttpStatusCode.Conflict)  // Update AssetUID in Store
                                UpdateAssetUid(opCtx,asset.AssetID,asset.fk_MakeCode,asset.SerialNumberVIN,svcResponse.Content.ReadAsStringAsync().Result);
                            if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.Conflict)
                            {
                                lastProcessedId = asset.AssetID;
                                lastInsertUtc = asset.InsertUTC;
                            }
                            else
                            {
                                Log.IfError($"Asset Create event failed for {asset.AssetID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(assetEvent)}");
                                break;
                            }

                            var associateDeviceAssetEvent = new AssociateDeviceAssetEvent
                            {
                                DeviceUID = (Guid)asset.DeviceUID,
                                AssetUID = (Guid)asset.AssetUID,
                                ActionUTC = DateTime.UtcNow
                            };

                            svcResponse = ProcessServiceRequestAndResponse<AssociateDeviceAssetEvent>(associateDeviceAssetEvent, _httpRequestWrapper, _associateDeviceApiEndPointUri, _requestHeaders, HttpMethod.Post);
                            if (svcResponse.StatusCode != HttpStatusCode.OK)
                            {
                            Log.Info($"AssciateDeviceAssetEvent failed with Status Code : {svcResponse.StatusCode} {svcResponse.Content.ReadAsStringAsync().Result}");
                            }

                            var assetOwnerEvent = new AssetOwnerEvent
                            {
                                AssetUID = (Guid)asset.AssetUID,
                                AssetOwnerRecord =
                                    new AssetOwner
                                    {
                                        CustomerName = asset.CustomerName,
                                        AccountName = asset.AccountName,
                                        AccountUID = asset.AccountUID.HasValue ? (Guid)asset.AccountUID : (Guid?)null,
                                        DealerAccountCode = asset.DealerAccountCode,
                                        DealerUID = asset.DealerUID.HasValue ? (Guid)asset.DealerUID : Guid.Empty,
                                        DealerName = asset.DealerName,
                                        NetworkDealerCode = asset.NetworkDealerCode,
                                        NetworkCustomerCode = asset.NetworkCustomerCode,
                                        CustomerUID = asset.CustomerUID.HasValue ? (Guid)asset.CustomerUID : (Guid?)null,
                                    },
                                Action = "Create",
                                ActionUTC = DateTime.UtcNow
                            };
                            svcResponse = ProcessServiceRequestAndResponse<AssetOwnerEvent>(assetOwnerEvent, _httpRequestWrapper, _assetOwnerDetailsApiEndPointUri, _requestHeaders, HttpMethod.Post);

                            if (svcResponse.StatusCode != HttpStatusCode.OK)
                            {
                                Log.IfError($"AssetOwnerEvent failed with Status Code : {svcResponse.StatusCode}, payload : {JsonHelper.SerializeObjectToJson(assetOwnerEvent)}");
                            }

                    }  // for loop
                }
                catch (Exception e)
                {
                    Log.IfError($"Exception in processing {_taskName} Creation Event {e.Message} \n {e.StackTrace}");
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
                        Log.IfInfo($"Completed Processing CreateAssetEvent. LastProcessedId : {lastProcessedId} , LastInsertedUTC : {lastInsertUtc}");
                    }
                    else
                    {
                        Log.IfInfo(string.Format("No Records Processed "));
                    }
                }
            }
            return true;
        }

        private void UpdateAssetUid(INH_OP ctx, long legacyAssetId, string makeCode, string serialNumber,string newAssetUid)
        {
            if (Guid.TryParse(newAssetUid, out var assetGuid))
            {
                ctx.Asset.First(x => x.AssetID == legacyAssetId).AssetUID = assetGuid;
                var mkSno = string.Concat(makeCode, "_", serialNumber);
                var assetRef = ctx.AssetReference.Where(x => x.Value == mkSno).ToList();
                if (assetRef.Any())
                {
                    Log.InfoFormat($"updating asset Reference with Value {mkSno} with AssetUID {newAssetUid}");
                    assetRef.ForEach(e => e.UID = assetGuid);
                }

                ctx.SaveChanges();
                Log.InfoFormat($"Updating legacy asset ID: {legacyAssetId} with new Asset UID {newAssetUid}");
            }
            else
            {
                Log.WarnFormat($"Duplicate create asset. No asset uid was returned / Incorrect AssetUID {newAssetUid}");
            }
        }

        private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastInsertUtc, DateTime? lastUpdateUtc, ref bool isServiceStopped)
        {
            Log.IfInfo($"Started Processing UpdateAssetEvent. LastProcessedId : {lastProcessedId} , LastInsertedUTC : {lastInsertUtc},LastUpdatedUTC : {lastUpdateUtc}");
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
					var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds);

                    var assets = (from a in opCtx.AssetReadOnly
                                  join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                  join c in opCtx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID into CustomerSS
                                  from ct in CustomerSS.DefaultIfEmpty()
                                  join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on ct.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
                                  from crt in customerRelationshipSubset.DefaultIfEmpty()
                                  join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
                                  from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
								  join drss in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer)
										    on ct.ID equals drss.fk_ClientCustomerID into dealerRelationSubset
								  from dr in dealerRelationSubset.DefaultIfEmpty()
								  join dss in opCtx.CustomerReadOnly on dr.fk_ParentCustomerID equals dss.ID into dealerSubset
								  from dealer in dealerSubset.DefaultIfEmpty()
								  where
                                  ((a.InsertUTC == lastInsertUtc && a.AssetID <= lastProcessedId) || a.InsertUTC < lastInsertUtc)
                                    && a.UpdateUTC <= currentUtc && a.UpdateUTC > lastUpdateUtc
                                  orderby a.UpdateUTC, a.AssetID
                                  select new
                                  {
                                      a.AssetUID,
                                      a.AssetID,
                                      a.Name,
                                      a.SerialNumberVIN,
                                      a.fk_MakeCode,
                                      a.Model,
                                      a.ProductFamilyName,
                                      OwningCustomerUID = ct == null ? null : (ct.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? (crct == null ? dealer.CustomerUID : crct.CustomerUID) : dealer.CustomerUID),
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
                        Log.IfInfo($"Current UTC : {currentUtc} Updated, No {_taskName} data to left for updation");
                        return false;
                    }

                    var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

                    if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
                    {
                        return true;
                    }

                    foreach (var asset in assets)
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
                            ActionUTC = DateTime.UtcNow
                        };

                        var svcResponse = ProcessServiceRequestAndResponse<UpdateAssetEvent>(updateAsset, _httpRequestWrapper, _assetApiEndPointUri, _requestHeaders, HttpMethod.Put);

                        if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.BadRequest)
                        {
                            lastUpdateUtc = asset.UpdateUTC;
                        }
                        else
                        {
                            Log.IfError($"Asset Update event failed for {asset.AssetID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateAsset)}");
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.IfError($"Exception in processing {_taskName} updation event {e.Message} \n { e.StackTrace}");
                }
                finally
                {
                    //Update the last read utc to masterdatasync
                    opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
                    opCtx.SaveChanges();
                    Log.IfInfo($"Completed Processing UpdateAssetEvent. LastProcessedId : {lastProcessedId} , LastInsertUTC : {lastInsertUtc} LastUpdateUTC : {lastUpdateUtc}");
                }
            }
            return true;
        }
    }
}
