using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.AssetRepository
{
	public class AssetOwnerServices : IAssetOwnerServices
	{
		#region Declarations
		
		private readonly ITransactions _transaction;
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly List<string> assetOwnerTopicNames;

		#endregion Declarations

		#region AssetOwner CUD

		public AssetOwnerServices(ITransactions transactions, IConfiguration configuration, ILogger logger)
		{
			_transaction = transactions;
			_configuration = configuration;
			_logger = logger;
			assetOwnerTopicNames = _configuration["AssetOwnerTopicName"].Split(',').ToList();
		}

		public virtual bool CreateAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent) 
		{
			string networkCustomerCode =string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.NetworkCustomerCode) ? null : assetOwnerEvent.AssetOwnerRecord.NetworkCustomerCode;
			string dealerAccountCode = string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.DealerAccountCode) ? null : assetOwnerEvent.AssetOwnerRecord.DealerAccountCode;
			string networkDealerCode = string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.NetworkDealerCode)? null : assetOwnerEvent.AssetOwnerRecord.NetworkDealerCode;
			string accountName = string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.AccountName)? null : assetOwnerEvent.AssetOwnerRecord.AccountName;
			string dealerName = string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.DealerName)? null : assetOwnerEvent.AssetOwnerRecord.DealerName;
			string customerName = string.IsNullOrWhiteSpace(assetOwnerEvent.AssetOwnerRecord.CustomerName)? null : assetOwnerEvent.AssetOwnerRecord.CustomerName;

			Guid? customerUid = assetOwnerEvent.AssetOwnerRecord.CustomerUID == null || assetOwnerEvent.AssetOwnerRecord.CustomerUID == Guid.Empty ? (Guid?)null : assetOwnerEvent.AssetOwnerRecord.CustomerUID;
			Guid? accountUid = assetOwnerEvent.AssetOwnerRecord.AccountUID == null || assetOwnerEvent.AssetOwnerRecord.AccountUID  == Guid.Empty ? (Guid?)null : assetOwnerEvent.AssetOwnerRecord.AccountUID;
			Guid? dealerUid = assetOwnerEvent.AssetOwnerRecord.DealerUID == Guid.Empty ? (Guid?)null : assetOwnerEvent.AssetOwnerRecord.DealerUID;
			try
			{

				var assetOwnerPayload = new AssetOwnerEvent
				{
					AssetUID = assetOwnerEvent.AssetUID,
					AssetOwnerRecord = new ClientModel.AssetOwner
					{
						NetworkCustomerCode = networkCustomerCode,
						DealerAccountCode = dealerAccountCode,
						NetworkDealerCode = networkDealerCode,
						AccountName = accountName,
						DealerName = dealerName,
						DealerUID = dealerUid,
						CustomerName = customerName,
						CustomerUID = customerUid,
						AccountUID = accountUid

					},
					Action = Operation.Create,
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				};

				var assetOwnerDBModel = GetAssetOwnerDBModel(assetOwnerPayload);

				var message = new KafkaMessage
				{
					Key = assetOwnerEvent.AssetUID.ToString(),
					Message = new {
						AssetUID = assetOwnerEvent.AssetUID,
						AssetOwnerRecord = new ClientModel.AssetOwner
						{
							NetworkCustomerCode = networkCustomerCode,
							DealerAccountCode = dealerAccountCode,
							NetworkDealerCode = networkDealerCode,
							AccountName = accountName,
							DealerName = dealerName,
							DealerUID = assetOwnerEvent.AssetOwnerRecord.DealerUID,
							CustomerName = customerName,
							CustomerUID = customerUid,
							AccountUID = accountUid

						},
						Action = "Create",
						ActionUTC = assetOwnerDBModel.UpdateUTC,
						ReceivedUTC = assetOwnerDBModel.UpdateUTC
					}
				};

				var actions = new List<Action>()
					{
						() => _transaction.Upsert(assetOwnerDBModel),
						() => assetOwnerTopicNames?.ForEach((topic)=>{message.Topic=topic; _transaction.Publish(message);})
					};
				_logger.LogInformation($"Create Asset Owner: {JsonSerializer.Serialize(assetOwnerPayload)}");

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

		public virtual bool UpdateAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent) 
		{
			
			var assetOwnerDBModel = GetAssetOwnerDBModel(assetOwnerEvent);

			var message = new KafkaMessage
			{
				Key = assetOwnerEvent.AssetUID.ToString(),
				Message = new {
					AssetUID = assetOwnerEvent.AssetUID,
					AssetOwnerRecord = new ClientModel.AssetOwner
					{
						CustomerName = assetOwnerEvent.AssetOwnerRecord.CustomerName,
						AccountName = assetOwnerEvent.AssetOwnerRecord.AccountName,
						DealerAccountCode = assetOwnerEvent.AssetOwnerRecord.DealerAccountCode,
						DealerUID = assetOwnerEvent.AssetOwnerRecord.DealerUID,
						DealerName = assetOwnerEvent.AssetOwnerRecord.DealerName,
						NetworkCustomerCode = assetOwnerEvent.AssetOwnerRecord.NetworkCustomerCode,
						NetworkDealerCode = assetOwnerEvent.AssetOwnerRecord.NetworkDealerCode,
						CustomerUID = assetOwnerEvent.AssetOwnerRecord.CustomerUID,
						AccountUID = assetOwnerEvent.AssetOwnerRecord.AccountUID
					},
					Action = "Update",
					ActionUTC = assetOwnerDBModel.UpdateUTC, //ToDO: Ensure the Insert UTC values
					ReceivedUTC = assetOwnerDBModel.UpdateUTC
				}
			};

			var actions = new List<Action>()
					{
						() => _transaction.Upsert(assetOwnerDBModel),
						() => assetOwnerTopicNames?.ForEach((topic)=>{message.Topic=topic; _transaction.Publish(message);})
					};
			_logger.LogInformation($"Update Asset Owner: {JsonSerializer.Serialize(assetOwnerEvent)}");
			return _transaction.Execute(actions);
		}

		public virtual bool DeleteAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent)
		{
			string guid = assetOwnerEvent.AssetUID.Value.ToStringWithoutHyphens();
			string query = $"delete from md_asset_AssetOwner where {MySql.ColumnName.AssetOwner.ColumnName.ASSET_UID} = {guid.WrapWithUnhex()}";

			Guid? customerUid = assetOwnerEvent.AssetOwnerRecord.CustomerUID == null || assetOwnerEvent.AssetOwnerRecord.CustomerUID == Guid.Empty ? (Guid?)null : assetOwnerEvent.AssetOwnerRecord.CustomerUID;
			Guid? accountUid = assetOwnerEvent.AssetOwnerRecord.AccountUID == null || assetOwnerEvent.AssetOwnerRecord.AccountUID == Guid.Empty ? (Guid?)null : assetOwnerEvent.AssetOwnerRecord.AccountUID;
			DateTime utcNow = DateTime.UtcNow;
			var message = new KafkaMessage
			{
				Key = assetOwnerEvent.AssetUID.ToString(),
				Message = new
				{
					AssetUID = assetOwnerEvent.AssetUID,
					AssetOwnerRecord = new ClientModel.AssetOwner
					{
						CustomerName = assetOwnerEvent.AssetOwnerRecord.CustomerName,
						AccountName = assetOwnerEvent.AssetOwnerRecord.AccountName,
						DealerAccountCode = assetOwnerEvent.AssetOwnerRecord.DealerAccountCode,
						DealerUID = assetOwnerEvent.AssetOwnerRecord.DealerUID,
						DealerName = assetOwnerEvent.AssetOwnerRecord.DealerName,
						NetworkCustomerCode = assetOwnerEvent.AssetOwnerRecord.NetworkCustomerCode,
						NetworkDealerCode = assetOwnerEvent.AssetOwnerRecord.NetworkDealerCode,
						CustomerUID = customerUid,
						AccountUID = accountUid
					},
					Action = "Delete",
					ActionUTC = utcNow, //ToDO: Ensure the Insert UTC values
					ReceivedUTC = utcNow
				}
			};

			var actions = new List<Action>()
					{
						() => _transaction.Delete(query),
						() => assetOwnerTopicNames?.ForEach((topic)=>{message.Topic=topic; _transaction.Publish(message);})
					};

			return _transaction.Execute(actions);
		}

		public bool CheckExistingAssetOwner(Guid assetGuid)
		{
			var query = $"select count(1) from md_asset_AssetOwner where fk_AssetUID = {assetGuid.ToStringWithoutHyphens().WrapWithUnhex()}";
			var result = _transaction.Get<string>(query);
			return Convert.ToInt32(result.FirstOrDefault()) > 0;
		}

		public AssetOwnerInfo GetExistingAssetOwner(Guid assetGuid)
		{
			string query = $"select hex(fk_CustomerUID) as CustomerUID,hex(fk_DealerCustomerUID) as DealerUID, hex(fk_AccountCustomerUID) as AccountUID, NetworkCustomerCode as NetworkCustomerCode, DealerAccountCode as DealerAccountCode, NetworkDealerCode as NetworkDealerCode, AccountName as AccountName ,CustomerName as CustomerName , DealerName as DealerName  " +
				$"from md_asset_AssetOwner where fk_AssetUID = {assetGuid.ToStringWithoutHyphens().WrapWithUnhex()}";
			return _transaction.Get<AssetOwnerInfo>(query).SingleOrDefault();
		}
		private DbModel.AssetOwner GetAssetOwnerDBModel(AssetOwnerEvent assetOwnerEvent)
		{
			DateTime utcNow = DateTime.UtcNow;
			return new DbModel.AssetOwner
			{
				fk_AssetUID = assetOwnerEvent.AssetUID.Value,
				fk_CustomerUID = assetOwnerEvent.AssetOwnerRecord.CustomerUID,
				CustomerName = assetOwnerEvent.AssetOwnerRecord.CustomerName,
				fk_DealerCustomerUID = assetOwnerEvent.AssetOwnerRecord.DealerUID,
				DealerName = assetOwnerEvent.AssetOwnerRecord.DealerName,
				fk_AccountCustomerUID = assetOwnerEvent.AssetOwnerRecord.AccountUID,
				AccountName = assetOwnerEvent.AssetOwnerRecord.AccountName,
				NetworkCustomerCode = assetOwnerEvent.AssetOwnerRecord.NetworkCustomerCode,
				DealerAccountCode = assetOwnerEvent.AssetOwnerRecord.DealerAccountCode,
				NetworkDealerCode = assetOwnerEvent.AssetOwnerRecord.NetworkDealerCode,
				InsertUTC = utcNow,
				UpdateUTC = utcNow
			};
		}

		#endregion
	}
}
