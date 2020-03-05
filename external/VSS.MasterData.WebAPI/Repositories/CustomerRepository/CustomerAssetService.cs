using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using CustomerEnum = VSS.MasterData.WebAPI.Utilities.Enums;

namespace VSS.MasterData.WebAPI.CustomerRepository
{
	public class CustomerAssetService : CustomerService, ICustomerAssetService
	{
		private readonly ITransactions transaction;
		private static List<string> CustomerTopics;
		private readonly ILogger logger;

		public CustomerAssetService(ITransactions transaction, IConfiguration configuration, ILogger logger)
			: base(transaction, configuration, logger)
		{
			this.transaction = transaction;
			this.logger = logger;
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
		}

		public DbAssetCustomer GetAssetCustomer(Guid customerUid, Guid assetUid)
		{
			var getCustomerAssetQuery = "SELECT AssetCustomerID,Fk_CustomerUID,Fk_AssetUID," +
										"fk_AssetRelationTypeID,LastCustomerUTC FROM md_customer_CustomerAsset WHERE Fk_CustomerUID = {0}" +
										" AND Fk_AssetUID = {1};";
			return transaction.Get<DbAssetCustomer>(string.Format(getCustomerAssetQuery,
				customerUid.ToStringAndWrapWithUnhex(), assetUid.ToStringAndWrapWithUnhex()))?.FirstOrDefault();
		}

		public DbAssetCustomer GetAssetCustomerByRelationType(Guid customerUid, Guid assetUid, int relationType)
		{
			var getCustomerAssetQuery = "SELECT AssetCustomerID,Fk_CustomerUID,Fk_AssetUID," +
										"fk_AssetRelationTypeID,LastCustomerUTC FROM md_customer_CustomerAsset WHERE Fk_CustomerUID = {0}" +
										" AND Fk_AssetUID = {1} AND fk_AssetRelationTypeID={2};";
			return transaction.Get<DbAssetCustomer>(string.Format(getCustomerAssetQuery,
					customerUid.ToStringAndWrapWithUnhex(), assetUid.ToStringAndWrapWithUnhex(), relationType))
				?.FirstOrDefault();
		}

		public bool AssociateCustomerAsset(AssociateCustomerAssetEvent associateCustomerAsset)
		{
			try
			{
				var customer = GetCustomer(associateCustomerAsset.CustomerUID);
				if (customer?.CustomerID > 0)
				{
					Enum.TryParse(associateCustomerAsset.RelationType, true,
						out CustomerEnum.RelationType relationType);

					var messages = CustomerTopics
						?.Select(topic => new KafkaMessage
						{
							Key = associateCustomerAsset.CustomerUID.ToString(),
							Message = new
							{
								AssociateCustomerAssetEvent = new
								{
									associateCustomerAsset.CustomerUID,
									associateCustomerAsset.AssetUID,
									RelationType = relationType.ToString(),
									associateCustomerAsset.ActionUTC,
									associateCustomerAsset.ReceivedUTC
								}
							},
							Topic = topic
						})
						?.ToList();
					var assetCustomer = new DbAssetCustomer
					{
						Fk_CustomerUID = associateCustomerAsset.CustomerUID,
						Fk_AssetUID = associateCustomerAsset.AssetUID,
						fk_AssetRelationTypeID = (int)relationType,
						LastCustomerUTC = DateTime.UtcNow
					};

					var actions = new List<Action>()
					{
						() => transaction.Upsert(assetCustomer),
						() => transaction.Publish(messages)
					};
					return transaction.Execute(actions);
				}

				logger.LogInformation($"Skipping the CustomerAsset Association as the required customeruid" +
									$" {associateCustomerAsset.CustomerUID} has not been received yet.");
				return false;
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while associating asset to customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DissociateCustomerAsset(DissociateCustomerAssetEvent dissociateCustomerAsset)
		{
			try
			{
				var messages = CustomerTopics
					?.Select(topic => new KafkaMessage
					{
						Key = dissociateCustomerAsset.CustomerUID.ToString(),
						Message = new { DissociateCustomerAssetEvent = dissociateCustomerAsset },
						Topic = topic
					})
					?.ToList();

				var deleteQuery = string.Format("DELETE FROM md_customer_CustomerAsset " +
												"WHERE fk_CustomerUID = {0} AND fk_AssetUID = {1};",
					dissociateCustomerAsset.CustomerUID.ToStringAndWrapWithUnhex(),
					dissociateCustomerAsset.AssetUID.ToStringAndWrapWithUnhex());

				var actions = new List<Action>()
				{
					() => transaction.Delete(deleteQuery),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while dissociating asset from customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}
	}
}