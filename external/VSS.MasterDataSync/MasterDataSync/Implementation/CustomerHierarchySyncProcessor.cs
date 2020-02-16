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
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
    public class CustomerHierarchySyncProcessor : SyncProcessorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _taskName;
        private readonly IHttpRequestWrapper _httpRequestWrapper;
        private readonly IConfigurationManager _configurationManager;
        private readonly Uri CustomerHierarchyUri;

        public CustomerHierarchySyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
          : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
        {
            _taskName = taskName;
            _httpRequestWrapper = httpRequestWrapper;
            _configurationManager = configurationManager;

            if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("CustomerService.WebAPIURI")))
                throw new ArgumentNullException("Uri", "Customer Hierarchy URL value cannot be empty");

            CustomerHierarchyUri = new Uri(_configurationManager.GetAppSetting("CustomerService.WebAPIURI") + "/customerrelationship");
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
            //MasterData Event for Customer Hierarchy
            var lastProcessedId = GetLastProcessedId(_taskName);
            var isAssociateEventProcessed = ProcessInsertionRecords(lastProcessedId, ref isServiceStopped);
            return isAssociateEventProcessed;
        }

        private bool ProcessInsertionRecords(long? lastProcessedId, ref bool isServiceStopped)
        {
            var currentUtc = DateTime.UtcNow;
            lastProcessedId = lastProcessedId ?? Int32.MinValue;

            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                try
                {
                    Log.IfInfo(string.Format("Started Processing CustomerHierarchyEvent. LastProcessedId : {0} ", lastProcessedId));

                    TaskState customerTaskState = (from m in opCtx.MasterDataSyncReadOnly
                                                   where m.TaskName == StringConstants.CustomerTask
                                                   select new TaskState() { lastProcessedId = m.LastProcessedID ?? Int32.MinValue, InsertUtc = m.LastInsertedUTC }).FirstOrDefault();				

					if (customerTaskState != null)
                    {
						var customerLastProcessedID = opCtx.MasterDataSyncReadOnly.SingleOrDefault(task => task.TaskName == "Customer").LastProcessedID; // TODO move this value to webconfig once the config values are corrected

						var customerHierarchyEventDataList = (from cr in opCtx.CustomerRelationshipExportReadOnly
                                                              join cp in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on cr.ParentCustomerID equals cp.ID into parentCustomerSubset
                                                              join cc in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on cr.AssociatedCustomerID equals cc.ID into childCustomerSubset
                                                              join acc in opCtx.CustomerReadOnly.Where(e => e.ID <= customerTaskState.lastProcessedId) on cr.AccountCustomerID equals acc.ID into accountCustomerSubset
                                                              from cpc in parentCustomerSubset.DefaultIfEmpty()
                                                              from ccc in childCustomerSubset.DefaultIfEmpty()
                                                              from acs in accountCustomerSubset.DefaultIfEmpty()
                                                              where cr.ID > lastProcessedId && cr.ParentCustomerID <= customerLastProcessedID && cr.AssociatedCustomerID   <= customerLastProcessedID 
															  && (cr.AccountCustomerID ??0) <= customerLastProcessedID
															  orderby cr.ID
                                                              select new
                                                              {
                                                                  cr.ID,
                                                                  ParentCustomerUID = cpc.CustomerUID,
                                                                  ChildCustomerUID = ccc.CustomerUID,
                                                                  AccountCustomerUID = (acs != null) ? acs.CustomerUID : null,
                                                                  AccountUIDInExport = cr.AccountCustomerID, // To handle dealer -> subdealer valid scenario
                                                                  ChildCustomerType = ccc.fk_CustomerTypeID == null ? -1 : ccc.fk_CustomerTypeID,
                                                                  cr.Operation
                                                              }).Take(BatchSize).ToList();


                        if (customerHierarchyEventDataList.Count < 1)
                        {
                            Log.IfInfo(string.Format("No {0} data left for CustomerHierarchy Event", _taskName));
                            return false;
                        }

                        var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

                        if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
                        {
                            return true;
                        }

                        foreach (var customerHierarchyEventData in customerHierarchyEventDataList)
                        {
                            if (isServiceStopped)
                            {
                                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                                break;
                            }

                            if (customerHierarchyEventData.ChildCustomerUID == null || (customerHierarchyEventData.AccountUIDInExport != null && customerHierarchyEventData.AccountCustomerUID == null) || (customerHierarchyEventData.ParentCustomerUID == null && customerHierarchyEventData.ChildCustomerType != (int)CustomerTypeEnum.Corporate) )
                            {
                                Log.IfInfo("The required CustomerUID's CreateEvent has not been processed yet..");
                                return true;
                            }
							// Allow the record if processed by Customer task

							

                            if (customerHierarchyEventData.Operation.ToLower() == "add")
                            {
                                var customerHierarchyEvent = new CreateCustomerRelationshipEvent
                                {
                                    ParentCustomerUID = customerHierarchyEventData.ParentCustomerUID,
                                    ChildCustomerUID = customerHierarchyEventData.ChildCustomerUID.Value,
                                    AccountCustomerUID = customerHierarchyEventData.AccountCustomerUID,
                                    ActionUTC = DateTime.UtcNow
                                };
                                var svcResponseForCustomerHierarchy = ProcessServiceRequestAndResponse(customerHierarchyEvent, _httpRequestWrapper, CustomerHierarchyUri, requestHeader, HttpMethod.Post);
                                Log.IfInfo("Create Customer relationship parent: " + customerHierarchyEventData.ParentCustomerUID + " child: " + customerHierarchyEventData.ChildCustomerUID.Value + " returned " + svcResponseForCustomerHierarchy.StatusCode);
                                switch (svcResponseForCustomerHierarchy.StatusCode)
                                {
                                    case HttpStatusCode.OK:
                                        lastProcessedId = customerHierarchyEventData.ID;
                                        break;
                                    case HttpStatusCode.Unauthorized:
                                        requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                                        svcResponseForCustomerHierarchy = ProcessServiceRequestAndResponse(customerHierarchyEvent, _httpRequestWrapper, CustomerHierarchyUri, requestHeader, HttpMethod.Post);

                                        if (svcResponseForCustomerHierarchy.StatusCode == HttpStatusCode.OK)
                                        {
                                            lastProcessedId = customerHierarchyEventData.ID;
                                        }
                                        break;
                                    case HttpStatusCode.InternalServerError:
                                        Log.IfError("Internal server error");
                                        return true;
                                    case HttpStatusCode.BadRequest:
                                        Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(customerHierarchyEventData));
                                        lastProcessedId = customerHierarchyEventData.ID;
                                        break;
                                    case HttpStatusCode.Forbidden:
                                        Log.IfError("Forbidden status code received while hitting Tpaas Customer service");
                                        break;
                                    default:
                                        Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload  = {1} ", svcResponseForCustomerHierarchy.StatusCode, JsonHelper.SerializeObjectToJson(customerHierarchyEventData)));
                                        return true;
                                }
                            }
                            else if (customerHierarchyEventData.Operation.ToLower() == "remove" || customerHierarchyEventData.Operation.ToLower() == "removecustomer" || customerHierarchyEventData.Operation.ToLower() == "removedealer")
                            {
                                Guid accountUID = (customerHierarchyEventData.AccountCustomerUID.HasValue) ? customerHierarchyEventData.AccountCustomerUID.Value : Guid.Empty;
                                var requestUri = new Uri(CustomerHierarchyUri + "?parentCustomerUID=" + customerHierarchyEventData.ParentCustomerUID + "&childCustomerUID=" +
                                 customerHierarchyEventData.ChildCustomerUID + "&actionUTC=" + DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")
                                 + "&type=" + customerHierarchyEventData.Operation + "&accountCustomerUID=" + accountUID);

                                var serviceRequestMessage = new VSS.Hosted.VLCommon.Services.MDM.Models.ServiceRequestMessage
                                {
                                    RequestHeaders = requestHeader,
                                    RequestMethod = HttpMethod.Delete,
                                    RequestUrl = requestUri
                                };

                                var svcResponseForCustomerHierarchy = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);
                                Log.IfInfo("Remove Customer relationship parent: " + customerHierarchyEventData.ParentCustomerUID + " child: " + customerHierarchyEventData.ChildCustomerUID.Value + " returned " + svcResponseForCustomerHierarchy.StatusCode);
                                switch (svcResponseForCustomerHierarchy.StatusCode)
                                {
                                    case HttpStatusCode.OK:
                                        lastProcessedId = customerHierarchyEventData.ID;
                                        break;
                                    case HttpStatusCode.Unauthorized:
                                        requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                                        svcResponseForCustomerHierarchy = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

                                        if (svcResponseForCustomerHierarchy.StatusCode == HttpStatusCode.OK)
                                        {
                                            lastProcessedId = customerHierarchyEventData.ID;
                                        }
                                        break;
                                    case HttpStatusCode.InternalServerError:
                                        Log.IfError("Internal server error");
                                        return true;
                                    case HttpStatusCode.BadRequest:
                                        Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(customerHierarchyEventData));
                                        lastProcessedId = customerHierarchyEventData.ID;
                                        break;
                                    case HttpStatusCode.Forbidden:
                                        Log.IfError("Forbidden status code received while hitting Tpaas Customer service");
                                        break;
                                    default:
                                        Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload  = {1} ", svcResponseForCustomerHierarchy.StatusCode, JsonHelper.SerializeObjectToJson(customerHierarchyEventData)));
                                        return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.IfError(string.Format("Exception in processing {0} CustomerHierarchy Event {1} \n {2}", _taskName, e.Message,
                      e.StackTrace));
                }
                finally
                {
                    if (lastProcessedId != Int32.MinValue)
                    {
                        //Update the last processed Id to masterdatasync
                        opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
                        opCtx.SaveChanges();
                        Log.IfInfo(
                          string.Format("Completed Processing CustomerHierarchyEvent. LastProcessedId : {0} ", lastProcessedId));
                    }
                    else
                    {
                        Log.IfInfo("No Records Processed");
                    }
                }
            }
            return true;
        }

    }
}
