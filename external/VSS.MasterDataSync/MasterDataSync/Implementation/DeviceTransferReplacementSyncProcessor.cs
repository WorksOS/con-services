using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Nighthawk.MasterDataSync.Interfaces;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
	public class DeviceTransferReplacementSyncProcessor : SyncProcessorBase
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Uri _deviceApiEndPointUri;
		private readonly Uri _associateDeviceAssetUri;
		private readonly Uri _dissociateDeviceAssetUri;
        private readonly Uri _assetOwnerUri;
        private readonly string _taskName;
		private readonly IHttpRequestWrapper _httpRequestWrapper;
		private readonly IConfigurationManager _configurationManager;

		public DeviceTransferReplacementSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
		  : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
		{
			_taskName = taskName;
			_httpRequestWrapper = httpRequestWrapper;
			_configurationManager = configurationManager;

			if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("DeviceService.WebAPIURI")))
				throw new ArgumentNullException("Uri", "Device api URL value cannot be empty");

			_deviceApiEndPointUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI"));
			_associateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/associatedeviceasset");
			_dissociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/dissociatedeviceasset");
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
			var lastProcessedId = GetLastProcessedId(_taskName);
			var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
			var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);
			return (isCreateEventProcessed);
		}

		// Based on book mark value of this task process records that are greated than the insertutc in Asset table
		private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
		{
            var currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds); // Processing 20 seconds before updated records inorder to avoid the Device UID mismatch for device replace/transfer;

            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
			{
				try
				{
					lastProcessedId = lastProcessedId ?? Int32.MinValue;
					Log.IfInfo(string.Format("Started Processing entries in AssetDeviceHistory LastProcessedId : {0}", lastProcessedId));

                    var assetDeviceHistory = (from adh in opCtx.AssetDeviceHistoryReadOnly
                                              join a in opCtx.AssetReadOnly on adh.fk_AssetID equals a.AssetID
                                              join newD in opCtx.DeviceReadOnly on a.fk_DeviceID equals newD.ID
                                              join oldD in opCtx.DeviceReadOnly on adh.fk_DeviceID equals oldD.ID
                                              where adh.ID > lastProcessedId // && a.UpdateUTC <= currentUtc   // && a.InsertUTC <= currentUtc
											  orderby adh.ID
											  select new
											  {
												  adh.ID,
                                                  a.AssetID,
											      a.AssetUID,
                                                  NewDeviceID = a.fk_DeviceID,
                                                  OldDeviceUID = oldD.DeviceUID,
                                                  NewDeviceUID = newD.DeviceUID,
                                                  OldOwnerBSSID = adh.OwnerBSSID,
                                                  NewOwnerBSSID = newD.OwnerBSSID,
												  UpdateUTC = adh.EndUTC
											  }).Take(BatchSize).ToList();

					if (assetDeviceHistory.Count < 1)
					{
						Log.IfInfo($"No {_taskName} data left in AssetDeviceHistory");
						return false;
					}
					var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

					if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
					{
						return true;
					}
				    requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                    foreach (var adh in assetDeviceHistory)
				    {
				        if (isServiceStopped)
				        {
				            Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
				            break;
				        }

				        if (adh.OldDeviceUID != adh.NewDeviceUID)
				        {
                            if (adh.NewDeviceID == 0 && adh.UpdateUTC > DateTime.UtcNow.AddMinutes(-5))
                            {
                                Log.IfInfo($"Waiting for the Asset with new device association event{adh.AssetUID}");
                                break;
                            }

                            var dissociateEvent = new DissociateDeviceAssetEvent()
                            {
                                AssetUID = (Guid)adh.AssetUID,
                                DeviceUID = (Guid)adh.OldDeviceUID,
                                ActionUTC = DateTime.UtcNow
                            };

                            var dResponse = ProcessServiceRequestAndResponse<DissociateDeviceAssetEvent>(dissociateEvent, _httpRequestWrapper, _dissociateDeviceAssetUri, requestHeader, HttpMethod.Post);
                            if (!(dResponse.StatusCode == HttpStatusCode.OK || dResponse.StatusCode == HttpStatusCode.BadRequest)) // APi throws bad request if there is no association exist for the dis-association request
                            {
                                Log.IfError($"Dissociate DeviceAsset failed with status code: {dResponse.StatusCode} {dResponse.Content.ReadAsStringAsync().Result}, payload : {JsonHelper.SerializeObjectToJson(dissociateEvent)}");
                                break;
                            }
				            if (adh.NewDeviceID != 0)
				            {
				                var associateEvent = new AssociateDeviceAssetEvent()
				                {
				                    AssetUID = (Guid) adh.AssetUID,
				                    DeviceUID = (Guid) adh.NewDeviceUID,
				                    ActionUTC = DateTime.UtcNow
				                };
				                var aResponse = ProcessServiceRequestAndResponse<AssociateDeviceAssetEvent>(associateEvent, _httpRequestWrapper, _associateDeviceAssetUri, requestHeader, HttpMethod.Post);
                                if (!(aResponse.StatusCode == HttpStatusCode.OK || aResponse.StatusCode == HttpStatusCode.BadRequest)) // APi throws bad request if there is no association exist for the dis-association request
								{
                                    Log.IfError($"Associate DeviceAsset failed with status code: {aResponse.StatusCode} {aResponse.Content.ReadAsStringAsync().Result}, payload : {JsonHelper.SerializeObjectToJson(associateEvent)}");
                                    break;
                                }
                                // publish ASsetOwner Update event.
                            }
                        }
				        if (adh.OldOwnerBSSID != adh.NewOwnerBSSID & adh.NewDeviceID != 0)
				        {
                            var assetOwnerEvent = getAssetOwnerEvent(opCtx, adh.AssetID);
                            var ownerResponse = ProcessServiceRequestAndResponse<AssetOwnerEvent>(assetOwnerEvent, _httpRequestWrapper, _assetOwnerUri, requestHeader, HttpMethod.Post);

                            if (ownerResponse.StatusCode != HttpStatusCode.OK)
                            {
                                Log.IfError($"AssetOwner Update Event failed with Status Code : {ownerResponse.StatusCode}, payload : {JsonHelper.SerializeObjectToJson(assetOwnerEvent)}");
                                break;
                            }
                            // ToDo: publish UpdateAssetEvent
                            // asset.OwningCustomerUID = (if customerUID is nul  = dealerUID else =CustomerUID

                        }

                        lastProcessedId = adh.ID;
				    }
				}
				catch (Exception e)
				{
					Log.IfError($"Exception in processing {_taskName} Records {e.Message} \n {e.StackTrace}");
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
						Log.IfInfo($"Completed Processing entries in AssetDeviceHistory LastProcessedId : {lastProcessedId} ");
					}
					else
					{
						Log.IfInfo(string.Format("No Records Processed in AssetDeviceHistory"));
					}
				}
			}
			return true;
		}
        private AssetOwnerEvent getAssetOwnerEvent(INH_OP ctx,long assetId)
        {

			var assetOwner = (from a in ctx.AssetReadOnly
                             join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                             where a.AssetID == assetId
                             join dass in ctx.CustomerReadOnly on d.OwnerBSSID equals dass.BSSID into dealerOrAccountSubSet
                             from dealerOrAccount in dealerOrAccountSubSet.DefaultIfEmpty()
                             join crss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer)
                                                  on dealerOrAccount.ID equals crss.fk_ClientCustomerID into customerRelationSubset
                             from cr in customerRelationSubset.DefaultIfEmpty()
                             join css in ctx.CustomerReadOnly on cr.fk_ParentCustomerID equals css.ID into customerSubset
                             from customer in customerSubset.DefaultIfEmpty()
                             join drss in ctx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer)
                                                  on dealerOrAccount.ID equals drss.fk_ClientCustomerID into dealerRelationSubset
                             from dr in dealerRelationSubset.DefaultIfEmpty()
                             join dss in ctx.CustomerReadOnly on dr.fk_ParentCustomerID equals dss.ID into dealerSubset
                             from dealer in dealerSubset.DefaultIfEmpty()
                             orderby a.InsertUTC, a.AssetID
                             select new AssetOwnerEvent
                             {
                                 AssetUID = (Guid) a.AssetUID,
                                 AssetOwnerRecord =
                                     new AssetOwner
                                     {
                                         CustomerName = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? ((customer == null)? "":customer.Name) : "",
                                         AccountName = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.Name : "",
                                         AccountUID = dealerOrAccount == null ? Guid.Empty : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? (Guid) dealerOrAccount.CustomerUID : Guid.Empty,
										 DealerAccountCode = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.DealerAccountCode : "",
                                         DealerUID = dealerOrAccount == null ? Guid.Empty : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? (Guid) dealerOrAccount.CustomerUID : ((dealer == null ) ? Guid.Empty :(Guid)dealer.CustomerUID),
                                         DealerName = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? dealerOrAccount.Name : ((dealer == null)? "" : dealer.Name),
                                         NetworkDealerCode = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer) ? dealerOrAccount.NetworkDealerCode : ((dealer == null)?"": dealer.NetworkDealerCode),
                                         NetworkCustomerCode = dealerOrAccount == null ? "" : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? dealerOrAccount.NetworkCustomerCode : "",
                                         CustomerUID = dealerOrAccount == null ? Guid.Empty : (dealerOrAccount.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) ? ((customer == null )? Guid.Empty : (Guid)customer.CustomerUID) : Guid.Empty
									 },
                                 Action = "Update",
                                 ActionUTC = DateTime.UtcNow
                             }
                        ).FirstOrDefault();

            return assetOwner;
        }
	}
}

