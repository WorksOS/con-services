using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
	public class DeviceSyncProcessor : SyncProcessorBase
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Uri DeviceApiEndPointUri;
		private readonly Uri AssociateDeviceAssetUri;
		private readonly Uri DissociateDeviceAssetUri;
		private readonly string _taskName;
		private readonly IHttpRequestWrapper _httpRequestWrapper;
		private readonly IConfigurationManager _configurationManager;
		private const string MTS_COMMUNICATION_GATEWAY_MID = "161";
		private const string PRODUCT_LINK_MID = "122";
		private const string J1939_522_MID = "400";
		private const string J1939_522_MID2 = "402";
		private const string DeviceSource = "Device";
		private const string DevicePersonalitySource = "DevicePersonality";
		private const string DeviceDataLinkSource = "DeviceDataLink";
		private static readonly List<string> deviceMIDs = new List<string> { MTS_COMMUNICATION_GATEWAY_MID, PRODUCT_LINK_MID, J1939_522_MID, J1939_522_MID2 };
		private static readonly List<int> PLDeviceTypeIDList = new List<int> { (int)DeviceTypeEnum.PL121, (int)DeviceTypeEnum.PL321 };
		private static readonly List<int> MTSDeviceTypeIDList = new List<int> { (int)DeviceTypeEnum.Series522, (int)DeviceTypeEnum.Series523,
		(int)DeviceTypeEnum.Series521, (int)DeviceTypeEnum.SNM940,
		(int)DeviceTypeEnum.CrossCheck, (int)DeviceTypeEnum.TrimTrac,
		(int)DeviceTypeEnum.PL420, (int)DeviceTypeEnum.PL421, (int)DeviceTypeEnum.SNM451,
		(int)DeviceTypeEnum.PL431, (int)DeviceTypeEnum.DCM300,
		(int)DeviceTypeEnum.SNM941 };

		public DeviceSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
		  : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
		{
			_taskName = taskName;
			_httpRequestWrapper = httpRequestWrapper;
			_configurationManager = configurationManager;

			if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("DeviceService.WebAPIURI")))
				throw new ArgumentNullException("Uri", "Device api URL value cannot be empty");

			DeviceApiEndPointUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI"));
			AssociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/associatedeviceasset");
			DissociateDeviceAssetUri = new Uri(_configurationManager.GetAppSetting("DeviceService.WebAPIURI") + "/dissociatedeviceasset");
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

			//MasterData Updation
			var lastUpdateUtc = GetLastUpdateUTC(_taskName);
			var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
			return (isCreateEventProcessed || isUpdateEventProcessed);
		}

		private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
		{
            var currentUtc = DateTime.UtcNow;  //.AddSeconds(-SyncPrioritySeconds);
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
			{
				try
				{
					lastProcessedId = lastProcessedId ?? int.MinValue;
					Log.IfInfo($"Started Processing CreateDeviceEvent LastProcessedId : {lastProcessedId}");

                    var deviceDataList = (from d in opCtx.DeviceReadOnly
                                          join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
                                          join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
                                          join dp in opCtx.DevicePersonalityReadOnly.Where(e => (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware)) on d.ID equals dp.fk_DeviceID into devicepersonalitysubset
                                          from dpt in devicepersonalitysubset.DefaultIfEmpty()
                                          join ecmInfo in opCtx.ECMInfoReadOnly on d.ID equals ecmInfo.fk_DeviceID into ecmInfoSubset
                                          from ecmS in ecmInfoSubset.DefaultIfEmpty()
                                          join ecmDLinkInfo in opCtx.ECMDatalinkInfoReadOnly on ecmS.ID equals ecmDLinkInfo.fk_ECMInfoID into ecmDLinkSubset
                                          from edls in ecmDLinkSubset.DefaultIfEmpty()
                                          join dataLink in opCtx.DatalinkReadOnly on edls.fk_DatalinkID equals dataLink.ID into dlSubset
                                          from dl in dlSubset.DefaultIfEmpty()
                                          join mid in opCtx.MIDReadOnly.Where(e => (e.MID1 != null && e.MID1.Length > 0)) on edls.fk_MIDID equals mid.ID into midSubset
                                          from md in midSubset.DefaultIfEmpty()
                                          where d.ID > lastProcessedId // && d.UpdateUTC <= currentUtc
                                          orderby d.ID ascending
                                          select new
                                          {
                                              d.ID,
                                              d.DeviceUID,
                                              d.GpsDeviceID,
                                              d.OwnerBSSID,
                                              DeviceType = dt.Name,
                                              DeviceTypeID = dt.ID,
                                              DeviceState = ds.Description,
                                              d.DeregisteredUTC,
                                              d.DeviceDetailsXML,
                                              dpt,
                                              md,
                                              ecmIsDevice = md.MID1 != null ? deviceMIDs.Contains(md.MID1) : false,
                                              datalinkName = dl.Name,
                                              UpdateUTC = currentUtc
										  }).Take(BatchSize).ToList();

					if (deviceDataList.Count < 1)
					{
						Log.IfInfo($"No {_taskName} data left for creation");
						return false;
					}
					var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

					if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
					{
						return true;
					}

					var deviceIdList = deviceDataList.GroupBy(e => e.ID).OrderBy(e => e.Key).Select(e => e.Key).ToList();
					foreach (var deviceId in deviceIdList)
					{
						if (isServiceStopped)
						{
							Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
							break;
						}

						var deviceData = deviceDataList.Where(e => e.ID == deviceId).ToList();

						PLConfigData pldata = null;
						string moduleType, dataLinkType, firmwarePartNumber, gatewayFirmwarePartNumber, radioFirmwarePartNumber, firmwareVersion, networkManagerFirmware, cellularRadioFirmware, satelliteRadioFirmware, ecmGatewayHardwarePartNumber, cellModemIMEI;
						moduleType = dataLinkType = firmwarePartNumber = gatewayFirmwarePartNumber = radioFirmwarePartNumber = firmwareVersion = networkManagerFirmware = cellularRadioFirmware = satelliteRadioFirmware = ecmGatewayHardwarePartNumber = cellModemIMEI = null;

						string deviceDetailsXML = deviceData.Select(e => e.DeviceDetailsXML).FirstOrDefault();
						var deviceType = deviceData.Select(e => e.DeviceTypeID).First();

						if (PLDeviceTypeIDList.Contains(deviceType))
						{
							if (!string.IsNullOrWhiteSpace(deviceDetailsXML) && deviceDetailsXML.Contains("moduleType") && deviceDetailsXML.Contains("SoftwareInfo"))
							{
								pldata = new PLConfigData(deviceDetailsXML);
								moduleType = pldata.CurrentGeneralRegistry.ModuleType;

								var softwareInfoElement = pldata.CurrentGeneralRegistry.Software;
								if (softwareInfoElement != null)
								{
									gatewayFirmwarePartNumber = softwareInfoElement.HC11SoftwarePartNumber != null
																				? softwareInfoElement.HC11SoftwarePartNumber
																				: string.Empty;
									radioFirmwarePartNumber = softwareInfoElement.ModemSoftwarePartNumber != null
																			  ? softwareInfoElement.ModemSoftwarePartNumber
																			  : string.Empty;
								}
							}
						}
						else
						{
							var tempPartNumber = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway));
							if (tempPartNumber != null && tempPartNumber.dpt != null)
							{
								string deviceFirmwarePartNumber = tempPartNumber.dpt.Value;

								if (!string.IsNullOrWhiteSpace(deviceFirmwarePartNumber))
								{
									var partnumbers = deviceFirmwarePartNumber.Split(' ');
									//char[] charArray = partNumber.ToCharArray();
									//Array.Reverse(charArray);
									//string revPartNumber = new string(charArray);
									//gatewayFirmwarePartNumber = partNumber.Substring(partNumber.Split(' ')[0].Length, partNumber.Length - revPartNumber.Split()[0].Length - partNumber.Split()[0].Length).Trim();
									if (partnumbers.Length == 3)
									{
										ecmGatewayHardwarePartNumber = partnumbers[0];
										gatewayFirmwarePartNumber = partnumbers[1];
									}
								}
							}
						}

						var tempVersion = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software));
						if (tempVersion != null && tempVersion.dpt != null)
						{
							string deviceFirmwareVersion = tempVersion.dpt.Value;

							if (!string.IsNullOrWhiteSpace(deviceFirmwareVersion))
							{
								var firmwareMsgs = deviceFirmwareVersion.Split(' ');
								firmwareVersion = firmwareMsgs[0];
								firmwarePartNumber = (firmwareMsgs.Length > 2) ? firmwareMsgs[2] : null;
							}
						}

						var tempVersion2 = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware));
						if (tempVersion2 != null && tempVersion2.dpt != null)
						{
							cellularRadioFirmware = tempVersion2.dpt.Value;
						}

						var tempVersion3 = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware));
						if (tempVersion3 != null && tempVersion3.dpt != null)
						{
							satelliteRadioFirmware = tempVersion3.dpt.Value;
						}

						var tempVersion4 = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware));
						if (tempVersion4 != null && tempVersion4.dpt != null)
						{
							networkManagerFirmware = tempVersion4.dpt.Value;
						}

						if (deviceType == (int)DeviceTypeEnum.TrimTrac)
						{
							cellModemIMEI = deviceData.Select(e => e.GpsDeviceID).First();
						}
						else
						{
							var tempVersion5 = deviceData.FirstOrDefault(e => (e.dpt != null && e.dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN));
							if (tempVersion5 != null && tempVersion5.dpt != null)
							{
								cellModemIMEI = tempVersion5.dpt.Value;
							}
						}

						if (PLDeviceTypeIDList.Contains(deviceType))
						{
							//PL device type; DataLink names could be different, but ecmIsDevice is not set
							var edmNames = deviceData.Where(e => e.md != null && e.md.MID1 != null && e.md.MID1.Length > 0).Select(e => e.datalinkName).Distinct().ToList();
							dataLinkType = EcmNameStr(edmNames);
						}
						else
						{
							var edmNames = deviceData.Where(e => e.ecmIsDevice && e.md != null && e.md.MID1 != null && e.md.MID1.Length > 0).Select(e => e.datalinkName).Distinct().ToList();
							dataLinkType = EcmNameStr(edmNames);
						}

						bool isMTSDeviceType = MTSDeviceTypeIDList.Contains((int)deviceType);

						var createDevice = new CreateDeviceEvent
						{
							DeviceUID = deviceData.Select(e => (Guid)e.DeviceUID).First(),
							DeviceSerialNumber = deviceData.Select(e => e.GpsDeviceID).First(),
							DeviceType = deviceData.Select(e => e.DeviceType).First(),
							DeviceState = deviceData.Select(e => e.DeviceState).First(),
							DeregisteredUTC = deviceData.Select(e => e.DeregisteredUTC).First(),
							ModuleType = isMTSDeviceType ? moduleType : null,
							MainboardSoftwareVersion = isMTSDeviceType ? firmwareVersion : null,
							FirmwarePartNumber = isMTSDeviceType ? firmwarePartNumber : null,
							RadioFirmwarePartNumber = isMTSDeviceType ? radioFirmwarePartNumber : null,
							GatewayFirmwarePartNumber = isMTSDeviceType ? gatewayFirmwarePartNumber : null,
							DataLinkType = isMTSDeviceType ? dataLinkType : null,
							CellModemIMEI = isMTSDeviceType ? cellModemIMEI : null,
							DevicePartNumber = isMTSDeviceType ? ecmGatewayHardwarePartNumber : null,
							CellularFirmwarePartnumber = isMTSDeviceType ? cellularRadioFirmware : null,
							NetworkFirmwarePartnumber = isMTSDeviceType ? networkManagerFirmware : null,
							SatelliteFirmwarePartnumber = isMTSDeviceType ? satelliteRadioFirmware : null,
							ActionUTC = DateTime.UtcNow
						};

                        var svcResponse = ProcessServiceRequestAndResponse<CreateDeviceEvent>(createDevice, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Post);

                        if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.BadRequest)
                        {
                            lastProcessedId = deviceId;
                        }
                        else
                        {
                            Log.IfError($"Device Create event failed for {createDevice.DeviceUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(createDevice)}");
                            break;
                        }

					}
				}
				catch (Exception e)
				{
					Log.IfError(string.Format("Exception in processing {0} Insertion {1} \n {2}", _taskName, e.Message, e.StackTrace));
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
						Log.IfInfo(string.Format("Completed Processing CreateDeviceEvent LastProcessedId : {0} ", lastProcessedId));
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
			Log.IfInfo(string.Format("Started Processing UpdateDeviceEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
			using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
			{
				DateTime currentUtc = DateTime.UtcNow.AddSeconds(-SyncPrioritySeconds);
                try
				{
					var deviceDataList = (from d in opCtx.DeviceReadOnly
										  join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
										  join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
										  join customer in opCtx.CustomerReadOnly on d.OwnerBSSID equals customer.BSSID
										  join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on customer.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
										  from crt in customerRelationshipSubset.DefaultIfEmpty()
										  join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
										  from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
										  join dp in opCtx.DevicePersonalityReadOnly.Where(e => ((e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware))) on d.ID equals dp.fk_DeviceID into devicePersonalitySubset
										  from dpt in devicePersonalitySubset.DefaultIfEmpty()
										  join ecmInfo in opCtx.ECMInfoReadOnly on d.ID equals ecmInfo.fk_DeviceID into ecmInfoSubset
										  from ecmS in ecmInfoSubset.DefaultIfEmpty()
										  join ecmDLinkInfo in opCtx.ECMDatalinkInfoReadOnly on ecmS.ID equals ecmDLinkInfo.fk_ECMInfoID into ecmDLinkSubset
										  from edls in ecmDLinkSubset.DefaultIfEmpty()
										  join dataLink in opCtx.DatalinkReadOnly on edls.fk_DatalinkID equals dataLink.ID into dlSubset
										  from dl in dlSubset.DefaultIfEmpty()
										  join mid in opCtx.MIDReadOnly.Where(md => md.MID1 != null && md.MID1.Length > 0) on edls.fk_MIDID equals mid.ID into midSubset
										  from md in midSubset.DefaultIfEmpty()
										  where d.ID <= lastProcessedId && d.UpdateUTC <= currentUtc && d.UpdateUTC > lastUpdateUtc
										  orderby d.UpdateUTC
										  select new
										  {
											  Source = DeviceSource,
											  d.ID,
											  d.DeviceUID,
											  d.GpsDeviceID,
											  d.OwnerBSSID,
											  DeviceType = dt.Name,
											  DeviceTypeID = dt.ID,
											  OwningCustomerUID = customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : customer.CustomerUID,
											  DeviceState = ds.Description,
											  d.DeregisteredUTC,
											  d.DeviceDetailsXML,
											  dpt,
											  PersonalityType = (int?)dpt.fk_PersonalityTypeID,
											  datalinkName = dl.Name,
											  ecmIsDevice = (bool?)deviceMIDs.Contains(md.MID1),
											  UpdateUTC = d.UpdateUTC
										  }).Take(BatchSize).ToList();

					var devicePersonalityList = (from dpt in opCtx.DevicePersonalityReadOnly
												 join d in opCtx.DeviceReadOnly on dpt.fk_DeviceID equals d.ID
												 join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
												 join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
												 join customer in opCtx.CustomerReadOnly on d.OwnerBSSID equals customer.BSSID
												 join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on customer.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
												 from crt in customerRelationshipSubset.DefaultIfEmpty()
												 join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
												 from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
												 join ecmInfo in opCtx.ECMInfoReadOnly on d.ID equals ecmInfo.fk_DeviceID into ecmInfoSubset
												 from ecmS in ecmInfoSubset.DefaultIfEmpty()
												 join ecmDLinkInfo in opCtx.ECMDatalinkInfoReadOnly on ecmS.ID equals ecmDLinkInfo.fk_ECMInfoID into ecmDLinkSubset
												 from edls in ecmDLinkSubset.DefaultIfEmpty()
												 join dataLink in opCtx.DatalinkReadOnly on edls.fk_DatalinkID equals dataLink.ID into dlSubset
												 from dl in dlSubset.DefaultIfEmpty()
												 join mid in opCtx.MIDReadOnly.Where(md => md.MID1 != null && md.MID1.Length > 0) on edls.fk_MIDID equals mid.ID into midSubset
												 from md in midSubset.DefaultIfEmpty()
												 where dpt.fk_DeviceID <= lastProcessedId && (dpt.UpdateUTC > lastUpdateUtc && dpt.UpdateUTC <= currentUtc)
													   && ((dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware))
												 orderby dpt.UpdateUTC
												 select new
												 {
													 Source = DevicePersonalitySource,
													 d.ID,
													 d.DeviceUID,
													 d.GpsDeviceID,
													 d.OwnerBSSID,
													 DeviceType = dt.Name,
													 DeviceTypeID = dt.ID,
													 OwningCustomerUID = customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : customer.CustomerUID,
													 DeviceState = ds.Description,
													 d.DeregisteredUTC,
													 d.DeviceDetailsXML,
													 dpt,
													 PersonalityType = (int?)dpt.fk_PersonalityTypeID,
													 datalinkName = dl.Name,
													 ecmIsDevice = (bool?)deviceMIDs.Contains(md.MID1),
													 UpdateUTC = dpt.UpdateUTC
												 }).Take(BatchSize).ToList();

					var deviceDataLinkList = (from ecmInfo in opCtx.ECMInfoReadOnly
											  join d in opCtx.DeviceReadOnly on ecmInfo.fk_DeviceID equals d.ID
											  join dt in opCtx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
											  join ds in opCtx.DeviceStateReadOnly on d.fk_DeviceStateID equals ds.ID
											  join customer in opCtx.CustomerReadOnly on d.OwnerBSSID equals customer.BSSID
											  join cr in opCtx.CustomerRelationshipReadOnly.Where(e => e.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer) on customer.ID equals cr.fk_ClientCustomerID into customerRelationshipSubset
											  from crt in customerRelationshipSubset.DefaultIfEmpty()
											  join crc in opCtx.CustomerReadOnly on crt.fk_ParentCustomerID equals crc.ID into customerRelationshipCustomerSubset
											  from crct in customerRelationshipCustomerSubset.DefaultIfEmpty()
											  join ecmDLinkInfo in opCtx.ECMDatalinkInfoReadOnly on ecmInfo.ID equals ecmDLinkInfo.fk_ECMInfoID
											  join dataLink in opCtx.DatalinkReadOnly on ecmDLinkInfo.fk_DatalinkID equals dataLink.ID
											  join mid in opCtx.MIDReadOnly on ecmDLinkInfo.fk_MIDID equals mid.ID
											  join dp in opCtx.DevicePersonalityReadOnly.Where(e => ((e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware) || (e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware))) on d.ID equals dp.fk_DeviceID into devicePersonalitySubset
											  from dpt in devicePersonalitySubset.DefaultIfEmpty()
											  where ecmInfo.fk_DeviceID <= lastProcessedId && ecmDLinkInfo.updateUTC > lastUpdateUtc && ecmDLinkInfo.updateUTC <= currentUtc && mid.MID1 != null && mid.MID1.Length > 0
											  orderby ecmDLinkInfo.updateUTC
											  select new
											  {
												  Source = DeviceDataLinkSource,
												  d.ID,
												  d.DeviceUID,
												  d.GpsDeviceID,
												  d.OwnerBSSID,
												  DeviceType = dt.Name,
												  DeviceTypeID = dt.ID,
												  OwningCustomerUID = customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account ? crct.CustomerUID : customer.CustomerUID,
												  DeviceState = ds.Description,
												  d.DeregisteredUTC,
												  d.DeviceDetailsXML,
												  dpt,
												  PersonalityType = (int?)dpt.fk_PersonalityTypeID,
												  datalinkName = dataLink.Name,
												  ecmIsDevice = (bool?)deviceMIDs.Contains(mid.MID1),
												  UpdateUTC = ecmDLinkInfo.updateUTC
											  }).Take(BatchSize).ToList();

					if (!deviceDataList.Any() && !devicePersonalityList.Any() && !deviceDataLinkList.Any())
					{
						lastUpdateUtc = currentUtc;
						Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
						return false;
					}

					//Setting the min of all three list's max utc to limit the upper boundary
					var deviceMaxUtc = deviceDataList.Max(e => e.UpdateUTC as DateTime?) ?? currentUtc;
					var devicePersonalityMaxUtc = devicePersonalityList.Max(e => e.UpdateUTC as DateTime?) ?? currentUtc;
					var deviceDataLinkMaxUtc = deviceDataLinkList.Max(e => e.UpdateUTC as DateTime?) ?? currentUtc;
					var minOfMaxUtc = GetMinUtc(deviceMaxUtc, devicePersonalityMaxUtc, deviceDataLinkMaxUtc);

					//using the upper boundary in all lists to limit records there by avoiding processing futuristic records and advancing the bookmark
					var deviceDetailsList = deviceDataList.Where(e => e.UpdateUTC <= minOfMaxUtc).Union(devicePersonalityList.Where(e => e.UpdateUTC <= minOfMaxUtc)).Union(deviceDataLinkList.Where(e => e.UpdateUTC <= minOfMaxUtc));

					//Grouping device based on deviceid and ordering it based on min updateutc, there by the device group which has minimum updatetc will be at top.
					var deviceIdList = deviceDetailsList.GroupBy(e => e.ID).OrderBy(e => e.Min(s => s.UpdateUTC)).Select(e => e.Key).ToList();

					var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

					if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
					{
						return true;
					}

					foreach (var deviceId in deviceIdList)
					{
						if (isServiceStopped)
						{
							Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
							break;
						}

						var dptValues = (from dpt in opCtx.DevicePersonalityReadOnly
										 where dpt.fk_DeviceID == deviceId
															  && ((dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware) || (dpt.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware))
										 select dpt).ToList();

						var deviceDetail = deviceDetailsList.Where(e => e.ID == deviceId).ToList();
						var deviceData = deviceDetail.Where(e => e.Source == DeviceSource).ToList();
						var devicePersonalityData = deviceDetail.Where(e => e.Source == DevicePersonalitySource).ToList();
						var deviceDataLinkData = deviceDetail.Where(e => e.Source == DeviceDataLinkSource).ToList();
						var deviceFinalData = deviceData.Any() ? deviceData : (devicePersonalityData.Any() ? devicePersonalityData : deviceDataLinkData);
						var utc = deviceData.Any() ? deviceData.Min(e => e.UpdateUTC) : currentUtc;
						var utc1 = devicePersonalityData.Any() ? devicePersonalityData.Min(e => e.UpdateUTC) : currentUtc;
						var utc2 = deviceDataLinkData.Any() ? deviceDataLinkData.Min(e => e.UpdateUTC) : currentUtc;
						// Finding the minimum update utc
						var minUtc = GetMinUtc(utc, utc1, utc2);
						PLConfigData pldata = null;
						string moduleType, dataLinkType, firmwarePartNumber, gatewayFirmwarePartNumber, radioFirmwarePartNumber, firmwareVersion, networkManagerFirmware, cellularRadioFirmware, satelliteRadioFirmware, ecmGatewayHardwarePartNumber, cellModemIMEI;
						moduleType = dataLinkType = firmwarePartNumber = gatewayFirmwarePartNumber = radioFirmwarePartNumber = firmwareVersion = networkManagerFirmware = cellularRadioFirmware = satelliteRadioFirmware = ecmGatewayHardwarePartNumber = cellModemIMEI = null;

						string deviceDetailsXML = deviceFinalData.Select(e => e.DeviceDetailsXML).FirstOrDefault();
						var deviceTypeID = deviceFinalData.Select(e => e.DeviceTypeID).FirstOrDefault();

						if (PLDeviceTypeIDList.Contains(deviceTypeID))
						{
							if (!string.IsNullOrWhiteSpace(deviceDetailsXML) && deviceDetailsXML.Contains("moduleType") && deviceDetailsXML.Contains("SoftwareInfo"))
							{
								pldata = new PLConfigData(deviceDetailsXML);
								moduleType = pldata.CurrentGeneralRegistry.ModuleType;

								var softwareInfoElement = pldata.CurrentGeneralRegistry.Software;
								if (softwareInfoElement != null)
								{
									gatewayFirmwarePartNumber = softwareInfoElement.HC11SoftwarePartNumber != null
																				? softwareInfoElement.HC11SoftwarePartNumber
																				: string.Empty;
									radioFirmwarePartNumber = softwareInfoElement.ModemSoftwarePartNumber != null
																			  ? softwareInfoElement.ModemSoftwarePartNumber
																			  : string.Empty;
								}
							}
						}
						else
						{
							var tempPartNumber = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Gateway));
							if (tempPartNumber != null)
							{
								string deviceFirmwarePartNumber = tempPartNumber.Value;
								if (!string.IsNullOrWhiteSpace(deviceFirmwarePartNumber))
								{
									var partnumbers = deviceFirmwarePartNumber.Split(' ');
									//radioFirmwarePartNumber = deviceFirmwarePartNumber.Substring(0, deviceFirmwarePartNumber.Split(' ')[0].Length).Trim();
									//char[] charArray = deviceFirmwarePartNumber.ToCharArray();
									//Array.Reverse(charArray);
									//string revPartNumber = new string(charArray);
									//gatewayFirmwarePartNumber = deviceFirmwarePartNumber.Substring(deviceFirmwarePartNumber.Split(' ')[0].Length, deviceFirmwarePartNumber.Length - revPartNumber.Split()[0].Length - deviceFirmwarePartNumber.Split()[0].Length).Trim();
									if (partnumbers.Length == 3)
									{
										ecmGatewayHardwarePartNumber = partnumbers[0];
										gatewayFirmwarePartNumber = partnumbers[1];
									}
								}
							}
						}

						var tempVersion = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software));
						if (tempVersion != null)
						{
							string deviceFirmwareVersion = tempVersion.Value;
							if (!string.IsNullOrWhiteSpace(deviceFirmwareVersion))
							{
								var firmwareMsgs = deviceFirmwareVersion.Split(' ');
								firmwareVersion = firmwareMsgs[0];
								firmwarePartNumber = (firmwareMsgs.Length > 2) ? firmwareMsgs[2] : null;
							}
						}

						var tempVersion2 = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.CellularRadioFirmware));
						if (tempVersion2 != null)
						{
							cellularRadioFirmware = tempVersion2.Value;
						}

						var tempVersion3 = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SatelliteRadioFirmware));
						if (tempVersion3 != null)
						{
							satelliteRadioFirmware = tempVersion3.Value;
						}

						var tempVersion4 = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.NetworkManagerFirmware));
						if (tempVersion4 != null)
						{
							networkManagerFirmware = tempVersion4.Value;
						}

						if (deviceTypeID == (int)DeviceTypeEnum.TrimTrac)
						{
							cellModemIMEI = deviceFinalData.Select(e => e.GpsDeviceID).First();
						}
						else
						{
							var tempVersion5 = dptValues.FirstOrDefault(e => (e != null && e.fk_PersonalityTypeID == (int)PersonalityTypeEnum.GSN));
							if (tempVersion5 != null)
							{
								cellModemIMEI = tempVersion5.Value;
							}
						}

						if (PLDeviceTypeIDList.Contains(deviceTypeID))
						{
							//PL device type; DataLink names could be different, but ecmIsDevice is not set
							var edmNames = deviceFinalData.Select(e => e.datalinkName).Distinct().ToList();
							dataLinkType = EcmNameStr(edmNames);
						}
						else
						{
							var edmNames = deviceFinalData.Where(e => (e.ecmIsDevice != null && (bool)e.ecmIsDevice)).Select(e => e.datalinkName).Distinct().ToList();
							dataLinkType = EcmNameStr(edmNames);
						}

						bool isMTSDeviceType = MTSDeviceTypeIDList.Contains((int)deviceTypeID);


						var updateDeviceEvent = new UpdateDeviceEvent
						{
							DeviceUID = deviceFinalData.Select(e => (Guid)e.DeviceUID).First(),
							OwningCustomerUID = deviceFinalData.Select(e => e.OwningCustomerUID).First(),
							DeviceSerialNumber = deviceFinalData.Select(e => e.GpsDeviceID).First(),
							DeviceType = deviceFinalData.Select(e => e.DeviceType).First(),
							DeviceState = deviceFinalData.Select(e => e.DeviceState).First(),
							DeregisteredUTC = deviceFinalData.Select(e => e.DeregisteredUTC).First(),
							ModuleType = isMTSDeviceType ? moduleType : null,
							MainboardSoftwareVersion = isMTSDeviceType ? firmwareVersion : null,
							FirmwarePartNumber = isMTSDeviceType ? firmwarePartNumber : null,
							RadioFirmwarePartNumber = isMTSDeviceType ? radioFirmwarePartNumber : null,
							GatewayFirmwarePartNumber = isMTSDeviceType ? gatewayFirmwarePartNumber : null,
							DataLinkType = isMTSDeviceType ? dataLinkType : null,
							CellModemIMEI = isMTSDeviceType ? cellModemIMEI : null,
							DevicePartNumber = isMTSDeviceType ? ecmGatewayHardwarePartNumber : null,
							CellularFirmwarePartnumber = isMTSDeviceType ? cellularRadioFirmware : null,
							NetworkFirmwarePartnumber = isMTSDeviceType ? networkManagerFirmware : null,
							SatelliteFirmwarePartnumber = isMTSDeviceType ? satelliteRadioFirmware : null,
							ActionUTC = DateTime.UtcNow
						};

                        var svcResponse = ProcessServiceRequestAndResponse<UpdateDeviceEvent>(updateDeviceEvent, _httpRequestWrapper, DeviceApiEndPointUri, requestHeader, HttpMethod.Put);

                        if (svcResponse.StatusCode == HttpStatusCode.OK || svcResponse.StatusCode == HttpStatusCode.BadRequest)
                        {
                            lastUpdateUtc = minUtc;
                        }
                        else
                        {
                            Log.IfError($"Device Update event failed for {updateDeviceEvent.DeviceUID}, StatusCode : {svcResponse.StatusCode} , payload : {JsonHelper.SerializeObjectToJson(updateDeviceEvent)}");
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
					Log.IfInfo(string.Format("Completed Processing UpdateDeviceEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
				}
			}
			return true;
		}

		private DateTime GetMinUtc(DateTime utc, DateTime utc1, DateTime utc2)
		{
			return (DateTime.Compare(utc, utc1) < 0) ? (DateTime.Compare(utc, utc2) < 0 ? utc : utc2) : (DateTime.Compare(utc1, utc2) < 0 ? utc1 : utc2);
		}

		private static string EcmNameStr(IReadOnlyList<string> names)
		{
			if (names == null)
				return null;

			var nameCount = names.Count;

			if (nameCount == 0)
				return null;

			var retval = names[0];

			if (nameCount > 1)
			{
				for (var i = 1; i < nameCount; i++)
					retval += string.Format(" {0}", names[i]);
			}
			return retval;
		}

		private class ECMInfoDevice
		{
			public long DeviceID { get; set; }
		}
	}
}
