using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.AssetRepository
{
	public class AssetServices : IAssetServices
	{
		private readonly ITransactions _transaction;
		private readonly IConfiguration _configuration;
		private ILogger _logger;
		private readonly List<string> assetTopicNames;
		public readonly string connectionString;

		public AssetServices(ITransactions transactions, IConfiguration configuration, ILogger logger)
		{
			_transaction = transactions;
			_configuration = configuration;
			_logger = logger;
			assetTopicNames = _configuration["AssetTopicNames"].Split(',').ToList();
			connectionString = _configuration["ConnectionString:MasterData"];
		}

		#region Asset CUD

		public bool CreateAsset(CreateAssetEvent asset)
		{
			try
			{
				var owningCustomerUID = asset.OwningCustomerUID.HasValue ?
					asset.OwningCustomerUID.Value : new Guid();

				DateTime actionUTC = DateTime.UtcNow;

				//Db Object
				var assetObject = new DbModel.AssetPayload
				{
					AssetUID = new Guid(asset.AssetUID.ToString()),
					OwningCustomerUID = owningCustomerUID,
					AssetName = string.IsNullOrWhiteSpace(asset.AssetName) ? null : asset.AssetName,
					LegacyAssetID = asset.LegacyAssetID == -9999999 ? 0 : asset.LegacyAssetID,
					SerialNumber = asset.SerialNumber,
					MakeCode = asset.MakeCode,
					Model = string.IsNullOrWhiteSpace(asset.Model) ? null : asset.Model,
					AssetTypeName = string.IsNullOrWhiteSpace(asset.AssetType) ? null : asset.AssetType,
					IconKey = asset.IconKey == -9999999 ? null : asset.IconKey,
					EquipmentVIN = string.IsNullOrWhiteSpace(asset.EquipmentVIN) ? null : asset.EquipmentVIN,
					ModelYear = asset.ModelYear == -9999999 ? null : asset.ModelYear,
					InsertUTC = actionUTC,
					UpdateUTC = actionUTC,
					StatusInd = true,
					ObjectType = asset.ObjectType,
					Category = asset.Category,
					ProjectStatus = asset.ProjectStatus,
					SortField = asset.SortField,
					Source = asset.Source,
					UserEnteredRuntimeHours = asset.UserEnteredRuntimeHours,
					Classification = asset.Classification,
					PlanningGroup = asset.PlanningGroup
				};

				var actions = new List<Action>()
					{
						() => _transaction.Upsert(assetObject),
						() => _transaction.Publish(GetGetAssetSourceSnapshotTopublish(assetObject, asset.ReceivedUTC.Value, false))
					};
				return _transaction.Execute(actions);
			}
			catch (MySqlException ex)
			{
				_logger.LogError("error while creating asset in db: ", ex);
				throw ex;
			}
			finally
			{
			}
		}

		public virtual bool UpdateAsset(UpdateAssetEvent asset)
		{
			try
			{
				//Db Object
				var _asset = GetAsset(asset.AssetUID.Value);

				if (_asset == null || _asset.AssetUID == null)
					return false;   // Asset Not Found

				if (_asset.OwningCustomerUID == null && asset.OwningCustomerUID == null)
				{
					asset.OwningCustomerUID = new Guid();
				}
				else if (asset.OwningCustomerUID == null)
				{
					asset.OwningCustomerUID = new Guid(_asset.OwningCustomerUID);
				}

				DateTime actionUTC = DateTime.UtcNow;

				var assetUpdate = new AssetPayload
				{
					//Un-updatable columns: Added an ignored columns
					SerialNumber = _asset.SerialNumber,
					MakeCode = _asset.MakeCode,
					InsertUTC = _asset.InsertUTC,
					StatusInd = _asset.StatusInd == 1 ? true : false,
					UpdateUTC = actionUTC,

					//updatable columns
					AssetUID = asset.AssetUID.Value,
					OwningCustomerUID = asset.OwningCustomerUID,

					LegacyAssetID = asset.LegacyAssetID == null ? (long)_asset.LegacyAssetID : (long)asset.LegacyAssetID,
					ModelYear = asset.ModelYear == -9999999 ? null : (asset.ModelYear == null ? _asset.ModelYear : asset.ModelYear),
					IconKey = asset.IconKey == -9999999 ? null : (asset.IconKey == null ? _asset.IconKey : asset.IconKey),

					AssetName = asset.AssetName == null ? _asset.AssetName : (string.IsNullOrWhiteSpace(asset.AssetName) ? null : asset.AssetName),
					Model = asset.Model == null ? _asset.Model : (string.IsNullOrWhiteSpace(asset.Model) ? null : asset.Model),
					AssetTypeName = asset.AssetType == null ? _asset.AssetType : (string.IsNullOrWhiteSpace(asset.AssetType) ? null : asset.AssetType),
					EquipmentVIN = asset.EquipmentVIN == null ? _asset.EquipmentVIN : (string.IsNullOrWhiteSpace(asset.EquipmentVIN) ? null : asset.EquipmentVIN),
					ObjectType = asset.ObjectType == null ? _asset.ObjectType : (string.IsNullOrWhiteSpace(asset.ObjectType) ? null : asset.ObjectType),
					Category = asset.Category == null ? _asset.Category : (string.IsNullOrWhiteSpace(asset.Category) ? null : asset.Category),
					ProjectStatus = asset.ProjectStatus == null ? _asset.ProjectStatus : (string.IsNullOrWhiteSpace(asset.ProjectStatus) ? null : asset.ProjectStatus),
					SortField = asset.SortField == null ? _asset.SortField : (string.IsNullOrWhiteSpace(asset.SortField) ? null : asset.SortField),
					Source = asset.Source == null ? _asset.Source : (string.IsNullOrWhiteSpace(asset.Source) ? null : asset.Source),
					UserEnteredRuntimeHours = asset.UserEnteredRuntimeHours == null ? _asset.UserEnteredRuntimeHours : (string.IsNullOrWhiteSpace(asset.UserEnteredRuntimeHours) ? null : asset.UserEnteredRuntimeHours),
					Classification = asset.Classification == null ? _asset.Classification : (string.IsNullOrWhiteSpace(asset.Classification) ? null : asset.Classification),
					PlanningGroup = asset.PlanningGroup == null ? _asset.PlanningGroup : (string.IsNullOrWhiteSpace(asset.PlanningGroup) ? null : asset.PlanningGroup),
				};

				var actions = new List<Action>()
					{
						() => _transaction.Upsert(assetUpdate),
						() => _transaction.Publish(GetGetAssetSourceSnapshotTopublish(assetUpdate, asset.ReceivedUTC.Value, true))
					};
				return _transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				_logger.LogError("error while Upadting asset in db: ", ex);
				throw ex;
			}
		}

		public virtual bool DeleteAsset(DeleteAssetPayload asset)
		{
			string guid = asset.AssetUID.Value.ToStringWithoutHyphens();
			DateTime actionUTC = DateTime.UtcNow;
			string query = $"update md_asset_Asset set StatusInd = 0, UpdateUTC = '{actionUTC.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' where AssetUID = {guid.WrapWithUnhex()}";
			//Kafka message
			asset.ActionUTC = actionUTC;
			var message = new KafkaMessage
			{
				Key = asset.AssetUID.ToString(),
				Message = new { DeleteAssetEvent = asset }
			};

			var actions = new List<Action>()
					{
						() => _transaction.Delete(query),
						() => assetTopicNames?.ForEach((topic)=>{message.Topic=topic; _transaction.Publish(message);})
					};
			return _transaction.Execute(actions);
		}

		private List<KafkaMessage> GetGetAssetSourceSnapshotTopublish(dynamic assetDetails, DateTime receivedUTC, bool isAssetUpdate = false)
		{
			var kafkaMessageList = new List<KafkaMessage>();
			if (isAssetUpdate)
			{

				assetTopicNames.ToList().ForEach(topic =>
				{
					KafkaMessage kafkaMessage = new KafkaMessage()
					{
						Key = assetDetails.AssetUID.ToString(),
						Message = new
						{
							UpdateAssetEvent = new
							{
								AssetUID = assetDetails.AssetUID,
								LegacyAssetID = assetDetails.LegacyAssetID,
								Model = assetDetails.Model,
								AssetType = assetDetails.AssetTypeName,
								IconKey = assetDetails.IconKey,
								EquipmentVIN = assetDetails.EquipmentVIN,
								ModelYear = assetDetails.ModelYear,
								OwningCustomerUID = assetDetails.OwningCustomerUID,
								ObjectType = assetDetails.ObjectType,
								Category = assetDetails.Category,
								ProjectStatus = assetDetails.ProjectStatus,
								SortField = assetDetails.SortField,
								UserEnteredRuntimeHours = assetDetails.UserEnteredRuntimeHours,
								Classification = assetDetails.Classification,
								PlanningGroup = assetDetails.PlanningGroup,
								Source = assetDetails.Source,
								AssetName = assetDetails.AssetName,
								ActionUTC = assetDetails.UpdateUTC,
								ReceivedUTC = receivedUTC,
							}
						},
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});
			}
			else
			{
				var messagePayload = AssetSnapshotToPublish(assetDetails);
				messagePayload.ReceivedUTC = receivedUTC;
				messagePayload.ActionUTC = assetDetails.InsertUTC;
				assetTopicNames.ToList().ForEach(topic =>
				{
					KafkaMessage kafkaMessage = new KafkaMessage()
					{
						Key = assetDetails.AssetUID.ToString(),
						Message = new { CreateAssetEvent = messagePayload },
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});
			}
			return kafkaMessageList;
		}

		private UpdateAssetPayload AssetSnapshotToPublish(dynamic assetDetails)
		{
			return new UpdateAssetPayload()
			{
				SerialNumber = assetDetails.SerialNumber,
				MakeCode = assetDetails.MakeCode,
				AssetUID = assetDetails.AssetUID,
				LegacyAssetID = assetDetails.LegacyAssetID,
				Model = assetDetails.Model,
				AssetType = assetDetails.AssetTypeName,
				IconKey = assetDetails.IconKey,
				EquipmentVIN = assetDetails.EquipmentVIN,
				ModelYear = assetDetails.ModelYear,
				OwningCustomerUID = assetDetails.OwningCustomerUID,
				ObjectType = assetDetails.ObjectType,
				Category = assetDetails.Category,
				ProjectStatus = assetDetails.ProjectStatus,
				SortField = assetDetails.SortField,
				UserEnteredRuntimeHours = assetDetails.UserEnteredRuntimeHours,
				Classification = assetDetails.Classification,
				PlanningGroup = assetDetails.PlanningGroup,
				ActionUTC = assetDetails.UpdateUTC,
				ReceivedUTC = assetDetails.UpdateUTC,
				Source = assetDetails.Source,
				AssetName = assetDetails.AssetName
			};
		}

		public ClientModel.Asset GetAsset(Guid assetUID)
		{
			try
			{
				string assetQuery = $"SELECT Hex(AssetUID) As AssetUID, AssetName, LegacyAssetID, SerialNumber, MakeCode, Model, AssetTypeName As AssetType, IconKey, " +
					$"EquipmentVIN, ModelYear, StatusInd, Hex(OwningCustomerUID) As OwningCustomerUID, ObjectType, Category, ProjectStatus, " +
					$"SortField, Source, UserEnteredRuntimeHours, Classification, PlanningGroup, InsertUTC, UpdateUTC " +
					$"FROM md_asset_Asset WHERE AssetUID = Unhex('{assetUID.ToString("N")}')";
				return _transaction.Get<ClientModel.Asset>(assetQuery).FirstOrDefault();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving AssetDetail By Asset Guid for Application from database. {ex}"); ;
				throw ex;
			}
			finally
			{
			}
		}

		public Guid? GetAssetUid(Guid assetGuid, string make, string sno)
		{
			// Persist Pattern Value will be Yes or Allow Duplicates if Allow Duplicates Assetuid check has to removed checked along with make serial number

			var queryFormat = $"select hex(AssetUID) from md_asset_Asset where (AssetUID = {assetGuid.ToStringWithoutHyphens().WrapWithUnhex()} or (MakeCode = '{MySqlHelper.EscapeString(make)}' and SerialNumber = '{MySqlHelper.EscapeString(sno)}')) and StatusInd = 1";

			Guid? assetExistingGuid;
			string uid = _transaction.Get<string>(queryFormat).FirstOrDefault();
			assetExistingGuid = !string.IsNullOrEmpty(uid) ? new Guid(uid) : (Guid?)null;
			return assetExistingGuid;
		}
		public List<Guid> GetCustomersForApplication(string appName)
		{
			try
			{
				var customerUidStrings = _transaction.Get<string>($"select hex(fk_CustomerUID) from md_customer_CustomerApplication where `ApplicationCode` ='{MySqlHelper.EscapeString(appName)}'").ToList();
				return customerUidStrings.Select(x => new Guid(x)).ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving Customers for Application from database. {ex}"); ;
				throw ex;
			}
		}

		#endregion CUD

		#region reaD

		public bool IsValidMakeCode(string makeCode)
		{
			if (string.IsNullOrEmpty(makeCode)) return false;
			string make;
			try
			{
				var getMakeQuery = $"Select distinct Upper(Code) from msg_md_make_Make where Code = '{MySqlHelper.EscapeString(makeCode.ToUpper())}' LIMIT 1;";
				make = _transaction.Get<string>(getMakeQuery).FirstOrDefault();
				if (make != null)
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving make code from database. {ex}");
				return false;
			}
		}
		public bool ValidateAuthorizedCustomerByAsset(Guid assetGuid, Guid userGuid)
		{
			var ValidateAuthorizedCustomerAssetQuery = $"SELECT  count(1) FROM md_customer_CustomerUser CU " +
														$"INNER JOIN " +
														$"md_customer_CustomerAsset CA ON CA.fk_CustomerUID = CU.fk_CustomerUID " +
														$"WHERE " +
														$"CU.fk_UserUID = UNHEX('{userGuid.ToStringWithoutHyphens()}') " +
														$"AND CA.fk_AssetUID = UNHEX('{assetGuid.ToStringWithoutHyphens()}')";
			var isAvail = _transaction.Get<string>(ValidateAuthorizedCustomerAssetQuery).FirstOrDefault();

			return Convert.ToInt32(isAvail) > 0;
		}

		public List<LegacyAssetData> GetAssetByAssetLegacyID(Guid userGuid, long legacyAssetID = 0, Guid? AssetUID = null, long? pageNumber = 1, int? pageSize = 100, string makeCode = null, string serialNumber = null)
		{
			try
			{
				if(makeCode != null )
					makeCode =  MySqlHelper.EscapeString(makeCode);
				if (serialNumber != null)
					serialNumber = MySqlHelper.EscapeString(serialNumber);

				string where = AssetUID != null && AssetUID.HasValue ?
				$" and a.AssetUID = {((Guid)AssetUID).ToStringWithoutHyphens().WrapWithUnhex()} " : legacyAssetID > 0 ? $" and a.LegacyAssetID = {legacyAssetID}" : string.Empty;

				where = !string.IsNullOrEmpty(makeCode) && !string.IsNullOrEmpty(serialNumber) ? $" and a.MakeCode = '{makeCode}' and a.SerialNumber = '{serialNumber}'" : where;

				string GetAssetQueryByLegacyAssetID = "select hex(a.AssetUID) as AssetUID, a.AssetName, a.LegacyAssetID, a.SerialNumber, a.MakeCode, a.Model, a.AssetTypeName, a.EquipmentVIN, a.IconKey, a.ModelYear, a.StatusInd,a.AssetTypeName as ProductFamily,a.makecode as MakeName,a.LegacyAssetID, d.DeviceType,d.DeviceSerialNumber from md_asset_Asset a inner join md_customer_CustomerAsset ca on ca.fk_AssetUID = a.AssetUID inner join md_customer_CustomerUser cu on cu.fk_CustomerUID = ca.fk_CustomerUID  left outer join (select dt.TypeName AS DeviceType, d1.SerialNumber AS DeviceSerialNumber, ad1.fk_AssetUID from md_asset_Asset a1 inner join md_asset_AssetDevice ad1 on ad1.fk_AssetUID = a1.AssetUID inner join md_device_Device d1 on d1.DeviceUID = ad1.fk_DeviceUID   JOIN md_device_DeviceType dt ON dt.DeviceTypeID = d1.fk_DeviceTypeID where ad1.RowUpdatedUTC = (select max(ad2.RowUpdatedUTC) from md_asset_AssetDevice ad2 where ad2.fk_AssetUID = a1.AssetUID and ad2.fk_DeviceUID = d1.DeviceUID)) d on d.fk_AssetUID = a.AssetUID where a.StatusInd=1 and cu.fk_UserUID= {0}";
				string query = string.Format(GetAssetQueryByLegacyAssetID, userGuid.ToStringWithoutHyphens().WrapWithUnhex()) + where + $" limit {(pageNumber - 1) * pageSize},{pageSize + 1}";

				var assetRecords = _transaction.Get<LegacyAssetData>(query);
				return assetRecords != null ? assetRecords.ToList() : null;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving AssetsByLegacyID from database. {ex}"); ;
				throw;
			}
		}

		public List<DbModel.Asset> GetAssets(Guid[] assetGuids, Guid userGuid)
		{
			var batches = assetGuids.Batch(1000);
			var assets = new List<DbModel.Asset>();
			try
			{
				foreach (var batch in batches)
				{
					assets.AddRange(_transaction.Get<DbModel.Asset>(Get(userGuid.ToStringWithoutHyphens(), batch)));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving Assets from database. {ex}"); ;
				throw;
			}
			finally
			{

			}
			return assets;
		}

		private string Get(string userUid, IEnumerable<Guid> guids)
		{
			List<string> guidString = guids.Select(x => x.ToStringWithoutHyphens().WrapWithUnhex()).ToList();

			return $"select hex(a.AssetUID) as AssetUID, a.AssetName, a.LegacyAssetID, a.SerialNumber, a.MakeCode, a.Model, a.AssetTypeName, a.EquipmentVIN, a.IconKey, a.ModelYear, a.StatusInd " +
				$" from md_asset_Asset a join md_customer_CustomerAsset ca " +
				$" on ca.fk_AssetUID = a.AssetUID join md_customer_CustomerUser cu on cu.fk_CustomerUID = ca.fk_CustomerUID" +
				$" where a.StatusInd=1 and  cu.fk_UserUID=" + userUid.WrapWithUnhex() + " and a.AssetUID in (" + string.Join(",", guidString) + ")";
		}

		public List<object> GetHarvesterAssets()
		{
			var assets = new List<DbModel.Asset>();
			var assetsToBeReturned = new List<object>();
			var query = $"select distinct hex(a.AssetUID) as AssetUID,a.SerialNumber,a.MakeCode from md_asset_Asset a join md_asset_AssetDevice ad on a.AssetUID = ad.fk_AssetUID join md_device_Device d on d.DeviceUID = ad.fk_DeviceUID where a.StatusInd = 1 and d.fk_DeviceTypeID = 36";
			try
			{
				assets.AddRange(_transaction.Get<DbModel.Asset>(query));
				foreach (var asset in assets)
				{
					assetsToBeReturned.Add(new
					{
						SerialNumber = asset.SerialNumber,
						MakeCode = asset.MakeCode,
						AssetUID = Guid.Parse(asset.AssetUID)
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error retrieving HarvesterAssets from database. {ex}"); ;
				throw;
			}
			finally
			{

			}
			return assetsToBeReturned;
		}

		public object GetAssetDetail(Guid? assetUID, Guid? deviceUID = null)
		{
			var assets = new List<AssetDetail>();
			string guidString = string.Empty;
			var query = "";
			if (assetUID != null && deviceUID != null)
			{
				guidString = ((Guid)assetUID).ToStringWithoutHyphens().WrapWithUnhex();
				string deviceGuidString = ((Guid)deviceUID).ToStringWithoutHyphens().WrapWithUnhex();
				query =
					$"SELECT hex(a.AssetUID) as AssetUID,a.AssetName,a.SerialNumber,a.MakeCode,a.Model," +
					$" a.AssetTypeName,a.ModelYear,hex(a.OwningCustomerUID) as OwningCustomerUID, a.UpdateUTC as TimestampOfModification,d.SerialNumber as DeviceSerialNumber," +
					$" dt.TypeName as DeviceType,d.fk_DeviceStatusID  as DeviceState ,hex(d.DeviceUID) as DeviceUID ,group_concat(hex(ac.fk_CustomerUID) separator ',')  as AssetCustomerUID FROM " +
					" md_asset_Asset a " +
					$" left outer join md_customer_CustomerAsset ac on ac.fk_AssetUID = a.AssetUID " +
					$" left outer join md_asset_AssetDevice ad on ad.fk_AssetUID = a.AssetUID " +
					$" left outer join md_device_Device d on d.DeviceUID = ad.fk_DeviceUID " +
					$" Join md_device_DeviceType dt on dt.DeviceTypeID = d.fk_DeviceTypeID" +
					$" where a.StatusInd =1 and a.AssetUID = {guidString} group by a.AssetUID" +
					" union " +
					$"SELECT hex(a.AssetUID) as AssetUID,a.AssetName,a.SerialNumber,a.MakeCode,a.Model," +
					$" a.AssetTypeName,a.ModelYear,hex(a.OwningCustomerUID)  as OwningCustomerUID,a.UpdateUTC as TimestampOfModification,d.SerialNumber as DeviceSerialNumber," +
					$" dt.TypeName as DeviceType,d.fk_DeviceStatusID  as DeviceState,hex(d.DeviceUID) as DeviceUID ,group_concat(hex(ac.fk_CustomerUID) separator ',')  as AssetCustomerUID FROM " +
					" md_device_Device d " +
					$" left outer join md_asset_AssetDevice ad on ad.fk_DeviceUID = d.DeviceUID " +
					$" left outer join md_asset_Asset a on a.AssetUID = ad.fk_AssetUID and a.StatusInd=1" +
					$" left outer join md_customer_CustomerAsset ac on ac.fk_AssetUID = a.AssetUID " +
					$" Join md_device_DeviceType dt on dt.DeviceTypeID = d.fk_DeviceTypeID" +
					$" where  d.DeviceUID = {deviceGuidString} group by a.AssetUID";
			}
			else if (assetUID != null)
			{
				guidString = ((Guid)assetUID).ToStringWithoutHyphens().WrapWithUnhex();
				query =
					$"SELECT hex(a.AssetUID) as AssetUID,a.AssetName,a.SerialNumber,a.MakeCode,a.Model," +
					$" a.AssetTypeName,a.ModelYear,hex(a.OwningCustomerUID)  as OwningCustomerUID,a.UpdateUTC as TimestampOfModification,d.SerialNumber as DeviceSerialNumber," +
					$" dt.TypeName as DeviceType, d.fk_DeviceStatusID  as DeviceState,hex(d.DeviceUID) as DeviceUID ,group_concat(hex(ac.fk_CustomerUID) separator ',')  as AssetCustomerUID FROM " +
					" md_asset_Asset a " +
					$" left outer join md_customer_CustomerAsset ac on ac.fk_AssetUID = a.AssetUID " +
					$" left outer join md_asset_AssetDevice ad on ad.fk_AssetUID = a.AssetUID " +
					$" left outer join md_device_Device d on d.DeviceUID = ad.fk_DeviceUID " +
					$" Join md_device_DeviceType dt on dt.DeviceTypeID = d.fk_DeviceTypeID " +
					$" where a.StatusInd=1 and a.AssetUID = {guidString} group by a.AssetUID";
			}
			else if (deviceUID != null)
			{
				guidString = ((Guid)deviceUID).ToStringWithoutHyphens().WrapWithUnhex();
				query =
					$"SELECT hex(a.AssetUID) as AssetUID,a.AssetName,a.SerialNumber,a.MakeCode,a.Model," +
					$" a.AssetTypeName,a.ModelYear,hex(a.OwningCustomerUID)  as OwningCustomerUID,a.UpdateUTC as TimestampOfModification,d.SerialNumber as DeviceSerialNumber," +
					$" dt.TypeName as DeviceType,d.fk_DeviceStatusID  as DeviceState,hex(d.DeviceUID) as DeviceUID ,group_concat(hex(ac.fk_CustomerUID) separator ',')  as AssetCustomerUID FROM " +
					" md_device_Device d " +
					$" left outer join md_asset_AssetDevice ad on ad.fk_DeviceUID = d.DeviceUID " +
					$" left outer join md_asset_Asset a on a.AssetUID = ad.fk_AssetUID and a.StatusInd=1" +
					$" left outer join md_customer_CustomerAsset ac on ac.fk_AssetUID = a.AssetUID " +
					$" Join md_device_DeviceType dt on dt.DeviceTypeID = d.fk_DeviceTypeID" +
					$" where  d.DeviceUID = {guidString} group by a.AssetUID";
			}

			try
			{
				assets.AddRange(_transaction.Get<AssetDetail>(query));
			}
			catch (Exception ex)
			{
				_logger.LogError($"{ex}");
				throw;
			}
			finally
			{
			}
			return assets != null && assets.Count > 0 ? assets : null;
		}

		public object GetAssetsforSupportUser(string searchString, int pageNum, int pageLimit)
		{
			var assets = new List<object>();

			var assetsToBeReturned = new DbModel.AssetDeviceForSupportUserListD();
			int rowToStart = (pageNum - 1) * pageLimit;
			int noOfRows = pageLimit;

			if (searchString!=null)
				searchString = MySqlHelper.EscapeString(searchString);

			var query = $"SELECT DISTINCT " +
						$"HEX(a.AssetUID) AS AssetUID, " +
						$"a.AssetName AS AssetName, " +
						$"a.SerialNumber AS AssetSerialNumber, " +
						$"a.MakeCode AS AssetMakeCode, " +
						$"IFNULL(HEX(d.DeviceUID), ' - ') AS DeviceUID, " +
						$"IFNULL(d.SerialNumber, ' - ') AS DeviceSerialNumber, " +
						$"IFNULL(dt.TypeName, ' - ') AS DeviceType " +
						$"FROM " +
						$"md_asset_Asset a " +
							$"LEFT OUTER JOIN " +
						$"md_asset_AssetDevice ad ON a.AssetUID = ad.fk_AssetUID " +
							$"LEFT OUTER JOIN " +
						$"md_device_Device d ON d.DeviceUID = ad.fk_DeviceUID " +
							$"JOIN " +
						$"md_device_DeviceType dt ON dt.DeviceTypeID = d.fk_DeviceTypeID " +
						$"WHERE " +
						$"a.StatusInd = 1 " +
							$"AND (a.AssetName LIKE '%{searchString}%' " +
							$"OR a.SerialNumber LIKE '%{searchString}%')  " +
					$"UNION SELECT DISTINCT " +
						$"IFNULL(HEX(a.AssetUID), ' - ') AS AssetUID, " +
						$"IFNULL(a.AssetName, ' - ') AS AssetName, " +
						$"IFNULL(a.SerialNumber, ' - ') AS AssetSerialNumber, " +
						$"IFNULL(a.MakeCode, ' - ') AS AssetMakeCode, " +
						$"HEX(d.DeviceUID) AS DeviceUID, " +
						$"d.SerialNumber AS DeviceSerialNumber, " +
						$"dt.TypeName AS DeviceType " +
						$"FROM " +
						$"md_device_Device d " +
							$"LEFT OUTER JOIN " +
						$"md_asset_AssetDevice ad ON d.DeviceUID = ad.fk_DeviceUID  " +
							$"LEFT OUTER JOIN " +
						$"md_asset_Asset a ON a.AssetUID = ad.fk_AssetUID " +
							$"AND a.StatusInd = 1 " +
							$"JOIN " +
						$"md_device_DeviceType dt ON dt.DeviceTypeID = d.fk_DeviceTypeID " +
					$"WHERE " +
						$"d.SerialNumber LIKE '%{searchString}%' " +
					$"ORDER BY AssetSerialNumber " +
					$"LIMIT {rowToStart}, {noOfRows}; " +
					$"SELECT FOUND_ROWS();";
			try
			{
				var resultSet = _transaction.GetMultipleResultSetAsync<dynamic, int>(query, null).Result;

				var lstAssets = resultSet.Item1 as IList<dynamic>;
				var assetFoundRows = (resultSet.Item2 as IList<int>);
				for (int i = 0; i < lstAssets.Count; i++)
				{
					AssetDeviceForSupportUserD asset = new AssetDeviceForSupportUserD();
					asset.AssetMakeCode = lstAssets[i].AssetMakeCode != null ? lstAssets[i].AssetMakeCode.ToString() : null;
					asset.AssetName = lstAssets[i].AssetName != null ? lstAssets[i].AssetName.ToString() : null;
					asset.AssetSerialNumber = lstAssets[i].AssetSerialNumber != null ? lstAssets[i].AssetSerialNumber.ToString() : null;
					asset.AssetUID = lstAssets[i].AssetUID != null && lstAssets[i].AssetUID.ToString().Trim() != "-" ? (Guid?)new Guid(lstAssets[i].AssetUID.ToString()) : null;
					asset.DeviceSerialNumber = lstAssets[i].DeviceSerialNumber != null ? lstAssets[i].DeviceSerialNumber.ToString() : null;
					asset.DeviceType = lstAssets[i].DeviceType != null ? lstAssets[i].DeviceType.ToString() : null;
					asset.DeviceUID = lstAssets[i].DeviceUID != null && lstAssets[i].DeviceUID.ToString().Trim() != "-" ? (Guid?)new Guid(lstAssets[i].DeviceUID.ToString()) : null;
					assets.Add(asset);
				}
				assetsToBeReturned.AssetDevices = assets.Select(x => (AssetDeviceForSupportUserD)x).ToList();
				assetsToBeReturned.TotalNumberOfPages = (int)Math.Ceiling(Convert.ToDouble(Convert.ToDouble(assetFoundRows.FirstOrDefault()) / pageLimit));
				assetsToBeReturned.PageNumber = assetsToBeReturned.TotalNumberOfPages > 0 ? pageNum : 0;
			}
			catch (Exception ex)
			{
				_logger.LogError($"{ex}");
				throw;
			}
			finally
			{
			}
			return assetsToBeReturned;
		}
		public CustomerAssetsListData GetAssetsForCustomer(List<Guid> customerGuids, int pageNum, int pageLimit)
		{
			var assetsToBeReturned = new CustomerAssetsListData();
			using (var connection = new MySqlConnection(connectionString))
			{
				var assets = new List<CustomerAsset>();
				try
				{
					int rowToStart = (pageNum - 1) * pageLimit;
					int noOfRows = pageLimit;

					List<string> lstCustomerGuids = customerGuids.Select(x => x.ToStringWithoutHyphens().WrapWithUnhex()).ToList();

					string GetAssetsForCustomerQuery =
							$"select SQL_CALC_FOUND_ROWS hex(a.AssetUID) as AssetUID, a.AssetName, a.LegacyAssetID, a.SerialNumber, a.MakeCode, a.Model, a.AssetTypeName, a.EquipmentVIN, a.IconKey, a.ModelYear, a.StatusInd,hex(a.OwningCustomerUID) as OwningCustomerUID from md_asset_Asset a " +
							$" inner join md_customer_CustomerAsset ca on ca.fk_AssetUID = a.AssetUID " +
							$" where a.StatusInd=1 and ca.fk_CustomerUID in ({"{0}"}) order by a.SerialNumber  LIMIT {rowToStart}, {noOfRows};select found_rows() as TotalNumberOfRows;";

					string query = string.Format(GetAssetsForCustomerQuery, string.Join(",", lstCustomerGuids));

					var customerAssets = _transaction.Get<CustomerAsset>(query).ToList();
					var totalRows = customerAssets.Count();

					customerAssets.All(x => { x.OwningCustomerUID = x.OwningCustomerUID != null ? (Guid?)new Guid(x.OwningCustomerUID.ToString()) : null; x.AssetUID = x.AssetUID != null ? (Guid?)new Guid(x.AssetUID.ToString()) : null; return true; });

					assetsToBeReturned.CustomerAssets = customerAssets;
					assetsToBeReturned.TotalRowsCount = totalRows > 0 ? totalRows : 0;
					assetsToBeReturned.TotalNumberOfPages = (int)Math.Ceiling(Convert.ToDouble(Convert.ToDouble(totalRows) / pageLimit));
					assetsToBeReturned.PageNumber = assetsToBeReturned.TotalNumberOfPages > 0 ? pageNum : 0;
				}
				catch (Exception ex)
				{
					_logger.LogError($"{ex}");
					throw;
				}
				finally
				{

				}
				return assetsToBeReturned;
			}
		}


		#endregion
	}
}
