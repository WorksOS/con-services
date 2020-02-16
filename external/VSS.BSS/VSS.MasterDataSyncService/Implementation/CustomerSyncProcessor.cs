using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Nighthawk.MasterDataSync.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class CustomerSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri _customerApiEndPointUri;
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
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);

      //MasterData Updation
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow.AddMinutes(-1);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateCustomerEvent. LastProcessedId : {0}", lastProcessedId));
          var customerDataList = (from c in opCtx.CustomerReadOnly
                                  join ct in opCtx.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                                  join dn in opCtx.DealerNetworkReadOnly on c.fk_DealerNetworkID equals dn.ID
                                  join u in opCtx.UserReadOnly on c.ID equals u.fk_CustomerID into userSubset
                                  from us in userSubset.DefaultIfEmpty()
                                  where c.ID > lastProcessedId && c.UpdateUTC <= currentUtc && ct.Name != StringConstants.AccountCustomerType
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
                                    us.EmailContact,
                                    us.FirstName,
                                    us.LastName,
                                    UpdateUTC = currentUtc
                                  }).Take(BatchSize).ToList();

          if (customerDataList.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
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
                                              PrimaryContactEmail = customerData.EmailContact,
                                              FirstName = customerData.FirstName,
                                              LastName = customerData.LastName,
                                              ActionUTC = customerData.UpdateUTC
                                            };

            var svcResponse = ProcessServiceRequestAndResponse(createCustomer, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Post);
						Log.IfInfo("Create Customer: " + customerData.CustomerUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastProcessedId = customerData.ID;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(createCustomer, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastProcessedId = customerData.ID;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createCustomer));
                lastProcessedId = customerData.ID;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Customer service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1} ", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createCustomer)));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Creation {1} \n {2}", _taskName, e.Message, e.StackTrace));
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
            Log.IfInfo(string.Format("Completed Processing CreateCustomerEvent. LastProcessedId : {0} ", lastProcessedId)); 
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing UpdateCustomerEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var customerDataList = (from c in opCtx.CustomerReadOnly
                                  join ct in opCtx.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                                  join dn in opCtx.DealerNetworkReadOnly on c.fk_DealerNetworkID equals dn.ID
                                  where c.ID <= lastProcessedId && c.UpdateUTC <= currentUtc && c.UpdateUTC > lastUpdateUtc && ct.Name.ToUpper() != "ACCOUNT"
                                  orderby c.UpdateUTC, c.ID
                                  select new
                                  {
                                    c.ID,
                                    c.CustomerUID,
                                    c.Name,
                                    c.BSSID,
                                    DealerNetwork = dn.Name,
                                    c.NetworkDealerCode,
                                    c.NetworkCustomerCode,
                                    c.DealerAccountCode,
                                    c.UpdateUTC
                                  }).Take(BatchSize).ToList();


          if (customerDataList.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
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
                      ActionUTC = customerData.UpdateUTC
                    };

            var svcResponse = ProcessServiceRequestAndResponse(updateCustomerEvent, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Put);
						Log.IfInfo("Update Customer: " + customerData.CustomerUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = customerData.UpdateUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(updateCustomerEvent, _httpRequestWrapper, _customerApiEndPointUri, requestHeader, HttpMethod.Put);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = customerData.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateCustomerEvent));
                lastUpdateUtc = customerData.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateCustomerEvent)));
                return true;
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
          Log.IfInfo(string.Format("Completed Processing UpdateCustomerEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return true;
    }
  }
}
