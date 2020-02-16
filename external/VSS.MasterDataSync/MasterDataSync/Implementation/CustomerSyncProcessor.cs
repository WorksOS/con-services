using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Nighthawk.MasterDataSync.Interfaces;
using VSS.Nighthawk.MasterDataSync.Models;


namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class CustomerSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri _customerApiEndPointUri;
    private readonly Uri _assetOwnerUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public CustomerSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("CustomerService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Customer api URL value cannot be empty");

      _customerApiEndPointUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI"));
      _assetOwnerUri = new Uri(ConfigurationManager.GetAppSetting("AssetService.WebAPIURI") + "/assetownerdetails");
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
        var currentUtc = DateTime.UtcNow;
        var lastProcessedId = GetLastProcessedId(_taskName);
      
        var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
        var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);

        //MasterData Updation
        //lastProcessedId = GetLastProcessedId(_taskName);
        var lastUpdateUtc = GetLastUpdateUTC(_taskName);
        var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, currentUtc,ref isServiceStopped);
        return (isCreateEventProcessed || isUpdateEventProcessed);
     }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
            var currentUtc = DateTime.UtcNow; //.AddSeconds(-SyncPrioritySeconds);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateCustomerEvent. LastProcessedId : {0}", lastProcessedId));
          var customerDataList = (from c in opCtx.CustomerReadOnly
                                  join ct in opCtx.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                                  join dn in opCtx.DealerNetworkReadOnly on c.fk_DealerNetworkID equals dn.ID
                                  where c.ID > lastProcessedId 
                                  orderby c.ID
                                  select new
                                  {
                                    c.ID,
                                    c.CustomerUID,
                                    c.Name,
                                    CustomerType = ct.Name,
                                    c.BSSID,
                                    DealerNetwork = dn.Name,
                                    c.NetworkDealerCode,
                                    c.NetworkCustomerCode,
                                    c.DealerAccountCode,
                                    c.PrimaryEmailContact,
                                    c.FirstName,
                                    c.LastName,
                                    c.IsActivated,
                                    UpdateUTC = currentUtc
                                  }).Take(BatchSize).ToList();

          if (customerDataList.Count < 1)
          {
            Log.IfInfo($"No {_taskName} data left for creation");
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);
					
          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var customerData in customerDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            var createCustomer = new CreateCustomerEvent
                                            {
                                              CustomerUID = (Guid)customerData.CustomerUID,
                                              CustomerName = customerData.Name,
                                              CustomerType = customerData.CustomerType,
                                              BSSID = customerData.BSSID,
                                              DealerNetwork = customerData.DealerNetwork,
                                              NetworkDealerCode = customerData.NetworkDealerCode,
                                              NetworkCustomerCode = customerData.NetworkCustomerCode,
                                              DealerAccountCode = customerData.DealerAccountCode,
                                              PrimaryContactEmail = customerData.PrimaryEmailContact,
                                              FirstName = customerData.FirstName,
                                              LastName = customerData.LastName,
                                              IsActive= customerData.IsActivated,
                                              ActionUTC = DateTime.UtcNow
                                            };
			Log.Info("Request Payload" + JsonHelper.SerializeObjectToJson(createCustomer));

                        var svcResponse = ProcessServiceRequestAndResponse<CreateCustomerEvent>(createCustomer, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Post);

                        if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.BadRequest)
                        {
                            lastProcessedId = customerData.ID;
                        }
                        else
                        {
                            Log.IfError($"Customer Create event failed for {createCustomer.CustomerUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(createCustomer)}");
                            break;
                        }

          }
        }
        catch (Exception e)
        {
          Log.IfError($"Exception in processing {_taskName} Creation {e.Message} \n {e.StackTrace}");
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
            opCtx.SaveChanges();
            Log.IfInfo($"Completed Processing CreateCustomerEvent. LastProcessedId : {lastProcessedId} "); 
          }
          else
          {
            Log.IfInfo("No Records Processed ");
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastUpdateUtc,DateTime currentUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing UpdateCustomerEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var customerDataList = (from c in opCtx.CustomerReadOnly
                                  join dn in opCtx.DealerNetworkReadOnly on c.fk_DealerNetworkID equals dn.ID
                                  where c.ID <= lastProcessedId && c.UpdateUTC <= currentUtc && c.UpdateUTC > lastUpdateUtc 
                                  orderby c.UpdateUTC, c.ID
                                  select new
                                  {
                                    c.ID,
                                    c.CustomerUID,
                                    c.Name,
                                    c.BSSID,
                                    DealerNetwork = dn.Name,
                                    c.fk_CustomerTypeID,
                                    c.NetworkDealerCode,
                                    c.NetworkCustomerCode,
                                    c.DealerAccountCode,
                                    c.PrimaryEmailContact,
                                    c.FirstName,
                                    c.LastName,
                                    c.UpdateUTC,
                                    c.IsActivated
                                  }).Take(BatchSize).ToList();


          if (customerDataList.Count() < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo($"Current UTC : {currentUtc} Updated, No {_taskName} data to left for Updation");
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

        foreach (var customerData in customerDataList)
        {
            if (isServiceStopped)
            {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
            }
            var updateCustomerEvent = new UpdateCustomerEvent
            {
                CustomerUID = (Guid)customerData.CustomerUID,
                CustomerName = customerData.Name,
                BSSID = customerData.BSSID,
                DealerNetwork = customerData.DealerNetwork,
                NetworkDealerCode = customerData.NetworkDealerCode,
                NetworkCustomerCode = customerData.NetworkCustomerCode,
                DealerAccountCode = customerData.DealerAccountCode,
                PrimaryContactEmail = customerData.PrimaryEmailContact,
                FirstName = customerData.FirstName,
                IsActive=customerData.IsActivated,
                LastName = customerData.LastName,
                ActionUTC = DateTime.UtcNow
            };

            var svcResponse = ProcessServiceRequestAndResponse<UpdateCustomerEvent>(updateCustomerEvent, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Put);

            if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                lastUpdateUtc = customerData.UpdateUTC;
            }
            else
            {
                Log.IfError($"Customer Update event failed for {updateCustomerEvent.CustomerUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateCustomerEvent)}");
                break;
            }
            switch (customerData.fk_CustomerTypeID)
            {
                case 0: // CustomerType.Dealer:
                    publishAssetownerEventForDealerAssets(opCtx, customerData.ID, requestHeader);
                    publishAssetownerEventForDealerDirectAssets(opCtx, customerData.ID, requestHeader);
                    break;
                case 1: // CustomerType.Customer:
                    publishAssetownerEventForCustomerAssets(opCtx, customerData.ID, requestHeader);
                    break;
                case 2:  // CustomerType.Account:
                    publishAssetownerEventForAccountAssets(opCtx, customerData.ID, requestHeader);
                    break;
                default:
                    break;
            }
        }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} updation {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
          opCtx.SaveChanges();
          Log.IfInfo($"Completed Processing UpdateCustomerEvent. LastProcessedId : {lastProcessedId} , LastUpdateUTC : {lastUpdateUtc}");
        }
      }
      return true;
    }
   
    private bool publishAssetownerEventForCustomerAssets(INH_OP ctx, long customerId, List<KeyValuePair<string,string>> requestHeader)
        {
            long lastProcessedAssetId = 0;
            bool processMoreRecords = false;
            do
            {
                var assetOwnerList = (
                                    from c in ctx.CustomerReadOnly
                                    join cr in ctx.CustomerRelationshipReadOnly on c.ID equals cr.fk_ParentCustomerID
                                    join ac in ctx.CustomerReadOnly on cr.fk_ClientCustomerID equals ac.ID
                                    join d in ctx.DeviceReadOnly on ac.BSSID equals d.OwnerBSSID
                                    join a in ctx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                    where c.ID == customerId && a.AssetID > lastProcessedAssetId && cr.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer
                                    // Dealer is Optional                
                                    join drss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer)
                                                           on cr.fk_ClientCustomerID equals drss.fk_ClientCustomerID into dealerRelationSubset
                                    from dr in dealerRelationSubset.DefaultIfEmpty()
                                    join dss in ctx.CustomerReadOnly on dr.fk_ParentCustomerID equals dss.ID into dealerSubset
                                    from dealer in dealerSubset.DefaultIfEmpty()
                                    orderby a.AssetID
                                    select new 
                                    {
                                        a.AssetID,
                                        AssetUID = (Guid)a.AssetUID,
                                        c.CustomerUID,
                                        CustomerName = c.Name,
                                        AccountUID = (Guid)ac.CustomerUID,
                                        AccountName = ac.Name,
                                        ac.NetworkCustomerCode,
                                        ac.DealerAccountCode,

                                        DealerUID = dealer == null ? Guid.Empty : (Guid)dealer.CustomerUID,
                                        DealerName = dealer == null ? null : dealer.Name,
                                        NetworkDealerCode = dealer == null ? null : dealer.NetworkDealerCode
                                    }
                            ).Take(BatchSize).ToList();

                processMoreRecords = assetOwnerList.Count() >= BatchSize;
                lastProcessedAssetId = publishAssetOwnerEvents(assetOwnerList, requestHeader);

            } while (processMoreRecords);

            return true;
        }

        private bool publishAssetownerEventForAccountAssets(INH_OP ctx, long customerId, List<KeyValuePair<string, string>> requestHeader)
        {
            long lastProcessedAssetId = 0;
            bool processMoreRecords = false;
            do
            {
                var assetOwnerList = (
                                from ac in ctx.CustomerReadOnly
                                join d in ctx.DeviceReadOnly on ac.BSSID equals d.OwnerBSSID
                                join a in ctx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                where ac.ID == customerId && a.AssetID > lastProcessedAssetId
                                join crss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer)
                                                on ac.ID equals crss.fk_ClientCustomerID into customerRelationSubset
                                from cr in customerRelationSubset.DefaultIfEmpty()
                                join css in ctx.CustomerReadOnly on cr.fk_ParentCustomerID equals css.ID into customerSubset
                                from customer in customerSubset.DefaultIfEmpty()
                                join drss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer)
                                                    on ac.ID equals drss.fk_ClientCustomerID into dealerRelationSubset
                                from dr in dealerRelationSubset.DefaultIfEmpty()
                                join dss in ctx.CustomerReadOnly on dr.fk_ParentCustomerID equals dss.ID into dealerSubset
                                from dealer in dealerSubset.DefaultIfEmpty()
                                orderby a.AssetID
                                select new {
                                                a.AssetID,
                                                AssetUID = (Guid)a.AssetUID,
                                                CustomerName = customer == null ? null : customer.Name,
                                                CustomerUID = customer == null ? (Guid?)null : (Guid)customer.CustomerUID,
                                                AccountName = ac.Name,
                                                AccountUID = ac.CustomerUID,
                                                ac.DealerAccountCode,
                                                ac.NetworkCustomerCode,
                                                DealerUID = dealer == null ? Guid.Empty : (Guid)dealer.CustomerUID,
                                                DealerName = dealer == null ? null : dealer.Name,
                                                NetworkDealerCode = dealer == null ? null : dealer.NetworkDealerCode,
                                            }
                        ).Take(BatchSize).ToList();

                processMoreRecords = assetOwnerList.Count() >= BatchSize;
                lastProcessedAssetId = publishAssetOwnerEvents(assetOwnerList, requestHeader);
            } while (processMoreRecords);
            return true;
        }
        private bool publishAssetownerEventForDealerAssets(INH_OP ctx, long customerId, List<KeyValuePair<string, string>> requestHeader)
        {
            long lastProcessedAssetId = 0;
            bool processMoreRecords = false;
            do
            {
                var assetOwnerList = (
                                from dealer in ctx.CustomerReadOnly
                                join cr in ctx.CustomerRelationshipReadOnly on dealer.ID equals cr.fk_ParentCustomerID
                                join ac in ctx.CustomerReadOnly on cr.fk_ClientCustomerID equals ac.ID
                                join d in ctx.DeviceReadOnly on ac.BSSID equals d.OwnerBSSID
                                join a in ctx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                where dealer.ID == customerId && a.AssetID > lastProcessedAssetId && cr.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer
                                // Customer is Optional    
                                join crss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer)
                                                on ac.ID equals crss.fk_ClientCustomerID into customerRelationSubset
                                from crs in customerRelationSubset.DefaultIfEmpty()
                                join css in ctx.CustomerReadOnly on crs.fk_ParentCustomerID equals css.ID into customerSubset
                                from customer in customerSubset.DefaultIfEmpty()
                                orderby a.AssetID
                                select new 
                                {
                                    a.AssetID,
                                    AssetUID = (Guid)a.AssetUID,
                                    DealerUID = (Guid)dealer.CustomerUID,
                                    DealerName =dealer.Name,
                                    dealer.NetworkDealerCode,

                                    CustomerUID = customer == null ? (Guid?)null : (Guid)customer.CustomerUID,
                                    CustomerName = customer == null ? null : customer.Name,

                                    AccountUID = (Guid)ac.CustomerUID,
                                    AccountName = ac.Name,
                                    ac.NetworkCustomerCode,
                                    ac.DealerAccountCode,
                                }
                        ).Take(BatchSize).ToList();

                processMoreRecords = assetOwnerList.Count() >= BatchSize;
                lastProcessedAssetId = publishAssetOwnerEvents(assetOwnerList, requestHeader);
            } while (processMoreRecords);
            return true;
        }
        private bool publishAssetownerEventForDealerDirectAssets(INH_OP ctx, long customerId, List<KeyValuePair<string, string>> requestHeader)
        {
            long lastProcessedAssetId = 0;
            bool processMoreRecords = false;
            do
            {
                var assetOwnerList = (
                                from dealer in ctx.CustomerReadOnly
                                join d in ctx.DeviceReadOnly on dealer.BSSID equals d.OwnerBSSID
                                join a in ctx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                where dealer.ID == customerId && a.AssetID > lastProcessedAssetId
                                orderby a.AssetID
                                select new 
                                {
                                    a.AssetID,
                                    AssetUID = (Guid)a.AssetUID,
                                    DealerUID = (Guid)dealer.CustomerUID,
                                    DealerName = dealer.Name,
                                    dealer.NetworkDealerCode,
                                    CustomerUID = (Guid?)null,
                                    CustomerName = (string)null,
                                    AccountUID = (Guid?)null,
                                    AccountName = (string)null,
                                    NetworkCustomerCode = (string)null,
                                    DealerAccountCode = (string)null
                                }
                        ).Take(BatchSize).ToList();

            processMoreRecords = assetOwnerList.Count() >= BatchSize;
            lastProcessedAssetId = publishAssetOwnerEvents(assetOwnerList, requestHeader);
        } while (processMoreRecords);
      return true;
    }
        private long publishAssetOwnerEvents(dynamic assetOwnerList, List<KeyValuePair<string, string>> requestHeader)
        {
            long lastProcessedAssetId = 0;
            foreach (var assetOwner in assetOwnerList)
            {
				var assetOwnerEvent = new AssetOwnerEvent
				{
					AssetUID = assetOwner.AssetUID,
					AssetOwnerRecord =
								new AssetOwner
								{
									CustomerUID = assetOwner.CustomerUID == null ? Guid.Empty : assetOwner.CustomerUID,
									CustomerName = string.IsNullOrEmpty(assetOwner.CustomerName) ? "" : assetOwner.CustomerName,
									AccountUID = assetOwner.AccountUID == null ? Guid.Empty : assetOwner.AccountUID,
									AccountName = string.IsNullOrEmpty(assetOwner.AccountName) ? "" : assetOwner.AccountName,
									NetworkCustomerCode = string.IsNullOrEmpty(assetOwner.NetworkCustomerCode) ? "" : assetOwner.NetworkCustomerCode,
									DealerAccountCode = string.IsNullOrEmpty(assetOwner.DealerAccountCode) ? "" : assetOwner.DealerAccountCode,
									DealerUID = assetOwner.DealerUID == null ? Guid.Empty : assetOwner.DealerUID,
									DealerName = string.IsNullOrEmpty(assetOwner.DealerName) ? "" : assetOwner.DealerName,
									NetworkDealerCode = string.IsNullOrEmpty(assetOwner.NetworkDealerCode) ? "": assetOwner.NetworkDealerCode
								},
                    Action = "Update",
                    ActionUTC = DateTime.UtcNow
                };

                var svcResponse = ProcessServiceRequestAndResponse<AssetOwnerEvent>(assetOwnerEvent, _httpRequestWrapper, _assetOwnerUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode != HttpStatusCode.OK)
                {
                    Log.IfError($"AssetOwner Update Event failed with Status Code : {svcResponse.StatusCode}, payload : {JsonHelper.SerializeObjectToJson(assetOwnerEvent)}");
                    // break;
                }
                lastProcessedAssetId = assetOwner.AssetID;
            }
            return lastProcessedAssetId;
        }
    }
}
