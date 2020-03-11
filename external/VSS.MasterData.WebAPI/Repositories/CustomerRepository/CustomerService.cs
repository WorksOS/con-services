using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using VSS.MasterData.WebAPI.Utilities.Helpers;
using CustomerEnum = VSS.MasterData.WebAPI.Utilities.Enums;

namespace VSS.MasterData.WebAPI.CustomerRepository
{
	public class CustomerService : ICustomerService
	{
		private readonly ITransactions transaction;
		private readonly List<string> CustomerTopics;
		private readonly Guid EngineeringOperationsCustomerUID;
		private readonly ILogger logger;

		public CustomerService(ITransactions transaction, IConfiguration configuration, ILogger logger)
		{
			this.transaction = transaction;
			this.logger = logger;
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
			EngineeringOperationsCustomerUID = Guid.Parse(configuration["EngineeringOperationsCustomerUid"]);
		}

		#region Customer

		public DbCustomer GetCustomer(Guid customerUid)
		{
			var getCustomerQuery = "select CustomerID,CustomerUID,CustomerName,fk_CustomerTypeID," +
									"LastCustomerUTC,PrimaryContactEmail,FirstName,LastName,NetworkDealerCode,IsActive," +
									"BSSID,DealerNetwork,NetworkCustomerCode,DealerAccountCode" +
									" from md_customer_Customer where CustomerUID = {0}";
			return transaction.Get<DbCustomer>(string.Format(getCustomerQuery,
				customerUid.ToStringAndWrapWithUnhex()))?.FirstOrDefault();
		}

		public List<DbCustomer> GetAssociatedCustomersbyUserUid(Guid userUid)
		{
			var getCustomerQuery = "SELECT c.CustomerID,c.CustomerName,c.fk_CustomerTypeID,c.CustomerUID," +
									"c.LastCustomerUTC FROM md_customer_Customer c " +
									"JOIN md_customer_CustomerUser uc on uc.fk_CustomerUID = c.CustomerUID " +
									"WHERE uc.fk_UserUID = {0};";
			return transaction.Get<DbCustomer>(string.Format(getCustomerQuery,
				userUid.ToStringAndWrapWithUnhex()))?.ToList();
		}

		public List<DbCustomer> GetAssociatedCustomersForDealer(Guid customerUid)
		{
			var getCustomerQuery = "SELECT c.CustomerUID,c.CustomerID,c.CustomerName,c.fk_CustomerTypeID," +
									"c.LastCustomerUTC FROM md_customer_CustomerAsset ac1 " +
									"JOIN md_customer_CustomerAsset ac2 ON ac1.fk_AssetUID = ac2.fk_AssetUID " +
									"JOIN md_customer_Customer c on c.CustomerUID = ac2.fk_CustomerUID " +
									"WHERE ac1.fk_CustomerUID = {0} " +
									"AND ac1.fk_CustomerUID != ac2.fk_CustomerUID " +
									"AND ac1.fk_AssetRelationTypeID != {1} " +
									"AND ac2.fk_AssetRelationTypeID in ({2},{3});";
			IEnumerable<DbCustomer> customerList = transaction.Get<DbCustomer>(string.Format(getCustomerQuery,
				customerUid.ToStringAndWrapWithUnhex(),
				(int)CustomerEnum.AssetCustomerRelationShipType.SharedOwner,
				(int)CustomerEnum.AssetCustomerRelationShipType.Owner,
				(int)CustomerEnum.AssetCustomerRelationShipType.Customer
			));

			return customerList?.GroupBy(e => e.CustomerUID)
				?.Select(e => e.FirstOrDefault())
				?.ToList();
		}

		public List<DbCustomer> GetCustomerByCustomerGuids(Guid[] customerUids)
		{
			var getCustomerQuery = "SELECT CustomerUID,CustomerID,CustomerName,fk_CustomerTypeID," +
									"LastCustomerUTC FROM md_customer_Customer WHERE CustomerUID in ({0});";
			List<string> customerGuids = customerUids.Select(g => g.ToStringAndWrapWithUnhex())?.ToList();
			return transaction.Get<DbCustomer>(string.Format(getCustomerQuery,
				string.Join(",", customerGuids)))?.ToList();
		}

		public List<Tuple<DbCustomer, DbAccount>> GetCustomersByNameSearch(string filter, int maxResults)
		{
			var getQuery = @" SELECT DISTINCT
									c.CustomerUID,
									c.CustomerID,
									c.CustomerName,
									c.fk_CustomerTypeID,
									c.LastCustomerUTC,
									c.NetworkDealerCode,
									CASE
										WHEN c.fk_CustomerTypeID = 0 THEN ca.NetworkCustomerCode
										ELSE NULL
									END AS NetworkCustomerCode
								FROM
									md_customer_Customer c
										LEFT JOIN
									md_customer_CustomerAccount ca ON c.CustomerUID = ca.fk_ChildCustomerUID
								WHERE
									(c.customername LIKE @filterText
										OR CASE
										WHEN c.fk_CustomerTypeID = 0 THEN ca.NetworkCustomerCode LIKE @filterText
										WHEN c.fk_CustomerTypeID = 1 THEN c.NetworkDealerCode LIKE @filterText
									END)
										AND c.CustomerUID <> @supportCustomerUid
										AND c.fk_CustomerTypeID IN (0 , 1)
								ORDER BY CustomerName
								LIMIT @recordCount; ";
			return transaction.Get<DbCustomer, DbAccount>(getQuery, "NetworkCustomerCode",
				new
				{
					filterText = $"%{filter}%",
					supportCustomerUid = EngineeringOperationsCustomerUID.ToStringAndWrapWithUnhex(),
					recordCount = maxResults
				});
		}

		public List<AssetCustomerDetail> GetAssetCustomerByAssetGuid(Guid assetUid)
		{
			var getCustomersQuery = @"SELECT 
											ac.fk_CustomerUID AS CustomerUID,
											c.CustomerName AS CustomerName,
											IFNULL(c.fk_CustomerTypeID,-1) AS CustomerType,
											c1.CustomerUID AS ParentCustomerUID,
											c1.CustomerName AS ParentName,
											IFNULL(c1.fk_CustomerTypeID,-1) AS ParentCustomerType
										FROM
											md_customer_CustomerAsset ac
												LEFT OUTER JOIN
											md_customer_Customer c ON c.CustomerUID = ac.fk_CustomerUID
												LEFT OUTER JOIN
											md_customer_CustomerRelationshipNode crn ON crn.fk_CustomerUID = c.CustomerUID
												LEFT OUTER JOIN
											md_customer_Customer c1 ON c1.CustomerUID = crn.fk_ParentCustomerUID
												AND c1.CustomerUID IN (SELECT 
													acs.fk_CustomerUID
												FROM
													md_customer_CustomerAsset acs
												WHERE
													acs.fk_AssetUID = ac.fk_AssetUID)
										WHERE
											ac.fk_AssetUID = {0};";
			return transaction.Get<AssetCustomerDetail>(string.Format(getCustomersQuery,
				assetUid.ToStringAndWrapWithUnhex()))?.ToList();
		}

