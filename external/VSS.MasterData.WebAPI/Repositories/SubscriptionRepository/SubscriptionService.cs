using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.SubscriptionRepository.Models;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Enums;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using VSS.MasterData.WebAPI.Utilities.Helpers;

namespace VSS.MasterData.WebAPI.SubscriptionRepository
{
	public class SubscriptionService : ISubscriptionService
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly ITransactions transaction;
		private readonly ICustomerService customerService;

		private string[] topics;

		private Dictionary<string, long> _assetSubscriptionTypeCache =
			new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);

		private Dictionary<string, long> _projectSubscriptionTypeCache =
			new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);

		private Dictionary<string, long> _customerSubscriptionTypeCache =
			new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);

		public SubscriptionService(ICustomerService customerService, IConfiguration configuration, ILogger logger,
									ITransactions transaction)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.transaction = transaction;
			this.customerService = customerService;
			topics = configuration["SubscriptionKafkaTopicNames"].Split(',')
				.Select(x => x + configuration["TopicSuffix"]).ToArray();
			GetServicePlan();
		}

		private void GetServicePlan()
		{
			var readAllServiceViewQuery =
				"select st.Name,st.ServiceTypeID,stf.FamilyName from md_subscription_ServiceType st inner join md_subscription_ServiceTypeFamily stf on st.fk_ServiceTypeFamilyID = stf.ServiceTypeFamilyID";
			IEnumerable<ServiceView> serviceView = transaction.Get<ServiceView>(readAllServiceViewQuery);

			foreach (var service in serviceView)
			{
				var familyname = service.FamilyName.ToLower();
				switch (familyname)
				{
					case "asset":
						_assetSubscriptionTypeCache.Add(service.Name, service.ServiceTypeID);
						break;
					case "customer":
						_customerSubscriptionTypeCache.Add(service.Name, service.ServiceTypeID);
						break;
					case "project":
						_projectSubscriptionTypeCache.Add(service.Name, service.ServiceTypeID);
						break;
					default:
						throw new Exception("ServiceTypeFamily does not exist");
				}
			}
		}

		public bool CheckExistingSubscription(Guid subscriptionGuid, string createSubscriptionType)
		{
			var isExistingSubscription = false;
			switch (createSubscriptionType)
			{
				case "AssetSubscriptionEvent":
					isExistingSubscription = GetExistingAssetSubscription(subscriptionGuid) != null ? true : false;
					break;
				case "CustomerSubscriptionEvent":
					isExistingSubscription = GetExistingCustomerSubscription(subscriptionGuid) != null ? true : false;
					break;
				case "ProjectSubscriptionEvent":
					isExistingSubscription = GetExistingProjectSubscription(subscriptionGuid) != null ? true : false;
					break;
			}

			return isExistingSubscription;
		}

		public DbAssetSubscription GetExistingAssetSubscription(Guid subscriptionGuid)
		{
			var readExistingSubscriptionQuery =
				string.Format("select * from md_subscription_AssetSubscription where AssetSubscriptionUID = {0}",
					subscriptionGuid.ToStringAndWrapWithUnhex());
			var existingSubscription =
				transaction.Get<DbAssetSubscription>(readExistingSubscriptionQuery).FirstOrDefault();
			return existingSubscription;
		}

		public DbCustomerSubscription GetExistingCustomerSubscription(Guid subscriptionGuid)
		{
			var readExistingSubscriptionQuery =
				string.Format("select * from md_subscription_CustomerSubscription where CustomerSubscriptionUID = {0}",
					subscriptionGuid.ToStringAndWrapWithUnhex());
			var existingSubscription =
				transaction.Get<DbCustomerSubscription>(readExistingSubscriptionQuery).FirstOrDefault();
			return existingSubscription;
		}

		public DbProjectSubscription GetExistingProjectSubscription(Guid subscriptionGuid)
		{
			var readExistingSubscriptionQuery =
				string.Format("select * from md_subscription_ProjectSubscription where ProjectSubscriptionUID = {0}",
					subscriptionGuid.ToStringAndWrapWithUnhex());
			var existingSubscription =
				transaction.Get<DbProjectSubscription>(readExistingSubscriptionQuery).FirstOrDefault();
			return existingSubscription;
		}

		#region Asset Subscription

		public bool CreateAssetSubscriptions(List<CreateAssetSubscriptionEvent> createSubscriptionList)
		{
			SubscriptionSource subscriptionSource;
			var kafkaMessageList = new List<KafkaMessage>();
			var currentUtc = DateTime.UtcNow;

			var subscriptionList = new List<DbAssetSubscription>();
			foreach (var createSubscription in createSubscriptionList)
			{
				if (createSubscription.SubscriptionType == null ||
					!_assetSubscriptionTypeCache.ContainsKey(createSubscription.SubscriptionType))
				{
					throw new Exception("Invalid Asset Subscription Type for the SubscriptionUID:- " +
										createSubscription.SubscriptionUID);
				}

				Enum.TryParse(createSubscription.Source, true, out subscriptionSource);
				var createSubscriptionModel = new DbAssetSubscription
				{
					AssetSubscriptionUID = createSubscription.SubscriptionUID.Value,
					fk_AssetUID = createSubscription.AssetUID.Value,
					fk_DeviceUID = createSubscription.DeviceUID.Value,
					fk_CustomerUID = createSubscription.CustomerUID.Value,
					fk_SubscriptionSourceID = (int)subscriptionSource,
					StartDate = createSubscription.StartDate.Value,
					EndDate = createSubscription.EndDate.Value,
					InsertUTC = currentUtc,
					UpdateUTC = currentUtc,
					fk_ServiceTypeID = _assetSubscriptionTypeCache[createSubscription.SubscriptionType],
					LastProcessStatus = 0
				};
				subscriptionList.Add(createSubscriptionModel);

				topics.ToList().ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = createSubscription.SubscriptionUID.ToString(),
						Message = new { CreateAssetSubscriptionEvent = createSubscription },
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});
			}

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbAssetSubscription>(subscriptionList));
			actions.Add(() => transaction.Publish(kafkaMessageList));

			return transaction.Execute(actions);
		}

		public bool UpdateAssetSubscriptions(List<UpdateAssetSubscriptionEvent> updateSubscriptionList)
		{
			SubscriptionSource subscriptionSource;
			var kafkaMessageList = new List<KafkaMessage>();
			var currentUtc = DateTime.UtcNow;

			var subscriptionList = new List<DbAssetSubscription>();
			foreach (var updateSubscription in updateSubscriptionList)
			{
				if (updateSubscription.SubscriptionType == null ||
					!_assetSubscriptionTypeCache.ContainsKey(updateSubscription.SubscriptionType))
				{
					throw new Exception("Invalid Asset Subscription Type for the SubscriptionUID:- " +
										updateSubscription.SubscriptionUID);
				}

				Enum.TryParse(updateSubscription.Source, true, out subscriptionSource);
				var updateSubscriptionModel = new DbAssetSubscription
				{
					AssetSubscriptionUID = updateSubscription.SubscriptionUID.Value,
					fk_AssetUID = updateSubscription.AssetUID.Value,
					fk_DeviceUID = updateSubscription.DeviceUID.Value,
					fk_CustomerUID = updateSubscription.CustomerUID.Value,
					fk_SubscriptionSourceID = (int)subscriptionSource,
					StartDate = updateSubscription.StartDate.Value,
					EndDate = updateSubscription.EndDate.Value,
					InsertUTC = currentUtc,
					UpdateUTC = currentUtc,
					fk_ServiceTypeID = _assetSubscriptionTypeCache[updateSubscription.SubscriptionType]
				};
				subscriptionList.Add(updateSubscriptionModel);

				topics.ToList().ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = updateSubscription.SubscriptionUID.ToString(),
						Message = new { UpdateAssetSubscriptionEvent = updateSubscription },
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});
			}

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbAssetSubscription>(subscriptionList));
			actions.Add(() => transaction.Publish(kafkaMessageList));

			return transaction.Execute(actions);
		}

		public bool CreateAssetSubscription(CreateAssetSubscriptionEvent createSubscription)
		{
			if (createSubscription.SubscriptionType == null ||
				!_assetSubscriptionTypeCache.ContainsKey(createSubscription.SubscriptionType))
			{
				throw new Exception("Invalid Asset Subscription Type for the SubscriptionUID:- " +
									createSubscription.SubscriptionUID);
			}

			var currentUtc = DateTime.UtcNow;
			SubscriptionSource subscriptionSource;
			Enum.TryParse(createSubscription.Source, true, out subscriptionSource);
			var createSubscriptionModel = new DbAssetSubscription
			{
				AssetSubscriptionUID = createSubscription.SubscriptionUID.Value,
				fk_AssetUID = createSubscription.AssetUID.Value,
				fk_DeviceUID = createSubscription.DeviceUID.HasValue ? createSubscription.DeviceUID.Value : Guid.Empty,
				fk_CustomerUID = createSubscription.CustomerUID.Value,
				fk_SubscriptionSourceID = (int)subscriptionSource,
				StartDate = createSubscription.StartDate.Value,
				EndDate = createSubscription.EndDate.Value,
				InsertUTC = currentUtc,
				UpdateUTC = currentUtc,
				fk_ServiceTypeID = _assetSubscriptionTypeCache[createSubscription.SubscriptionType],
				LastProcessStatus = 0
			};

			var kafkaMessage = new KafkaMessage()
			{
				Key = createSubscription.SubscriptionUID.ToString(),
				Message = new { CreateAssetSubscriptionEvent = createSubscription }
			};

			var actions = new List<Action>
			{
				() => transaction.Upsert<DbAssetSubscription>(createSubscriptionModel),
				() => topics.ToList().ForEach(topic =>
				{
					kafkaMessage.Topic = topic;
					transaction.Publish(kafkaMessage);
				})
			};

			return transaction.Execute(actions);
		}

		public bool UpdateAssetSubscription(UpdateAssetSubscriptionEvent updateSubscription)
		{
			var dbAsset = GetExistingAssetSubscription(updateSubscription.SubscriptionUID.Value);
			if (dbAsset == null)
			{
				throw new Exception("Asset Subscription not exists!");
			}

			if (!_assetSubscriptionTypeCache.ContainsKey(updateSubscription.SubscriptionType))
			{
				throw new Exception("Invalid Asset Subscription Type for the SubscriptionUID:- " +
									updateSubscription.SubscriptionUID);
			}

			if (!FieldHelper.IsValidValuesFilled(updateSubscription, dbAsset, logger))
			{
				logger.LogError("Second Parameter expects typeOf IDbTable");
			}

			dbAsset.fk_ServiceTypeID = _assetSubscriptionTypeCache[updateSubscription.SubscriptionType];
			dbAsset.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = updateSubscription.SubscriptionUID.ToString(),
				Message = new { UpdateAssetSubscriptionEvent = updateSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbAssetSubscription>(dbAsset));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		#endregion

		#region Customer Subscription

		public bool CreateCustomerSubscription(CreateCustomerSubscriptionEvent createSubscription)
		{
			if (createSubscription.SubscriptionType == null ||
				!_customerSubscriptionTypeCache.ContainsKey(createSubscription.SubscriptionType))
			{
				throw new Exception("Invalid Customer Subscription Type");
			}

			var currentUtc = DateTime.UtcNow;
			var subscription = new DbCustomerSubscription()
			{
				CustomerSubscriptionUID = createSubscription.SubscriptionUID.Value,
				fk_CustomerUID = createSubscription.CustomerUID.Value,
				fk_ServiceTypeID = _customerSubscriptionTypeCache[createSubscription.SubscriptionType],
				StartDate = createSubscription.StartDate.Value,
				EndDate = createSubscription.EndDate.Value,
				InsertUTC = currentUtc,
				UpdateUTC = currentUtc
			};

			var kafkaMessage = new KafkaMessage()
			{
				Key = createSubscription.SubscriptionUID.ToString(),
				Message = new { CreateCustomerSubscriptionEvent = createSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(subscription));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool UpdateCustomerSubscription(UpdateCustomerSubscriptionEvent updateSubscription)
		{
			var dbCustomer = GetExistingCustomerSubscription(updateSubscription.SubscriptionUID.Value);
			if (dbCustomer == null)
			{
				throw new Exception("Customer Subscription not exists!");
			}

			if (!updateSubscription.StartDate.HasValue && !updateSubscription.EndDate.HasValue)
			{
				throw new Exception("Update Customer Subscription Request should have atleast one field to update");
			}

			if (!FieldHelper.IsValidValuesFilled(updateSubscription, dbCustomer, logger))
			{
				logger.LogError("Second Parameter expects typeOf IDbTable");
			}

			dbCustomer.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = updateSubscription.SubscriptionUID.ToString(),
				Message = new { UpdateCustomerSubscriptionEvent = updateSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(dbCustomer));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		#endregion

		#region Project Subscription

		public bool CreateProjectSubscription(CreateProjectSubscriptionEvent createSubscription)
		{
			if (createSubscription.SubscriptionType == null ||
				!_projectSubscriptionTypeCache.ContainsKey(createSubscription.SubscriptionType))
			{
				throw new Exception("Invalid Project Subscription Type");
			}

			var currentUtc = DateTime.UtcNow;
			var subscription = new DbProjectSubscription()
			{
				ProjectSubscriptionUID = createSubscription.SubscriptionUID.Value,
				fk_ProjectUID = null,
				StartDate = createSubscription.StartDate.Value,
				EndDate = createSubscription.EndDate.Value,
				fk_CustomerUID = createSubscription.CustomerUID.Value,
				fk_ServiceTypeID = _projectSubscriptionTypeCache[createSubscription.SubscriptionType],
				InsertUTC = currentUtc,
				UpdateUTC = currentUtc
			};

			var kafkaMessage = new KafkaMessage()
			{
				Key = createSubscription.SubscriptionUID.ToString(),
				Message = new { CreateProjectSubscriptionEvent = createSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(subscription));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool UpdateProjectSubscription(UpdateProjectSubscriptionEvent updateSubscription)
		{
			var dbProject = GetExistingProjectSubscription(updateSubscription.SubscriptionUID.Value);
			if (dbProject == null)
			{
				throw new Exception("Project Subscription not exists!");
			}

			if (string.IsNullOrEmpty(updateSubscription.SubscriptionType) && !updateSubscription.CustomerUID.HasValue &&
				!updateSubscription.StartDate.HasValue && !updateSubscription.EndDate.HasValue)
			{
				throw new Exception("Update Project Subscription Request should have atleast one field to update");
			}

			if (string.IsNullOrEmpty(updateSubscription.SubscriptionType))
			{
				updateSubscription.SubscriptionType = _projectSubscriptionTypeCache
					.FirstOrDefault(x => x.Value == dbProject.fk_ServiceTypeID).Key;
			}
			else if (_projectSubscriptionTypeCache.ContainsKey(updateSubscription.SubscriptionType))
			{
				dbProject.fk_ServiceTypeID = _projectSubscriptionTypeCache[updateSubscription.SubscriptionType];
			}
			else
			{
				throw new Exception("Invalid Project Subscription Type");
			}

			if (!FieldHelper.IsValidValuesFilled(updateSubscription, dbProject, logger))
			{
				logger.LogError("Second Parameter expects typeOf IDbTable");
			}

			dbProject.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = updateSubscription.SubscriptionUID.ToString(),
				Message = new { UpdateProjectSubscriptionEvent = updateSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(dbProject));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool AssociateProjectSubscription(AssociateProjectSubscriptionEvent associateSubscription)
		{
			var toBeAssociated = GetExistingProjectSubscription(associateSubscription.SubscriptionUID);

			if (toBeAssociated == null)
			{
				throw new Exception("Invalid ProjectSubscriptionUID");
			}

			toBeAssociated.fk_ProjectUID = associateSubscription.ProjectUID;
			toBeAssociated.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = associateSubscription.SubscriptionUID.ToString(),
				Message = new { AssociateProjectSubscriptionEvent = associateSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(toBeAssociated));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool DissociateProjectSubscription(DissociateProjectSubscriptionEvent dissociateSubscription)
		{
			var toBeDissociated = GetExistingProjectSubscription(dissociateSubscription.SubscriptionUID);

			if (toBeDissociated == null)
			{
				throw new Exception("Invalid ProjectSubscriptionUID");
			}

			toBeDissociated.fk_ProjectUID = null;
			toBeDissociated.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = dissociateSubscription.SubscriptionUID.ToString(),
				Message = new { DissociateProjectSubscriptionEvent = dissociateSubscription }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert(toBeDissociated));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		#endregion

		#region Get Subscription Details

		public AssetSubscriptionModel GetSubscriptionForAsset(Guid assetGuid)
		{
			var getAssetSubscriptionQuery =
				"Select asn.AssetSubscriptionUID as SubscriptionUID,asn.StartDate as SubscriptionStartDate,asn.EndDate as SubscriptionEndDate,asn.fk_CustomerUID as CustomerUID, " +
				"(case UTC_TIMESTAMP() between asn.StartDate and asn.EndDate when true then 'Active' else 'InActive' end)  as SubscriptionStatus, st.Name as SubscriptionName " +
				"FROM md_subscription_AssetSubscription asn " +
				"join md_subscription_ServiceType st on st.ServiceTypeID = asn.fk_ServiceTypeID and st.fk_ServiceTypeFamilyID=1 where asn.fk_AssetUID= " +
				assetGuid.ToStringAndWrapWithUnhex() + "; ";
			List<OwnerVisibility> assetSubscriptonList =
				transaction.Get<OwnerVisibility>(getAssetSubscriptionQuery).ToList();
			var assetSubscription = new AssetSubscriptionModel();
			if (assetSubscriptonList.Any())
			{
				List<DbCustomer> lstCustomers =
					customerService.GetCustomerByCustomerGuids(assetSubscriptonList.Select(x => x.CustomerUID)
						.Distinct().ToList().ToArray());
				if (lstCustomers.Any())
				{
					foreach (var vi in assetSubscriptonList)
					{
						var customer = lstCustomers.Where(x => x.CustomerUID == vi.CustomerUID).Select(y => y).ToList()
							.FirstOrDefault();
						if (customer != null)
						{
							vi.CustomerName = customer.CustomerName;
							vi.CustomerType =
								((CustomerType)Enum.ToObject(typeof(CustomerType), customer.fk_CustomerTypeID))
								.ToString();
						}
					}
				}

				assetSubscription.AssetUID = assetGuid;
				assetSubscription.SubscriptionStatus =
					assetSubscriptonList.Where(s => s.SubscriptionStatus == "Active").ToList().Any() == true
						? "Active"
						: "InActive";
				assetSubscription.OwnersVisibility = assetSubscriptonList.ToList();
			}

			return assetSubscription;
		}

		public IEnumerable<CustomerSubscriptionModel> GetSubscriptionForCustomer(Guid customerGuid)
		{
			var getCustomerSubscriptionQuery =
				"select st.Name as SubscriptionType,customer.StartDate,customer.EndDate from md_subscription_CustomerSubscription customer " +
				"inner join md_subscription_ServiceType st on customer.fk_ServiceTypeID = st.ServiceTypeID " +
				"where fk_CustomerUID = " + customerGuid.ToStringAndWrapWithUnhex() +
				" and customer.startDate <= UTC_TIMESTAMP() and customer.enddate >= UTC_TIMESTAMP();";
			IEnumerable<CustomerSubscriptionModel> customerSubscriptionList =
				transaction.Get<CustomerSubscriptionModel>(getCustomerSubscriptionQuery);
			return customerSubscriptionList.OrderByDescending(e => e.EndDate).GroupBy(e => e.SubscriptionType)
				.Select(e => e.First()).ToList();
		}

		public IEnumerable<ActiveProjectCustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(
			Guid customerGuid)
		{
			var getActiveProjectCustomerSubscriptionQuery =
				"select project.ProjectSubscriptionUID as SubscriptionGuid,st.Name as SubscriptionType,customer.StartDate,customer.EndDate from md_subscription_CustomerSubscription customer " +
				"inner join md_subscription_ServiceType st on customer.fk_ServiceTypeID = st.ServiceTypeID " +
				"inner join md_subscription_ProjectSubscription project on project.fk_CustomerUID = customer.fk_CustomerUID and project.fk_ServiceTypeId = customer.fk_ServiceTypeId " +
				"where customer.fk_CustomerUID =" + customerGuid.ToStringAndWrapWithUnhex() +
				" and project.fk_ProjectUID is null and customer.startDate <= UTC_TIMESTAMP() and customer.enddate >= UTC_TIMESTAMP();";
			IEnumerable<ActiveProjectCustomerSubscriptionModel> activeProjectCustomerSubscriptionList =
				transaction.Get<ActiveProjectCustomerSubscriptionModel>(getActiveProjectCustomerSubscriptionQuery);
			return activeProjectCustomerSubscriptionList;
		}

		#endregion
	}
}