		public bool CreateCustomer(CreateCustomerEvent createCustomer)
		{
			try
			{
				Enum.TryParse(createCustomer.CustomerType, true, out CustomerEnum.CustomerType customerType);

				FieldHelper.ReplaceEmptyFieldsByNull(createCustomer);

				var messages = CustomerTopics?
					.Select(topic => new KafkaMessage
					{
						Key = createCustomer.CustomerUID.ToString(),
						Topic = topic,
						Message = new
						{
							CreateCustomerEvent = new
							{
								createCustomer.CustomerName,
								CustomerType = customerType.ToString(),
								createCustomer.BSSID,
								createCustomer.DealerNetwork,
								createCustomer.NetworkDealerCode,
								createCustomer.NetworkCustomerCode,
								createCustomer.DealerAccountCode,
								createCustomer.PrimaryContactEmail,
								createCustomer.FirstName,
								createCustomer.LastName,
								createCustomer.CustomerUID,
								createCustomer.ActionUTC,
								createCustomer.ReceivedUTC
							}
						}
					})?.ToList();

				var customer = new DbCustomer
				{
					CustomerUID = createCustomer.CustomerUID,
					CustomerName = createCustomer.CustomerName,
					fk_CustomerTypeID = (long)customerType,
					PrimaryContactEmail = createCustomer.PrimaryContactEmail,
					FirstName = createCustomer.FirstName,
					LastName = createCustomer.LastName,
					NetworkDealerCode = createCustomer.NetworkDealerCode,
					LastCustomerUTC = DateTime.UtcNow,
					IsActive = createCustomer.IsActive ?? true,
					DealerNetwork = createCustomer.DealerNetwork,
					NetworkCustomerCode = createCustomer.NetworkCustomerCode,
					DealerAccountCode = createCustomer.DealerAccountCode,
					BSSID = createCustomer.BSSID
				};

				var actions = new List<Action>()
				{
					() => transaction.Upsert(customer),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool UpdateCustomer(UpdateCustomerEvent updateCustomer, DbCustomer customerDetails)
		{
			try
			{
				if (!FieldHelper.IsValidValuesFilled(updateCustomer, customerDetails, logger))
				{
					logger.LogWarning("DB Object expects typeOf IDbTable");
				}

				FieldHelper.ReplaceEmptyFieldsByNull(updateCustomer);

				var messages = CustomerTopics
					?.Select(topic => new KafkaMessage
					{
						Key = updateCustomer.CustomerUID.ToString(),
						Topic = topic,
						Message = new
						{
							UpdateCustomerEvent = new
							{
								updateCustomer.CustomerUID,
								updateCustomer.CustomerName,
								updateCustomer.BSSID,
								updateCustomer.DealerAccountCode,
								updateCustomer.DealerNetwork,
								updateCustomer.NetworkCustomerCode,
								updateCustomer.PrimaryContactEmail,
								updateCustomer.FirstName,
								updateCustomer.LastName,
								updateCustomer.NetworkDealerCode,
								updateCustomer.ActionUTC,
								updateCustomer.ReceivedUTC
							}
						}
					})?.ToList();

				var actions = new List<Action>()
				{
					() => transaction.Upsert(customerDetails),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while updating customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DeleteCustomer(Guid customerUid, DateTime actionUtc)
		{
			try
			{
				List<KafkaMessage> messages = CustomerTopics
					?.Select(topic => new KafkaMessage
					{
						Key = customerUid.ToString(),
						Topic = topic,
						Message = new
						{
							DeleteCustomerEvent = new DeleteCustomerEvent
							{
								CustomerUID = customerUid,
								ActionUTC = actionUtc,
								ReceivedUTC = DateTime.UtcNow
							}
						}
					})?.ToList();

				var deleteQuery = string.Format("DELETE FROM md_customer_Customer WHERE CustomerUID = {0};",
					customerUid.ToStringAndWrapWithUnhex());

				var actions = new List<Action>
				{
					() => transaction.Delete(deleteQuery),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while deleting customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		#endregion

		#region User Customer Relationship

		public bool CreateUserCustomerRelationship(CreateUserCustomerRelationshipEvent userCustomerRelation)
		{
			try
			{
				var customer = GetCustomer(userCustomerRelation.CustomerUID);
				if (customer?.CustomerID > 0)
				{
					var customerUserMessage = new AssociateCustomerUserEvent
					{
						UserUID = userCustomerRelation.UserUID,
						CustomerUID = userCustomerRelation.CustomerUID,
						ActionUTC = userCustomerRelation.ActionUTC,
						ReceivedUTC = userCustomerRelation.ReceivedUTC
					};
					FieldHelper.ReplaceEmptyFieldsByNull(userCustomerRelation);

					List<KafkaMessage> messages = CustomerTopics?.Select(topic => new KafkaMessage
					{
						Key = userCustomerRelation.CustomerUID.ToString(),
						Topic = topic,
						Message = new { AssociateCustomerUserEvent = customerUserMessage }
					})?.ToList();

					messages?.AddRange(CustomerTopics?.Select(topic => new KafkaMessage
					{
						Key = userCustomerRelation.CustomerUID.ToString(),
						Topic = topic,
						Message = new { CreateUserCustomerRelationshipEvent = userCustomerRelation }
					}));


					var userCustomer = new DbUserCustomer
					{
						fk_CustomerID = customer.CustomerID,
						fk_CustomerUID = userCustomerRelation.CustomerUID,
						fk_UserUID = userCustomerRelation.UserUID,
						LastUserUTC = DateTime.UtcNow
					};

					var actions = new List<Action>
					{
						() => transaction.Upsert(userCustomer),
						() => transaction.Publish(messages)
					};
					return transaction.Execute(actions);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(
					$"Error while creating user customer relationship in db: {ex.Message}, {ex.StackTrace}");
				throw;
			}

			return false;
		}

		public bool UpdateUserCustomerRelationship(UpdateUserCustomerRelationshipEvent userCustomerRelation)
		{
			try
			{
				FieldHelper.ReplaceEmptyFieldsByNull(userCustomerRelation);
				List<KafkaMessage> messages = CustomerTopics?.Select(topic => new KafkaMessage
				{
					Key = userCustomerRelation.CustomerUID.ToString(),
					Topic = topic,
					Message = new { UpdateUserCustomerRelationshipEvent = userCustomerRelation }
				})?.ToList();
				transaction.Publish(messages);
				return true;
			}
			catch (Exception ex)
			{
				logger.LogError($"Error publishing update user customer relationship : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DeleteUserCustomerRelationship(DeleteUserCustomerRelationshipEvent userCustomerRelation)
		{
			try
			{
				FieldHelper.ReplaceEmptyFieldsByNull(userCustomerRelation);
				var customerUserMessage = new DissociateCustomerUserEvent
				{
					UserUID = userCustomerRelation.UserUID,
					CustomerUID = userCustomerRelation.CustomerUID,
					ActionUTC = userCustomerRelation.ActionUTC,
					ReceivedUTC = userCustomerRelation.ReceivedUTC
				};
				List<KafkaMessage> messages = CustomerTopics?.Select(topic => new KafkaMessage
				{
					Key = userCustomerRelation.CustomerUID.ToString(),
					Topic = topic,
					Message = new { DissociateCustomerUserEvent = customerUserMessage }
				})?.ToList();
				messages?.AddRange(
					CustomerTopics?.Select(topic => new KafkaMessage
					{
						Key = userCustomerRelation.CustomerUID.ToString(),
						Topic = topic,
						Message = new { DeleteUserCustomerRelationshipEvent = userCustomerRelation }
					}));


				var deleteQuery = string.Format("DELETE FROM md_customer_CustomerUser" +
												" WHERE fk_CustomerUID = {0} AND fk_UserUID = {1};",
					userCustomerRelation.CustomerUID.ToStringAndWrapWithUnhex(),
					userCustomerRelation.UserUID.ToStringAndWrapWithUnhex());

				var actions = new List<Action>
				{
					() => transaction.Delete(deleteQuery),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating customer in db: {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		#endregion

		#region Customer Relationship

		public List<DbCustomerRelationshipNode> GetCustomerRelationships(Guid parentCustomerUID, Guid childCustomerUID)
		{
			var getCustomerQuery = "SELECT CustomerRelationshipNodeID,fk_RootCustomerUID,fk_ParentCustomerUID" +
									",Fk_CustomerUID,LeftNodePosition,RightNodePosition,LastCustomerRelationshipNodeUTC " +
									"FROM md_customer_CustomerRelationshipNode WHERE fk_ParentCustomerUID = {0} and fk_CustomerUID = {1};";

			return transaction.Get<DbCustomerRelationshipNode>(string.Format(getCustomerQuery,
				parentCustomerUID.ToStringAndWrapWithUnhex(),
				childCustomerUID.ToStringAndWrapWithUnhex()))?.ToList();
		}

		public List<DbCustomerRelationshipNode> GetCustomerRelationshipsByCustomers(List<Guid> customerUids)
		{
			var getCustomerQuery = "SELECT CustomerRelationshipNodeID,fk_RootCustomerUID,fk_ParentCustomerUID" +
									",Fk_CustomerUID,LeftNodePosition,RightNodePosition,LastCustomerRelationshipNodeUTC " +
									"FROM md_customer_CustomerRelationshipNode WHERE fk_CustomerUID IN ({0});";

			IEnumerable<string> guids = customerUids
				?.Select((customerUid) => customerUid.ToStringAndWrapWithUnhex());

			return transaction.Get<DbCustomerRelationshipNode>(string.Format(getCustomerQuery,
				string.Join(",", guids)))?.ToList();
		}

		public bool IsCustomerRelationShipAlreadyExists(string parentCustomerUID, string childCustomerUID)
		{
			parentCustomerUID = string.IsNullOrWhiteSpace(parentCustomerUID)
				? Guid.Parse(childCustomerUID).ToStringAndWrapWithUnhex()
				: Guid.Parse(parentCustomerUID).ToStringAndWrapWithUnhex();
			var query = "select CustomerRelationshipNodeID from md_customer_CustomerRelationshipNode " +
						"where fk_ParentCustomerUID = {0} and fk_CustomerUID = {1};";
			return transaction.Get<DbCustomerRelationshipNode>(string.Format(query, parentCustomerUID,
				Guid.Parse(childCustomerUID).ToStringAndWrapWithUnhex())).Any();
		}

		public bool CreateCustomerRelationShip(CreateCustomerRelationshipEvent customerRelationship)
		{
			try
			{
				List<KafkaMessage> messages = CustomerTopics
					?.Select(topic => new KafkaMessage
					{
						Topic = topic,
						Message = new { CreateCustomerRelationShipEvent = customerRelationship },
						Key = customerRelationship.ChildCustomerUID.ToString()
					})?.ToList();

				//Stand alone customer node, triggered on customer creation Root/Parent/Customer as self
				if (customerRelationship.ParentCustomerUID == null)
				{
					var isChildNodeExists = GetCustomerRelationshipsByCustomers(new List<Guid>
					{
						customerRelationship.ChildCustomerUID
					}).Any();
					if (!isChildNodeExists)
					{
						var customerRelationshipNode = new DbCustomerRelationshipNode
						{
							fk_RootCustomerUID = customerRelationship.ChildCustomerUID,
							fk_ParentCustomerUID = customerRelationship.ChildCustomerUID,
							FK_CustomerUID = customerRelationship.ChildCustomerUID,
							LeftNodePosition = 1,
							RightNodePosition = 2,
							LastCustomerRelationshipNodeUTC = DateTime.UtcNow
						};
						return transaction.Execute(new List<Action>
						{
							() => transaction.Upsert(customerRelationshipNode),
							() => transaction.Publish(messages)
						});
					}
					else
					{
						logger.LogInformation(Messages.ChildNodeAlreadyExistsInHierarchy);
					}

					return true;
				}

				//Create-Update existing hierarchy for the parent-child customer relationship
				List<DbCustomerRelationshipNode> nodeValues = GetCustomerRelationshipsByCustomers(new List<Guid>
				{
					(Guid) customerRelationship.ParentCustomerUID,
					customerRelationship.ChildCustomerUID
				});

				List<DbCustomerRelationshipNode> parentNodes = nodeValues
					?.Where(e => e.FK_CustomerUID == customerRelationship.ParentCustomerUID)
					?.ToList();
				List<DbCustomerRelationshipNode> childNodes = nodeValues
					?.Where(e => e.FK_CustomerUID == customerRelationship.ChildCustomerUID)
					?.ToList();
				var rootChild = childNodes
					?.FirstOrDefault(e => e.fk_RootCustomerUID == customerRelationship.ChildCustomerUID);

				#region Parent Doesn't Exist

				if (!parentNodes.Any())
				{
					return transaction.Execute(new List<Action>
					{
						() =>
						{
							var parentLeftNodePosition = 1;
							var parentRightNodePosition = 2;

							// Parent and Child does not exist. So creating parent and child with their relationship
							if (!childNodes.Any())
							{
								transaction.Upsert(new DbCustomerRelationshipNode
								{
									fk_RootCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
									fk_ParentCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
									FK_CustomerUID = customerRelationship.ChildCustomerUID,
									LeftNodePosition = parentRightNodePosition,
									RightNodePosition = parentRightNodePosition + 1,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
								parentRightNodePosition += 2;
							}
							//Parent Doesn't exists but Child exists as a Root node
							else if (rootChild != null)
							{
								List<DbCustomerRelationshipNode> dbUpsertObjects =
									new List<DbCustomerRelationshipNode>();
								var childNodeVal = rootChild;
								var nodeRightLeftDiff = parentRightNodePosition - parentLeftNodePosition;

								//Updating the Root CustomerUID ,left and right node value from parent
								List<DbCustomerRelationshipNode> listToUpdateWithRootCustomer =
									GetRelationshipNodesForRootCustomer(childNodeVal.FK_CustomerUID);
								if (listToUpdateWithRootCustomer.Any())
								{
									dbUpsertObjects.Clear();
									listToUpdateWithRootCustomer?.ForEach((i) =>
									{
										dbUpsertObjects.Add(new DbCustomerRelationshipNode
										{
											CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
											fk_RootCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
											fk_ParentCustomerUID = i.fk_ParentCustomerUID,
											FK_CustomerUID = i.FK_CustomerUID,
											LeftNodePosition = i.LeftNodePosition + nodeRightLeftDiff,
											RightNodePosition = i.RightNodePosition + nodeRightLeftDiff,
											LastCustomerRelationshipNodeUTC = DateTime.UtcNow
										});
									});
									transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
								}

								List<DbCustomerRelationshipNode> listToUpdateWithRootAndCustomer =
									GetRelationshipNodesForRootAndCustomer(
										(Guid) customerRelationship.ParentCustomerUID, childNodeVal.FK_CustomerUID);
								if (listToUpdateWithRootAndCustomer.Any())
								{
									dbUpsertObjects.Clear();
									listToUpdateWithRootAndCustomer?.ForEach((i) =>
									{
										dbUpsertObjects.Add(new DbCustomerRelationshipNode
										{
											CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
											fk_RootCustomerUID = i.fk_RootCustomerUID,
											fk_ParentCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
											FK_CustomerUID = i.FK_CustomerUID,
											LeftNodePosition = i.LeftNodePosition,
											RightNodePosition = i.RightNodePosition,
											LastCustomerRelationshipNodeUTC = DateTime.UtcNow
										});
									});
									transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
								}

								parentRightNodePosition +=
									childNodeVal.RightNodePosition - childNodeVal.LeftNodePosition + 1;
							}
							//Parent Doesn't exist but Child exists with Root CustomerUID as not equal 
							//to itself i.e it's an intermediate node..so duplicate child tree again
							else
							{
								List<DbCustomerRelationshipNode> dbUpsertObjects =
									new List<DbCustomerRelationshipNode>();
								var childNodeVal = childNodes.First();
								var sourceLeftNode = childNodeVal.LeftNodePosition;
								var newRightNode = parentRightNodePosition;

								//Select & Insert new node relationships
								List<DbCustomerRelationshipNode> listToInsert =
									GetRelationshipNodesForRootAndLeftRightPosition(
										childNodeVal.fk_RootCustomerUID, childNodeVal.LeftNodePosition,
										childNodeVal.RightNodePosition);
								if (listToInsert.Any())
								{
									dbUpsertObjects.Clear();
									listToInsert?.ForEach((i) =>
									{
										dbUpsertObjects.Add(new DbCustomerRelationshipNode
										{
											fk_RootCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
											fk_ParentCustomerUID = i.fk_ParentCustomerUID,
											FK_CustomerUID = i.FK_CustomerUID,
											LeftNodePosition = i.LeftNodePosition - (sourceLeftNode - newRightNode),
											RightNodePosition = i.RightNodePosition - (sourceLeftNode - newRightNode),
											LastCustomerRelationshipNodeUTC = DateTime.UtcNow
										});
									});
									transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
								}

								//Select & update parent nodes
								List<DbCustomerRelationshipNode> listToUpdate = GetRelationshipNodesForRootAndCustomer(
									(Guid) customerRelationship.ParentCustomerUID,
									customerRelationship.ChildCustomerUID);
								if (listToUpdate.Any())
								{
									dbUpsertObjects.Clear();
									listToUpdate?.ForEach((i) =>
									{
										dbUpsertObjects.Add(new DbCustomerRelationshipNode
										{
											CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
											fk_RootCustomerUID = i.fk_RootCustomerUID,
											fk_ParentCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
											FK_CustomerUID = i.FK_CustomerUID,
											LeftNodePosition = i.LeftNodePosition,
											RightNodePosition = i.RightNodePosition,
											LastCustomerRelationshipNodeUTC = DateTime.UtcNow
										});
									});
									transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
								}

								parentRightNodePosition +=
									childNodeVal.RightNodePosition - childNodeVal.LeftNodePosition + 1;
							}

							transaction.Upsert(new DbCustomerRelationshipNode
							{
								fk_RootCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
								fk_ParentCustomerUID = (Guid) customerRelationship.ParentCustomerUID,
								FK_CustomerUID = (Guid) customerRelationship.ParentCustomerUID,
								LeftNodePosition = parentLeftNodePosition,
								RightNodePosition = parentRightNodePosition,
								LastCustomerRelationshipNodeUTC = DateTime.UtcNow
							});
						},
						() => transaction.Publish(messages)
					});
				}

				#endregion

				#region Parent Exists

				else
				{
					return transaction.Execute(new List<Action>
					{
						() =>
						{
							if (parentNodes.Any())
							{
								parentNodes
									?.ForEach(pNode =>
									{
										ProcessChildNodes(customerRelationship, pNode, childNodes, rootChild);
									});
							}
						},
						() => transaction.Publish(messages)
					});
				}

				#endregion
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating customer relationship : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DeleteCustomerRelationShip(Guid parentCustomerUID, Guid childCustomerUID,
												Guid? accountUID, DateTime actionUTC)
		{
			try
			{
				List<KafkaMessage> messages = CustomerTopics
					?.Select((topic) => new KafkaMessage
					{
						Topic = topic,
						Key = childCustomerUID.ToString(),
						Message = new
						{
							DeleteCustomerRelationShipEvent = new DeleteCustomerRelationshipEvent
							{
								ParentCustomerUID = parentCustomerUID,
								ChildCustomerUID = childCustomerUID,
								AccountCustomerUID = accountUID,
								ActionUTC = actionUTC,
								ReceivedUTC = DateTime.UtcNow
							}
						}
					})
					?.ToList();
				/*
				 * 1) If parentCustomerUID is null and childCustomerUID is not null, delete entire hierarchy
				 * 2) If child has only one rootCustomerUID then orphan it and make it as root
				 * 3) If child has more that one rootCustomerUID then orphan one child and delete 
				 * this child in other hierarchy
				*/
				var accountsCount = GetAccountsCount(parentCustomerUID, childCustomerUID);
				var isLastAccount = accountsCount > 1 ? false : true;
				if (isLastAccount && parentCustomerUID != null)
				{
					//Getting the Child Customers Node value from all the hierarchy
					List<DbCustomerRelationshipNode> nodeToBeDeletedList =
						GetCustomerRelationshipNodes(childCustomerUID);
					var toBeDeletedHavingParentCount = nodeToBeDeletedList
						.Count(e => e.fk_ParentCustomerUID == parentCustomerUID);

					if (nodeToBeDeletedList.Any(e => e.fk_ParentCustomerUID == parentCustomerUID))
					{
						#region Orphaning Child

						//delete all the duplicates other than one child
						if (toBeDeletedHavingParentCount > 1)
						{
							return transaction.Execute(new List<Action>
							{
								() =>
								{
									List<DbCustomerRelationshipNode> dbUpsertObjects =
										new List<DbCustomerRelationshipNode>();
									var nodeToBeOrphaned = nodeToBeDeletedList.First();
									DeleteTheHierarchy(nodeToBeOrphaned, true);
									nodeToBeDeletedList.Remove(nodeToBeOrphaned);

									foreach (var currentNodeVal in nodeToBeDeletedList)
									{
										var deleteQuery = "DELETE FROM md_customer_CustomerRelationshipNode" +
														" WHERE fk_RootCustomerUID = {0} " +
														"AND LeftNodePosition >= {1} AND RightNodePosition <= {2};";
										transaction.Delete(string.Format(deleteQuery,
											currentNodeVal.fk_RootCustomerUID.ToStringAndWrapWithUnhex() ??
											currentNodeVal.FK_CustomerUID.ToStringAndWrapWithUnhex(),
											currentNodeVal.LeftNodePosition, currentNodeVal.RightNodePosition));

										//Updating the right node value above from parent
										List<DbCustomerRelationshipNode> listToUpdateRight
											= GetRelationshipNodesForRootCustomer(currentNodeVal.fk_RootCustomerUID);
										if (listToUpdateRight.Any())
										{
											dbUpsertObjects.Clear();
											listToUpdateRight
												.FindAll(r => r.RightNodePosition >= currentNodeVal.RightNodePosition)
												?.ForEach((i) =>
												{
													dbUpsertObjects.Add(new DbCustomerRelationshipNode
													{
														CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
														fk_RootCustomerUID = i.fk_RootCustomerUID,
														fk_ParentCustomerUID = i.fk_ParentCustomerUID,
														FK_CustomerUID = i.FK_CustomerUID,
														LeftNodePosition = i.LeftNodePosition,
														RightNodePosition = i.RightNodePosition - (
																				currentNodeVal.RightNodePosition -
																				currentNodeVal.LeftNodePosition) + 1,
														LastCustomerRelationshipNodeUTC = DateTime.UtcNow
													});
												});
											transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
										}

										//Updating the Left node value of the nodes left value that are greater than 
										//parent's right node value
										List<DbCustomerRelationshipNode> listToUpdateLeft
											= GetRelationshipNodesForRootCustomer(currentNodeVal.fk_RootCustomerUID);
										if (listToUpdateLeft.Any())
										{
											dbUpsertObjects.Clear();
											listToUpdateLeft
												.FindAll(r => r.LeftNodePosition > currentNodeVal.RightNodePosition)
												?.ForEach((i) =>
												{
													dbUpsertObjects.Add(new DbCustomerRelationshipNode
													{
														CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
														fk_RootCustomerUID = i.fk_RootCustomerUID,
														fk_ParentCustomerUID = i.fk_ParentCustomerUID,
														FK_CustomerUID = i.FK_CustomerUID,
														RightNodePosition = i.RightNodePosition,
														LeftNodePosition = i.LeftNodePosition - (
																				currentNodeVal.RightNodePosition -
																				currentNodeVal.LeftNodePosition) + 1,
														LastCustomerRelationshipNodeUTC = DateTime.UtcNow
													});
												});
											transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
										}
									}
								},
								() => transaction.Publish(messages)
							});
						}
						//delete under the parent hierarchy as we have a duplicate structure elsewhere
						else if (nodeToBeDeletedList.Count() > 1 && toBeDeletedHavingParentCount == 1)
						{
							return transaction.Execute(new List<Action>
							{
								() =>
								{
									var nodeToBeOrphaned = nodeToBeDeletedList
										.Single(e => e.fk_ParentCustomerUID == parentCustomerUID);
									DeleteTheHierarchy(nodeToBeOrphaned, false);
								},
								() => transaction.Publish(messages)
							});
						}
						//this is the only copy of child so keep it as orphan
						else if (nodeToBeDeletedList.Count() == 1 && toBeDeletedHavingParentCount == 1)
						{
							return transaction.Execute(new List<Action>
							{
								() =>
								{
									var nodeToBeOrphaned = nodeToBeDeletedList
										.Single(e => e.fk_ParentCustomerUID == parentCustomerUID);
									DeleteTheHierarchy(nodeToBeOrphaned, true);
								},
								() => transaction.Publish(messages)
							});
						}

						#endregion
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				logger.LogError($"Exception {ex} \n Message {ex.Message} \n " +
								$"StackTrace {ex.StackTrace} \n Source {ex.Source}");
				throw;
			}
		}

		#endregion

		#region Hierarchy

		public CustomerHierarchyInfo GetHierarchyInformationForUser(
			string targetUserUid, string targetCustomerUid = "", bool topLevelsOnly = false)
		{
			var uuid = targetUserUid;
			var hierarchyInfo
				= new CustomerHierarchyInfo() { UserUID = uuid };

			if (string.IsNullOrWhiteSpace(uuid))
			{
				logger.LogInformation("Passed UserUID is empty");
				return hierarchyInfo;
			}

			logger.LogInformation($"Started getting associated customer hierarchy for user: {uuid}");
			var watches = new List<Stopwatch>();
			var hierarchies = new List<CustomerHierarchyNode>();
			var customerHierarchyNodes = new List<CustomerHierarchyNode>();
			watches.Add(new Stopwatch());
			watches.Last().Start();

			List<Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>> hie =
				GetHierarchyForUser(new Guid(targetUserUid), true, targetCustomerUid);

			watches.Last().Stop();
			watches.Add(new Stopwatch());
			watches.Last().Start();

			if (topLevelsOnly)
			{
				hie?.Where(e => e.Item1.CustomerUID == new Guid(e.Item4))
					?.ToList()
					?.ForEach((ud) =>
					{
						if (!customerHierarchyNodes.Any(o => o.CustomerUID == ud.Item1.CustomerUID.ToString()))
						{
							customerHierarchyNodes.Add(
								new CustomerHierarchyNode
								{
									CustomerUID = ud.Item1.CustomerUID.ToString(),
									CustomerId = ud.Item1.CustomerID,
									Name = ud.Item1.CustomerName,
									DisplayName = !string.IsNullOrEmpty(ud.Item1.NetworkDealerCode)
										? $"({ud.Item1.NetworkDealerCode}) {ud.Item1.CustomerName}"
										: ud.Item1.CustomerName,
									CustomerType = ((CustomerEnum.CustomerType)ud.Item1.fk_CustomerTypeID).ToString(),
									LeftNodePosition = ud.Item2.LeftNodePosition,
									RightNodePosition = ud.Item2.RightNodePosition,
									CustomerCode = ud.Item1.NetworkDealerCode
								});
						}
					});
			}
			else
			{
				// Group the records based on Dealers from UserCustomer
				hie = hie
					.GroupBy(o => new { o.Item1.CustomerUID, o.Item3?.NetworkCustomerCode })
					.Select(o => o.First())
					.ToList();
				List<string> userDealerGroupUID = hie.GroupBy(e => e.Item4).Select(e => e.Key).ToList();
				foreach (var userDealer in userDealerGroupUID)
				{
					#region Hierarchy Building

					//Build hierarchy for each value in User Customer
					List<Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>> userDealerGroup = hie
						.Where(e => e.Item4 == userDealer)
						.ToList();

					var rootCustGroup = userDealerGroup
						.GroupBy(e => e.Item2.fk_RootCustomerUID)
						.Select(e => e.Key)
						.ToList();

					foreach (var rootCust in rootCustGroup)
					{
						Queue<CustomerHierarchyNode> custs = new Queue<CustomerHierarchyNode>();
						//Sort Hierarchy by LeftNode position
						List<Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>> sortHierList =
							userDealerGroup
								.Where(e => e.Item2.fk_RootCustomerUID == rootCust)
								.OrderBy(e => e.Item2.LeftNodePosition)
								.ThenByDescending(e => e.Item3?.NetworkCustomerCode)
								.ToList();

						foreach (Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string> h in sortHierList)
						{
							if (h.Item1.fk_CustomerTypeID == (long)CustomerEnum.CustomerType.Customer
								|| h.Item1.fk_CustomerTypeID == (long)CustomerEnum.CustomerType.Dealer
								&& !custs.Any(o => o.CustomerUID == h.Item1.CustomerUID.ToString()))
							{
								custs.Enqueue(new CustomerHierarchyNode
								{
									CustomerUID = h.Item1.CustomerUID.ToString(),
									CustomerId = h.Item1.CustomerID,
									Name = h.Item1.CustomerName,
									DisplayName = BuildName(h),
									CustomerType = ((CustomerEnum.CustomerType)h.Item1.fk_CustomerTypeID).ToString(),
									LeftNodePosition = h.Item2.LeftNodePosition,
									RightNodePosition = h.Item2.RightNodePosition,
									CustomerCode = h.Item1.fk_CustomerTypeID ==
													(long)CustomerEnum.CustomerType.Customer
										? h.Item3?.NetworkCustomerCode
										: h.Item1.NetworkDealerCode,
								});
							}
						}

						if (custs.Any())
						{
							while (custs.Count != 0)
							{
								var parent = custs.Dequeue();
								BuildCustomerHierarchy(ref parent, ref custs);
								customerHierarchyNodes.Add(parent);
							}
						}
					}

					#endregion
				}
			}

			hierarchies.AddRange(customerHierarchyNodes);
			watches.Last().Stop();

			watches.Add(new Stopwatch());
			watches.Last().Start();

			if (string.IsNullOrEmpty(targetCustomerUid))
			{
				List<CustomerHierarchyNode> Cuslist = new List<CustomerHierarchyNode>();
				List<Tuple<DbCustomer, DbAccount>> custList =
					GetOnlyAssociatedCustomersbyUserUid(new Guid(targetUserUid));
				custList = custList.GroupBy(o => new { o.Item1.CustomerUID, o.Item2?.NetworkCustomerCode })
					.Select(o => o.First()).OrderBy(o => o.Item1.CustomerUID)
					.ThenByDescending(o => o.Item2?.NetworkCustomerCode).ToList();
				foreach (Tuple<DbCustomer, DbAccount> cus in custList)
				{
					if (Cuslist
						.Any(o => o.CustomerUID == cus.Item1.CustomerUID.ToString()
								&& o.CustomerCode != cus.Item2?.NetworkCustomerCode))
					{
						if (!string.IsNullOrEmpty(cus.Item2?.NetworkCustomerCode))
						{
							Cuslist.Add(new CustomerHierarchyNode
							{
								CustomerUID = cus.Item1.CustomerUID.ToString(),
								CustomerId = cus.Item1.CustomerID,
								Name = cus.Item1.CustomerName,
								DisplayName = string.IsNullOrEmpty(cus.Item2?.NetworkCustomerCode)
									? cus.Item1.CustomerName
									: $"({cus.Item2?.NetworkCustomerCode}) {cus.Item1.CustomerName}",
								CustomerType = ((CustomerEnum.CustomerType)cus.Item1.fk_CustomerTypeID).ToString(),
								CustomerCode = cus.Item2?.NetworkCustomerCode
							});
						}
					}
					else
					{
						Cuslist.Add(new CustomerHierarchyNode
						{
							CustomerUID = cus.Item1.CustomerUID.ToString(),
							CustomerId = cus.Item1.CustomerID,
							Name = cus.Item1.CustomerName,
							DisplayName = string.IsNullOrEmpty(cus.Item2?.NetworkCustomerCode)
								? cus.Item1.CustomerName
								: $"({cus.Item2?.NetworkCustomerCode}) {cus.Item1.CustomerName}",
							CustomerType = ((CustomerEnum.CustomerType)cus.Item1.fk_CustomerTypeID).ToString(),
							CustomerCode = cus.Item2?.NetworkCustomerCode
						});
					}
				}

				hierarchies.AddRange(Cuslist);
			}

			watches.Last().Stop();
			logger.LogDebug("TIME TAKEN :GetHierarchy: {0}ms, Building Hierarchy: {1}ms, " +
							"GetAssociatedCustomersbyUserUid: {2}ms , Total Time: {3}ms",
				watches.First().ElapsedMilliseconds, watches[1].ElapsedMilliseconds,
				watches.Last().ElapsedMilliseconds, watches.Select(e => e.ElapsedMilliseconds).Sum());
			hierarchyInfo.Customers = hierarchies;
			return hierarchyInfo;
		}

		public List<Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>> GetHierarchyForUser(
			Guid userUid, bool filterForHavingAssets, string targetCustomerUid = "")
		{
			var getHierarchyForDealerQuery = @"SELECT 
						    c.CustomerID,c.CustomerName,c.fk_CustomerTypeID,c.CustomerUID,
						    c.LastCustomerUTC,c.NetworkDealerCode,crn.CustomerRelationshipNodeID,
						    crn.fk_RootCustomerUID,crn.fk_ParentCustomerUID,crn.fk_CustomerUID,
						    crn.LeftNodePosition,crn.RightNodePosition,crn.LastCustomerRelationshipNodeUTC,
						    ca.AccountName,ca.DealerAccountCode,ca.CustomerAccountUID,
						    ca.NetworkCustomerCode,hex(dcrn.fk_CustomerUID) as UserDealerUID
						FROM
						    md_customer_CustomerRelationshipNode crn
						        JOIN
						    md_customer_Customer c ON crn.fk_CustomerUID = c.CustomerUID AND c.IsActive = 1
						        JOIN
						    md_customer_CustomerRelationshipNode dcrn ON crn.fk_RootCustomerUID = dcrn.fk_RootCustomerUID
						        AND crn.LeftNodePosition >= dcrn.LeftNodePosition
						        AND crn.RightNodePosition <= dcrn.RightNodePosition
						        LEFT JOIN
						    md_customer_CustomerAccount ca ON ca.fk_ChildCustomerUID = crn.Fk_CustomerUID
						        AND ca.fk_ParentCustomerUID = crn.fk_ParentCustomerUID
						WHERE dcrn.fk_CustomerUID IN (SELECT fk_CustomerUID
						        FROM md_customer_CustomerUser uc
						        JOIN md_customer_Customer c ON uc.fk_CustomerUID = c.CustomerUID
						        WHERE c.IsActive = 1 AND c.fk_CustomerTypeID = {1} AND uc.fk_UserUID = {0} {2})
						        AND crn.fk_RootCustomerUID = dcrn.fk_RootCustomerUID";

			var filterForAssets = string.Format(" AND (c.fk_CustomerTypeID != {0} OR " +
												"EXISTS(SELECT NULL FROM md_customer_CustomerAsset ac " +
												"INNER JOIN md_customer_CustomerAsset acd ON acd.fk_AssetUID = ac.fk_AssetUID " +
												"WHERE ac.fk_CustomerUID=c.CustomerUID AND acd.fk_CustomerUID = dcrn.fk_CustomerUID))",
				(int)CustomerEnum.CustomerType.Customer);

			var targetCustomerUidClause = string.IsNullOrWhiteSpace(targetCustomerUid)
				? string.Empty
				: string.Format(" AND uc.fk_CustomerUID = {0}",
					Guid.Parse(targetCustomerUid).ToStringAndWrapWithUnhex());

			var queryToExecuteUnformatted = filterForHavingAssets
				? getHierarchyForDealerQuery + filterForAssets
				: getHierarchyForDealerQuery;

			var queryToExecute = string.Format(queryToExecuteUnformatted,
				userUid.ToStringAndWrapWithUnhex(), (int)CustomerEnum.CustomerType.Dealer,
				targetCustomerUidClause);

			return transaction.Get<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>(queryToExecute,
				"CustomerRelationshipNodeID,AccountName,UserDealerUID");
		}

		public List<Tuple<DbCustomer, DbAccount>> GetOnlyAssociatedCustomersbyUserUid(Guid userUID)
		{
			var query = string.Format("SELECT c.CustomerID,c.CustomerName,c.fk_CustomerTypeID," +
									"c.CustomerUID,c.LastCustomerUTC,ca.AccountName,ca.NetworkCustomerCode,ca.CustomerAccountUID," +
									"ca.DealerAccountCode FROM md_customer_Customer c " +
									"JOIN md_customer_CustomerUser uc ON uc.fk_CustomerUID = c.CustomerUID and c.IsActive=1" +
									" LEFT join md_customer_CustomerAccount ca ON ca.fk_ChildCustomerUID = c.CustomerUID" +
									" WHERE uc.fk_UserUID = {0} AND c.fk_CustomerTypeID = {1} ",
				userUID.ToStringAndWrapWithUnhex(), (int)CustomerEnum.CustomerType.Customer);
			return transaction.Get<DbCustomer, DbAccount>(query, "AccountName");
		}

		#endregion

		#region Customer Account

		public int GetAccountsCount(Guid dealerUid, Guid customerUid)
		{
			var countQuery = "SELECT * FROM md_customer_CustomerAccount " +
							"WHERE fk_ParentCustomerUID = {0} AND fk_ChildCustomerUID = {1};";
			int? accountCount = transaction.Get<object>(string.Format(countQuery,
					dealerUid.ToStringAndWrapWithUnhex(), customerUid.ToStringAndWrapWithUnhex()))
				?.ToList()
				?.Count;
			return accountCount ?? 0;
		}

		#endregion

		#region Private Methods

		private static string BuildName(Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string> hierarchy)
		{
			return (CustomerEnum.CustomerType)hierarchy.Item1.fk_CustomerTypeID == CustomerEnum.CustomerType.Dealer
				? string.IsNullOrEmpty(hierarchy.Item1.NetworkDealerCode)
					? hierarchy.Item1.CustomerName
					: $"({hierarchy.Item1.NetworkDealerCode}) {hierarchy.Item1.CustomerName}"
				: string.IsNullOrEmpty(hierarchy.Item3?.NetworkCustomerCode)
					? hierarchy.Item1.CustomerName
					: $"({hierarchy.Item3?.NetworkCustomerCode}) {hierarchy.Item1.CustomerName}";
		}

		private void BuildCustomerHierarchy(ref CustomerHierarchyNode parent,
											ref Queue<CustomerHierarchyNode> custQueue)
		{
			while (custQueue.Any())
			{
				var crNode = custQueue.Peek();
				if (crNode.RightNodePosition < parent.RightNodePosition)
				{
					var child = custQueue.Dequeue();
					if (child.RightNodePosition - child.LeftNodePosition > 1)
					{
						BuildCustomerHierarchy(ref child, ref custQueue);
					}

					parent.Children.Add(child);
					while (custQueue.Any())
					{
						var nextchild = custQueue.Peek();
						if (child.CustomerUID == nextchild.CustomerUID)
						{
							nextchild = custQueue.Dequeue();
							if (!string.IsNullOrEmpty(nextchild.CustomerCode)
								&& child.CustomerCode != nextchild.CustomerCode)
							{
								parent.Children.Add(nextchild);
							}
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					break;
				}
			}
		}

		private void ProcessChildNodes(
			CreateCustomerRelationshipEvent customerRelationship,
			DbCustomerRelationshipNode parentNodeVal,
			List<DbCustomerRelationshipNode> childNodes,
			DbCustomerRelationshipNode rootChild)
		{
			try
			{
				var childCustomerUid = customerRelationship.ChildCustomerUID;
				var parentCustomerUid = (Guid)customerRelationship.ParentCustomerUID;
				List<DbCustomerRelationshipNode> dbUpsertObjects = new List<DbCustomerRelationshipNode>();

				//Parent Exists but Child Does not exists
				if (!childNodes.Any())
				{
					//Updating the right node value above from parent
					List<DbCustomerRelationshipNode> listToUpdateRight =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateRight.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateRight
							.FindAll(r => r.RightNodePosition >= parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition,
									RightNodePosition = i.RightNodePosition + 2,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Updating the right node value of nodes that are excluded above
					List<DbCustomerRelationshipNode> listToUpdateLeft =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateLeft.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateLeft
							.FindAll(r => r.LeftNodePosition > parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition + 2,
									RightNodePosition = i.RightNodePosition,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Inserting the current node
					transaction.Upsert(new DbCustomerRelationshipNode
					{
						fk_RootCustomerUID = parentNodeVal.fk_RootCustomerUID,
						fk_ParentCustomerUID = parentCustomerUid,
						FK_CustomerUID = childCustomerUid,
						LeftNodePosition = parentNodeVal.RightNodePosition,
						RightNodePosition = parentNodeVal.RightNodePosition + 1,
						LastCustomerRelationshipNodeUTC = DateTime.UtcNow
					});
				}
				//Parent exists but Child exists with Root CustomerUID as child's customeruid
				else if (rootChild != null)
				{
					var childNodeVal = rootChild;
					var nodeLength = childNodeVal.RightNodePosition - childNodeVal.LeftNodePosition + 1;

					//Updating the right node value above from parent
					List<DbCustomerRelationshipNode> listToUpdateRight =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateRight.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateRight
							.FindAll(r => r.RightNodePosition >= parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition,
									RightNodePosition = i.RightNodePosition + nodeLength,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Updating the left node value above from parent
					List<DbCustomerRelationshipNode> listToUpdateLeft =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateLeft.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateLeft
							.FindAll(r => r.LeftNodePosition > parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition + nodeLength,
									RightNodePosition = i.RightNodePosition,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					nodeLength = parentNodeVal.RightNodePosition - childNodeVal.LeftNodePosition;

					//Updating the right,left node value under the parent and changing the root customeruid
					List<DbCustomerRelationshipNode> listForRightLeftAndParentUpdate
						= GetRelationshipNodesForRootCustomer(childNodeVal.FK_CustomerUID);
					if (listForRightLeftAndParentUpdate.Any())
					{
						dbUpsertObjects.Clear();
						listForRightLeftAndParentUpdate.ForEach((i) =>
						{
							dbUpsertObjects.Add(new DbCustomerRelationshipNode
							{
								CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
								fk_RootCustomerUID = parentNodeVal.fk_RootCustomerUID,
								fk_ParentCustomerUID = i.fk_ParentCustomerUID,
								FK_CustomerUID = i.FK_CustomerUID,
								LeftNodePosition = i.LeftNodePosition + nodeLength,
								RightNodePosition = i.RightNodePosition + nodeLength,
								LastCustomerRelationshipNodeUTC = DateTime.UtcNow
							});
						});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Updating the childs parent customeruid
					List<DbCustomerRelationshipNode> childsToUpdateParentCustomerUid =
						GetRelationshipNodesForRootAndCustomer(
							parentNodeVal.fk_RootCustomerUID, childNodeVal.FK_CustomerUID);
					if (childsToUpdateParentCustomerUid.Any())
					{
						//Update the db records
						dbUpsertObjects.Clear();
						childsToUpdateParentCustomerUid.ForEach((i) =>
						{
							dbUpsertObjects.Add(new DbCustomerRelationshipNode
							{
								CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
								fk_RootCustomerUID = i.fk_RootCustomerUID,
								fk_ParentCustomerUID = parentNodeVal.FK_CustomerUID,
								FK_CustomerUID = i.FK_CustomerUID,
								LeftNodePosition = i.LeftNodePosition,
								RightNodePosition = i.RightNodePosition,
								LastCustomerRelationshipNodeUTC = DateTime.UtcNow
							});
						});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}
				}
				// Childs exists but the child rootCustomer is not the parentCustomerid.
				else
				{
					var childNodeVal = childNodes.First();
					var parentNodediff = childNodeVal.RightNodePosition - childNodeVal.LeftNodePosition + 1;

					//Update right node
					List<DbCustomerRelationshipNode> listToUpdateRight =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateRight.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateRight
							.FindAll(r => r.RightNodePosition >= parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition,
									RightNodePosition = i.RightNodePosition + parentNodediff,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Update left node 
					List<DbCustomerRelationshipNode> listToUpdateLeft =
						GetRelationshipNodesForRootCustomer(parentNodeVal.fk_RootCustomerUID);
					if (listToUpdateLeft.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateLeft
							.FindAll(r => r.LeftNodePosition > parentNodeVal.RightNodePosition)
							?.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition + parentNodediff,
									RightNodePosition = i.RightNodePosition,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Existing child node
					var existingChildNode = GetRelationshipNodesForRootAndCustomer(
						childNodeVal.fk_RootCustomerUID, childNodeVal.FK_CustomerUID)?.FirstOrDefault();
					if (existingChildNode != null)
					{
						List<DbCustomerRelationshipNode> relationshipNodesToInsert =
							GetRelationshipNodesForRootAndLeftRightPosition(
								childNodeVal.fk_RootCustomerUID, existingChildNode.LeftNodePosition,
								existingChildNode.RightNodePosition);
						if (relationshipNodesToInsert.Any())
						{
							dbUpsertObjects.Clear();
							relationshipNodesToInsert
								?.ForEach((i) =>
								{
									dbUpsertObjects.Add(new DbCustomerRelationshipNode
									{
										fk_RootCustomerUID = parentNodeVal.fk_RootCustomerUID,
										fk_ParentCustomerUID = i.fk_ParentCustomerUID,
										FK_CustomerUID = i.FK_CustomerUID,
										LeftNodePosition = i.LeftNodePosition
															- existingChildNode.LeftNodePosition +
															parentNodeVal.RightNodePosition,
										RightNodePosition = i.RightNodePosition
															- existingChildNode.LeftNodePosition +
															parentNodeVal.RightNodePosition,
										LastCustomerRelationshipNodeUTC = DateTime.UtcNow
									});
								});
							transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
						}
					}

					//Update parent
					List<DbCustomerRelationshipNode> availableNodesorParentUpdate =
						GetRelationshipNodesForRootAndCustomer(
							parentNodeVal.fk_RootCustomerUID, childNodeVal.FK_CustomerUID);
					if (availableNodesorParentUpdate.Any())
					{
						var nodeToUpdate = availableNodesorParentUpdate
							.OrderByDescending(i => i.CustomerRelationshipNodeID).First();
						transaction.Upsert(new DbCustomerRelationshipNode
						{
							CustomerRelationshipNodeID = nodeToUpdate.CustomerRelationshipNodeID,
							fk_RootCustomerUID = nodeToUpdate.fk_RootCustomerUID,
							fk_ParentCustomerUID = parentCustomerUid,
							FK_CustomerUID = nodeToUpdate.FK_CustomerUID,
							LeftNodePosition = nodeToUpdate.LeftNodePosition,
							RightNodePosition = nodeToUpdate.RightNodePosition,
							LastCustomerRelationshipNodeUTC = DateTime.UtcNow
						});
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Exception {ex} \n Message {ex.Message} \n " +
								$"StackTrace {ex.StackTrace} \n Source {ex.Source}");
				throw;
			}
		}

		private void DeleteTheHierarchy(
			DbCustomerRelationshipNode nodeToBeOrphaned, bool orphanTheChild = true)
		{
			try
			{
				List<DbCustomerRelationshipNode> dbUpsertObjects = new List<DbCustomerRelationshipNode>();
				if (orphanTheChild)
				{
					//Updating the left,right node,rootCustomerUID value of child
					List<DbCustomerRelationshipNode> listToUpdateLeftRightAndRoot =
						GetRelationshipNodesForRootAndLeftRightPosition(
							nodeToBeOrphaned.fk_RootCustomerUID, nodeToBeOrphaned.LeftNodePosition,
							nodeToBeOrphaned.RightNodePosition);
					if (listToUpdateLeftRightAndRoot.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateLeftRightAndRoot
							.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = nodeToBeOrphaned.FK_CustomerUID,
									fk_ParentCustomerUID = i.fk_ParentCustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition - (nodeToBeOrphaned.LeftNodePosition - 1),
									RightNodePosition = i.RightNodePosition - (nodeToBeOrphaned.LeftNodePosition - 1),
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}

					//Updating the ParentUID of orphaned childs root
					List<DbCustomerRelationshipNode> listToUpdateParent = GetRelationshipNodesForRootAndCustomer(
						nodeToBeOrphaned.FK_CustomerUID, nodeToBeOrphaned.FK_CustomerUID);
					if (listToUpdateParent.Any())
					{
						dbUpsertObjects.Clear();
						listToUpdateParent
							.ForEach((i) =>
							{
								dbUpsertObjects.Add(new DbCustomerRelationshipNode
								{
									CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
									fk_RootCustomerUID = i.fk_RootCustomerUID,
									fk_ParentCustomerUID = nodeToBeOrphaned.FK_CustomerUID,
									FK_CustomerUID = i.FK_CustomerUID,
									LeftNodePosition = i.LeftNodePosition,
									RightNodePosition = i.RightNodePosition,
									LastCustomerRelationshipNodeUTC = DateTime.UtcNow
								});
							});
						transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
					}
				}
				else
				{
					var query = "DELETE from md_customer_CustomerRelationshipNode " +
								"WHERE CustomerRelationShipNodeID > -1 " +
								"AND fk_RootCustomerUID = {0} AND LeftNodePosition >={1} AND RightNodePosition <= {2};";
					transaction.Delete(string.Format(query,
						nodeToBeOrphaned.fk_RootCustomerUID.ToStringAndWrapWithUnhex(),
						nodeToBeOrphaned.LeftNodePosition, nodeToBeOrphaned.RightNodePosition));
				}

				//Updating the right node value above from parent
				List<DbCustomerRelationshipNode> listToUpdateRight =
					GetRelationshipNodesForRootCustomer(nodeToBeOrphaned.fk_RootCustomerUID);
				if (listToUpdateRight.Any())
				{
					dbUpsertObjects.Clear();
					listToUpdateRight
						.FindAll(r => r.RightNodePosition > nodeToBeOrphaned.RightNodePosition)
						?.ForEach((i) =>
						{
							dbUpsertObjects.Add(new DbCustomerRelationshipNode
							{
								CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
								fk_RootCustomerUID = i.fk_RootCustomerUID,
								fk_ParentCustomerUID = i.fk_ParentCustomerUID,
								FK_CustomerUID = i.FK_CustomerUID,
								LeftNodePosition = i.LeftNodePosition,
								RightNodePosition = i.RightNodePosition -
													(nodeToBeOrphaned.RightNodePosition -
													nodeToBeOrphaned.LeftNodePosition + 1),
								LastCustomerRelationshipNodeUTC = DateTime.UtcNow
							});
						});
					transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
				}

				//Updating the left node value above from parent
				List<DbCustomerRelationshipNode> listToUpdateLeft =
					GetRelationshipNodesForRootCustomer(nodeToBeOrphaned.fk_RootCustomerUID);
				if (listToUpdateLeft.Any())
				{
					dbUpsertObjects.Clear();
					listToUpdateLeft
						.FindAll(l => l.LeftNodePosition > nodeToBeOrphaned.RightNodePosition)
						?.ForEach((i) =>
						{
							dbUpsertObjects.Add(new DbCustomerRelationshipNode
							{
								CustomerRelationshipNodeID = i.CustomerRelationshipNodeID,
								fk_RootCustomerUID = i.fk_RootCustomerUID,
								fk_ParentCustomerUID = i.fk_ParentCustomerUID,
								FK_CustomerUID = i.FK_CustomerUID,
								RightNodePosition = i.RightNodePosition,
								LeftNodePosition = i.LeftNodePosition -
													(nodeToBeOrphaned.RightNodePosition -
													nodeToBeOrphaned.LeftNodePosition + 1),
								LastCustomerRelationshipNodeUTC = DateTime.UtcNow
							});
						});
					transaction.Upsert<DbCustomerRelationshipNode>(dbUpsertObjects);
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Exception {ex} \n Message {ex.Message} \n " +
								$"StackTrace {ex.StackTrace} \n Source {ex.Source}");
				throw;
			}
		}

		private List<DbCustomerRelationshipNode> GetRelationshipNodesForRootCustomer(Guid rootCustomerUid)
		{
			var query = "SELECT CustomerRelationshipNodeID,fk_RootCustomerUID,fk_ParentCustomerUID" +
						",Fk_CustomerUID,LeftNodePosition,RightNodePosition,LastCustomerRelationshipNodeUTC " +
						"FROM md_customer_CustomerRelationshipNode " +
						"WHERE CustomerRelationShipNodeID > -1 and fk_RootCustomerUID = {0};";

			return transaction.GetWithTransaction<DbCustomerRelationshipNode>(string.Format(query,
				rootCustomerUid.ToStringAndWrapWithUnhex()), true)?.ToList();
		}

		private List<DbCustomerRelationshipNode> GetRelationshipNodesForRootAndCustomer(
			Guid rootCustomerUid, Guid customerUid)
		{
			var query = "SELECT CustomerRelationshipNodeID,fk_RootCustomerUID,fk_ParentCustomerUID" +
						",Fk_CustomerUID,LeftNodePosition,RightNodePosition,LastCustomerRelationshipNodeUTC " +
						"FROM md_customer_CustomerRelationshipNode WHERE CustomerRelationShipNodeID > -1 " +
						"AND fk_RootCustomerUID = {0} AND Fk_CustomerUID = {1};";

			return transaction.GetWithTransaction<DbCustomerRelationshipNode>(string.Format(query,
				rootCustomerUid.ToStringAndWrapWithUnhex(),
				customerUid.ToStringAndWrapWithUnhex()), true)?.ToList();
		}

		private List<DbCustomerRelationshipNode> GetRelationshipNodesForRootAndLeftRightPosition(
			Guid rootCustomerUid, int leftNodeValue, int rightNodeValue)
		{
			var query = "SELECT CustomerRelationshipNodeID,fk_RootCustomerUID,fk_ParentCustomerUID" +
						",Fk_CustomerUID,LeftNodePosition,RightNodePosition,LastCustomerRelationshipNodeUTC " +
						"FROM md_customer_CustomerRelationshipNode WHERE fk_RootCustomerUID = {0} " +
						"AND LeftNodePosition >= {1} AND RightNodePosition <= {2};";

			return transaction.GetWithTransaction<DbCustomerRelationshipNode>(string.Format(query,
					rootCustomerUid.ToStringAndWrapWithUnhex(), leftNodeValue, rightNodeValue), true)
				?.ToList();
		}

		private List<DbCustomerRelationshipNode> GetCustomerRelationshipNodes(Guid customerUid)
		{
			var query = "SELECT crn.CustomerRelationshipNodeID,crn.fk_RootCustomerUID,crn.fk_ParentCustomerUID," +
						"crn.FK_CustomerUID,crn.LeftNodePosition,crn.RightNodePosition,crn.LastCustomerRelationshipNodeUTC" +
						" FROM md_customer_CustomerRelationshipNode crn WHERE FK_CustomerUID = {0};";

			return transaction.GetWithTransaction<DbCustomerRelationshipNode>(string.Format(query,
					customerUid.ToStringAndWrapWithUnhex()), true)
				?.ToList();
		}

		#endregion
	}
}