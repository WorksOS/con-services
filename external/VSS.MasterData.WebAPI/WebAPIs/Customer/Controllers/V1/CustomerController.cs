using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Utilities;
using VSS.MasterData.WebAPI.Utilities.Enums;
using Newtonsoft.Json;

namespace VSS.MasterData.WebAPI.Customer.Controllers.V1
{
	/// <summary>
	/// 
	/// </summary>
	[Route("v1")]
	[ApiController]
	public class CustomerController : ControllerBase
	{
		#region Fields & Properties

		private readonly ICustomerService customerService;
		private readonly IAccountService accountService;
		private readonly IUserCustomerService userCustomerService;
		private readonly ICustomerAssetService customerAssetService;
		private readonly ILogger logger;

		#endregion

		/// <summary>
		/// Customer controller constructor
		/// </summary>
		public CustomerController(ICustomerService customerService,
								IAccountService accountService, IUserCustomerService userCustomerService,
								ICustomerAssetService customerAssetService, ILogger logger)
		{
			this.customerService = customerService;
			this.accountService = accountService;
			this.customerAssetService = customerAssetService;
			this.userCustomerService = userCustomerService;
			this.logger = logger;
		}

		#region Customer CRUD

		/// <summary>
		/// Create Customer
		/// </summary>
		/// <param name="customer">Create Customer model</param>
		/// <remarks>Create customer</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[Route("")]
		[HttpPost]
		public IActionResult CreateCustomer([FromBody] CreateCustomerEvent customer)
		{
			try
			{
				customer.ReceivedUTC = DateTime.UtcNow;
				if (!Enum.TryParse(customer.CustomerType.Trim(), true, out CustomerType customerType))
				{
					logger.LogInformation(Messages.InvalidCustomerType);
					return BadRequest(Messages.InvalidCustomerType);
				}

				if (customerType != CustomerType.Account
					&& customerService.GetCustomer(customer.CustomerUID) != null)
				{
					logger.LogInformation(Messages.CustomerAlreadyExists);
					return BadRequest(Messages.CustomerAlreadyExists);
				}

				if (customerType == CustomerType.Account
					&& accountService.GetAccount(customer.CustomerUID) != null)
				{
					logger.LogInformation(Messages.AccountAlreadyExists);
					return BadRequest(Messages.AccountAlreadyExists);
				}

				if (customerType == CustomerType.Account && accountService.CreateAccount(customer))
				{
					return Ok();
				}

				if (customerService.CreateCustomer(customer))
				{
					//NOTE : Bug 91897 - Adding the customertype check from converted enum 
					//using incoming payload since it fails to create relationship when 
					//"CustomerType" is allowed to pass integer values.
					//TODO : The change will not be required once Newtonsoft json usage is removed 
					//and datatype validation is stricter from the API user end.
					if (string.Equals(customerType.ToString(), CustomerType.Dealer.ToString(),
						StringComparison.OrdinalIgnoreCase))
					{
						var createCustomerRelationshipEvent = new CreateCustomerRelationshipEvent
						{
							ParentCustomerUID = null,
							ChildCustomerUID = customer.CustomerUID,
							ActionUTC = DateTime.UtcNow
						};

						var createCustomerRelationshipResponse
							= CreateCustomerRelationship(createCustomerRelationshipEvent);
						var responseData = (IStatusCodeActionResult)createCustomerRelationshipResponse;
						if (responseData != null && responseData.StatusCode != (int)HttpStatusCode.OK)
						{
							logger.LogInformation(
								$"Failed to create Customer Relationship Payload {createCustomerRelationshipEvent}");
						}
					}

					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Update customer
		/// </summary>
		/// <param name="customer">Update Customer model</param>
		/// <remarks>Update customer</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[Route("")]
		[HttpPut]
		public IActionResult UpdateCustomer([FromBody] UpdateCustomerEvent customer)
		{
			try
			{
				customer.ReceivedUTC = DateTime.UtcNow;
				var customerDetails = customerService.GetCustomer(customer.CustomerUID);
				var accountDetails = accountService.GetAccount(customer.CustomerUID);
				if (customerDetails == null && accountDetails == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				if (IsInvalidUpdateCustomer(customer))
				{
					logger.LogInformation(Messages.NoDataToUpdate);
					return BadRequest(Messages.NoDataToUpdate);
				}

				if (accountDetails != null && accountService.UpdateAccount(customer, accountDetails))
				{
					return Ok();
				}

				if (customerDetails != null && customerService.UpdateCustomer(customer, customerDetails))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Delete customer
		/// </summary>
		/// <param name="CustomerUID">Customer unique identifier</param>
		/// <param name="ActionUTC">Timestamp of event</param>
		/// <remarks>Delete customer</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[Route("")]
		[HttpDelete]
		public IActionResult DeleteCustomer(Guid CustomerUID, DateTime ActionUTC)
		{
			try
			{
				var customerDetails = customerService.GetCustomer(CustomerUID);
				var accountDetails = accountService.GetAccount(CustomerUID);

				if (customerDetails == null && accountDetails == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				if (customerDetails != null && customerService.DeleteCustomer(CustomerUID, ActionUTC))
				{
					return Ok();
				}

				if (accountDetails != null && accountService.DeleteAccount(CustomerUID, ActionUTC, accountDetails))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
			}
		}

		#endregion

		#region Customer Asset Associate/Dissociate

		/// <summary>
		/// Associate customer and asset
		/// </summary>
		/// <param name="customerAsset">Customer - Asset model</param>
		/// <remarks>Associate customer and asset</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("AssociateCustomerAsset")]
		public IActionResult AssociateCustomerAsset([FromBody] AssociateCustomerAssetEvent customerAsset)
		{
			try
			{
				customerAsset.ReceivedUTC = DateTime.UtcNow;
				if (customerService.GetCustomer(customerAsset.CustomerUID) == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				if (!Enum.TryParse(customerAsset.RelationType, true, out RelationType assetCustomerRelationShipType))
				{
					logger.LogInformation(Messages.InvalidRelationshipType);
					return BadRequest(Messages.InvalidRelationshipType);
				}

				var customerAssetInDb = customerAssetService
					.GetAssetCustomerByRelationType(customerAsset.CustomerUID, customerAsset.AssetUID,
						(int)assetCustomerRelationShipType);
				if (customerAssetInDb != null)
				{
					logger.LogInformation(string.Format(Messages.DuplicateCARequested,
						customerAsset.CustomerUID.ToString(), customerAsset.AssetUID.ToString()));
					return Conflict(Messages.AssociationAlreadyExists);
				}

				if (customerAssetService.AssociateCustomerAsset(customerAsset))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Dissociate customer and asset
		/// </summary>
		/// <param name="customerAsset">Customer - Asset model</param>
		/// <remarks>Dissociate customer and asset</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("DissociateCustomerAsset")]
		public IActionResult DissociateCustomerAsset([FromBody] DissociateCustomerAssetEvent customerAsset)
		{
			try
			{
				customerAsset.ReceivedUTC = DateTime.UtcNow;
				if (customerService.GetCustomer(customerAsset.CustomerUID) == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				var customerAssetInDb
					= customerAssetService.GetAssetCustomer(customerAsset.CustomerUID, customerAsset.AssetUID);
				if (customerAssetInDb == null)
				{
					logger.LogInformation(Messages.CustomerAssetDoesntExist);
					return BadRequest(Messages.CustomerAssetDoesntExist);
				}

				if (customerAssetService.DissociateCustomerAsset(customerAsset))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Customer User Relationship

		/// <summary>
		/// Create
		/// User Customer Relationship
		/// </summary>
		/// <param name="userCustomerRelationship">Create User Customer Relationship Model</param>
		/// <remarks>Create User Customer Relationship</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("CreateUserCustomerRelationship")]
		public IActionResult CreateUserCustomerRelationship(
			[FromBody] CreateUserCustomerRelationshipEvent userCustomerRelationship)
		{
			try
			{
				userCustomerRelationship.ReceivedUTC = DateTime.UtcNow;
				if (customerService.GetCustomer(userCustomerRelationship.CustomerUID) == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				if (userCustomerService.GetCustomerUser(userCustomerRelationship.CustomerUID,
						userCustomerRelationship.UserUID) != null)
				{
					logger.LogInformation(Messages.CustomerUserAlreadyExists);
					return BadRequest(Messages.CustomerUserAlreadyExists);
				}

				if (customerService.CreateUserCustomerRelationship(userCustomerRelationship))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Update User Customer Relationship
		/// </summary>
		/// <param name="userCustomerRelationship">Update User Customer Relationship Model</param>
		/// <remarks>Update User Customer Relationship</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("UpdateUserCustomerRelationship")]
		public IActionResult UpdateUserCustomerRelationship(
			[FromBody] UpdateUserCustomerRelationshipEvent userCustomerRelationship)
		{
			try
			{
				userCustomerRelationship.ReceivedUTC = DateTime.UtcNow;
				if (customerService.GetCustomer(userCustomerRelationship.CustomerUID) == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				if (userCustomerService.GetCustomerUser(userCustomerRelationship.CustomerUID,
						userCustomerRelationship.UserUID) == null)
				{
					logger.LogInformation(Messages.CustomerUserDoesntExist);
					return BadRequest(Messages.CustomerUserDoesntExist);
				}

				if (customerService.UpdateUserCustomerRelationship(userCustomerRelationship))
				{
					return Ok();
				}

				logger.LogWarning(Messages.PublishKafkaFailure);
				return BadRequest(Messages.PublishKafkaFailure);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Delete User Customer Relationship
		/// </summary>
		/// <param name="userCustomerRelationship">Delete User Customer Relationship Model</param>
		/// <remarks>Delete User Customer Relationship</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("DeleteUserCustomerRelationship")]
		public IActionResult DeleteUserCustomerRelationship(
			[FromBody] DeleteUserCustomerRelationshipEvent userCustomerRelationship)
		{
			try
			{
				userCustomerRelationship.ReceivedUTC = DateTime.UtcNow;
				if (userCustomerService.GetCustomerUser(userCustomerRelationship.CustomerUID,
						userCustomerRelationship.UserUID) == null)
				{
					logger.LogInformation(Messages.CustomerUserAssociationNotExists);
					return BadRequest(Messages.CustomerUserAssociationNotExists);
				}

				if (customerService.DeleteUserCustomerRelationship(userCustomerRelationship))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Customer User Associate/Dissociate

		/// <summary>
		/// Associate customer and user
		/// </summary>
		/// <param name="customerUser">Customer - User model</param>
		/// <remarks>Associate customer and user</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("AssociateCustomerUser")]
		public IActionResult AssociateCustomerUser([FromBody] AssociateCustomerUserEvent customerUser)
		{
			try
			{
				customerUser.ReceivedUTC = DateTime.UtcNow;
				if (customerService.GetCustomer(customerUser.CustomerUID) == null)
				{
					logger.LogInformation(Messages.CustomerDoesntExist);
					return BadRequest(Messages.CustomerDoesntExist);
				}

				var userCustomerInDb =
					userCustomerService.GetCustomerUser(customerUser.CustomerUID, customerUser.UserUID);
				if (userCustomerInDb != null)
				{
					logger.LogInformation(Messages.CustomerUserAlreadyExists);
					return BadRequest(Messages.CustomerUserAlreadyExists);
				}

				if (userCustomerService.AssociateCustomerUser(customerUser))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Dissociate customer and user
		/// </summary>
		/// <param name="customerUser">Customer - User model</param>
		/// <remarks>Dissociate customer and user</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("DissociateCustomerUser")]
		public IActionResult DissociateCustomerUser([FromBody] DissociateCustomerUserEvent customerUser)
		{
			try
			{
				customerUser.ReceivedUTC = DateTime.UtcNow;
				var userCustomerInDb =
					userCustomerService.GetCustomerUser(customerUser.CustomerUID, customerUser.UserUID);
				if (userCustomerInDb == null)
				{
					logger.LogInformation(Messages.CustomerUserAssociationNotExists);
					return BadRequest(Messages.CustomerUserAssociationNotExists);
				}

				if (userCustomerService.DissociateCustomerUser(customerUser))
				{
					return Ok();
				}

				logger.LogWarning(Messages.UnableToSaveToDb);
				return BadRequest(Messages.UnableToSaveToDb);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Dissociate customer and users
		/// </summary>
		/// <param name="customerUser">Bulk dissociate Customer - User model</param>
		/// <remarks>Dissociate customer and users</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("BulkDissociateCustomerUser")]
		public IActionResult BulkCustomerUserDissociation([FromBody] BulkDissociateCustomerUserEvent customerUser)
		{
			try
			{
				List<Guid> userList = customerUser.UserUID; //list of userUIDs to dissociate
				var currentUtc = DateTime.UtcNow;
				if (userCustomerService.BulkDissociateCustomerUser(
					customerUser.CustomerUID, userList, customerUser.ActionUTC))
				{
					return Ok();
				}

				logger.LogInformation(Messages.CustomerUsersDoesntExist);
				return BadRequest(Messages.CustomerUsersDoesntExist);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Customer Relationship

		/// <summary>
		/// Create Customer Relationship
		/// </summary>
		/// <param name="customerRelationship">
		/// CreateCustomerRelationshipEvent Model Object</param>
		/// <remarks>Create Customer Relationship</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpPost]
		[Route("customerrelationship")]
		public IActionResult CreateCustomerRelationship(CreateCustomerRelationshipEvent customerRelationship)
		{
			try
			{
				var isSuccess = true;
				if (customerRelationship.ChildCustomerUID == Guid.Empty
					&& customerRelationship.ParentCustomerUID == Guid.Empty ||
					customerRelationship.ChildCustomerUID == Guid.Empty
					&& customerRelationship.ParentCustomerUID == null)
				{
					logger.LogInformation(Messages.BothParentChildCustomerEmpty);
					return BadRequest(Messages.BothParentChildCustomerEmpty);
				}

				customerRelationship.ReceivedUTC = DateTime.UtcNow;
				if (customerRelationship.AccountCustomerUID != null)
				{
					var accountDetails = accountService.GetAccount(customerRelationship.AccountCustomerUID.Value);
					if (accountDetails != null)
					{
						isSuccess
							= accountService.CreateAccountCustomerRelationShip(customerRelationship, accountDetails);
					}
				}

				if (customerRelationship.ParentCustomerUID != customerRelationship.ChildCustomerUID)
				{
					var parentCustomerUID =
						customerRelationship.ParentCustomerUID ?? customerRelationship.ChildCustomerUID;
					List<DbCustomerRelationshipNode> customerRelationshipsInDb =
						customerService.GetCustomerRelationships(
							parentCustomerUID, customerRelationship.ChildCustomerUID);
					if (!customerRelationshipsInDb.Any())
					{
						isSuccess = customerService.CreateCustomerRelationShip(customerRelationship);
					}
				}

				if (isSuccess)
				{
					return Ok();
				}

				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Delete customer Relationship
		/// </summary>
		/// <param name="parentCustomerUID">Parent Customer unique identifier</param>
		/// <param name="childCustomerUID">Child Customer unique identifier</param>
		/// <param name="accountCustomerUID">Account Customer unique identifier</param>
		/// <param name="type">Delete type identifier</param>
		/// <param name="actionUTC">Timestamp of event</param>
		/// <remarks>Delete Customer Relationship</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad Request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpDelete]
		[Route("customerrelationship")]
		public IActionResult DeleteCustomerRelationship(Guid parentCustomerUID, Guid childCustomerUID,
														DateTime actionUTC, string type, Guid accountCustomerUID)
		{
			logger.LogInformation($"Received DeleteCustomerRelationshipEvent for  ParentCustomerUID - " +
								$"{parentCustomerUID} ChildCustomerUID - {childCustomerUID} ActionUTC - {actionUTC}");

			var isDeleteRelationshipSuccess = false;
			Guid? accountUID = accountCustomerUID == Guid.Empty ? (Guid?)null : accountCustomerUID;

			try
			{
				if (childCustomerUID == Guid.Empty || parentCustomerUID == Guid.Empty)
				{
					logger.LogInformation(Messages.InvalidCustomerUID);
					return BadRequest(Messages.InvalidCustomerUID);
				}

				if (parentCustomerUID != childCustomerUID && !customerService.IsCustomerRelationShipAlreadyExists(
						parentCustomerUID.ToString(), childCustomerUID.ToString()))
				{
					logger.LogInformation(Messages.CustomerDealerRelationNotExists);
					return BadRequest(Messages.CustomerDealerRelationNotExists);
				}

				if (!Enum.TryParse(type, out DeleteType deleteTypeEnum))
				{
					logger.LogInformation(Messages.InvalidDeleteType);
					return BadRequest(Messages.InvalidDeleteType);
				}

				if (parentCustomerUID != childCustomerUID)
				{
					if (customerService.DeleteCustomerRelationShip(
						parentCustomerUID, childCustomerUID, accountUID, actionUTC))
					{
						isDeleteRelationshipSuccess = true;
					}
					else
					{
						logger.LogWarning(Messages.UnableToSaveToDb);
						return BadRequest(Messages.UnableToSaveToDb);
					}
				}

				if (parentCustomerUID == childCustomerUID)
				{
					isDeleteRelationshipSuccess = true;
				}

				if (accountUID != null && isDeleteRelationshipSuccess & (parentCustomerUID == childCustomerUID))
				{
					var accountDetails = accountService.GetAccount(accountUID.Value);
					if (accountDetails != null)
					{
						if (accountService.DeleteAccountCustomerRelationShip(
							parentCustomerUID, childCustomerUID, accountDetails, actionUTC))
						{
							return Ok();
						}
						else
						{
							logger.LogWarning(Messages.UnableToDeleteCustomerAccountInDb);
							return BadRequest(Messages.UnableToDeleteCustomerAccountInDb);
						}
					}
				}

				if (accountUID != null && isDeleteRelationshipSuccess & (parentCustomerUID != childCustomerUID))
				{
					var accountDetails = accountService.GetAccount(accountUID.Value);
					if (accountDetails != null)
					{
						if (accountService.CreateAccountCustomerRelationShip(
							parentCustomerUID, childCustomerUID, accountDetails, actionUTC, type))
						{
							return Ok();
						}
						else
						{
							logger.LogWarning(Messages.UnableToUpdateCustomerAccountInDb);
							return BadRequest(Messages.UnableToUpdateCustomerAccountInDb);
						}
					}
				}

				return Ok();
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Customers

		/// <summary>
		/// Get associated customers for the user
		/// </summary>
		/// <param name="JWTAssertion">JWT Assertion to extract user details</param>
		/// <returns></returns>
		/// <remarks>Get Associated customer for user</remarks>
		[HttpGet]
		[Route("Customers/me")]
		public IActionResult GetAssociatedCustomers([FromHeader(Name = "X-JWT-Assertion")] string JWTAssertion)
		{
			try
			{
				if (!string.IsNullOrEmpty(JWTAssertion))
				{
					var jwt = new TPaaSJWT(JWTAssertion);

					logger.LogInformation($"Started getting associated customers for user: {jwt.UserUid}");
					List<DbCustomer> customersList = customerService.GetAssociatedCustomersbyUserUid(jwt.UserUid);
					List<AssociatedCustomer> associatedCustomers = customersList.Select(c => new AssociatedCustomer()
					{
						CustomerUID = c.CustomerUID,
						CustomerName = c.CustomerName,
						CustomerType = Enum.Parse<CustomerType>(c.fk_CustomerTypeID.ToString())
					})?.ToList();

					logger.LogInformation($"Retrieved {associatedCustomers.Count} associated customers " +
										$"for user: {jwt.UserUid}");

					return Ok(new CustomerListSuccessResponse()
					{
						Status = HttpStatusCode.OK,
						Metadata = new Metadata { Message = "Customers retrieved successfully" },
						Customers = associatedCustomers
					});
				}

				return BadRequest();
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Get associated customers for the Dealer user
		/// </summary>
		/// <param name="JWTAssertion">JWT Assertion to extract user details</param>
		/// <returns></returns>
		/// <remarks>Get Associated customer for Dealer user</remarks>
		[HttpGet]
		[Route("dealerscustomer")]
		public IActionResult GetAssociatedDealersCustomer([FromHeader(Name = "X-JWT-Assertion")] string JWTAssertion)
		{
			try
			{
				if (!string.IsNullOrEmpty(JWTAssertion))
				{
					var jwt = new TPaaSJWT(JWTAssertion);
					logger.LogInformation($"Started getting associated customers for user: {jwt.UserUid}");
					List<DbCustomer> customersList = customerService.GetAssociatedCustomersbyUserUid(jwt.UserUid);
					if (customersList.Count(customer => customer.fk_CustomerTypeID == (long)CustomerType.Dealer) == 1)
					{
						//Selecting the first dealer
						var selectedDealer =
							customersList.First(d => d.fk_CustomerTypeID == (long)CustomerType.Dealer);
						logger.LogInformation($"Selecting Associated Customers for the dealer" +
											$" {selectedDealer.CustomerUID}");

						List<DbCustomer> dealersAssetBasedCustomers =
							customerService.GetAssociatedCustomersForDealer(selectedDealer.CustomerUID);
						if (dealersAssetBasedCustomers?.Count > 0)
						{
							logger.LogDebug($"Found {dealersAssetBasedCustomers.Count}" +
											$" customers under this dealer {selectedDealer.CustomerUID}");
							List<ClientModel.Customer> customersListResult = dealersAssetBasedCustomers
								.ConvertAll(c => new ClientModel.Customer()
								{
									CustomerName = c.CustomerName,
									CustomerUID = c.CustomerUID,
									CustomerType = Enum.Parse<CustomerType>(c.fk_CustomerTypeID.ToString())
										.ToString().ToUpper()
								});
							return new JsonResult(customersListResult) { StatusCode = (int)HttpStatusCode.OK };
						}

						logger.LogInformation($"No Customers found under this dealer {selectedDealer.CustomerUID}");
					}
					else
					{
						logger.LogInformation($"No Dealers found under this user {jwt.UserUid}");
					}

					return new JsonResult(new List<ClientModel.Customer>())
					{
						StatusCode = (int)HttpStatusCode.OK
					};
				}

				logger.LogInformation(Messages.JWTTokenEmpty);
				return BadRequest(Messages.JWTTokenEmpty);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Gets the Customer name and type for array  of Customer UID
		/// </summary>
		/// <param name="arrCustomerUIDs">Customer Guids</param>
		/// <remarks>Gets  the Customer names with Type</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[HttpGet]
		[Route("Customers")]
		public IActionResult GetCustomerDataForCustomerUIDs([FromQuery] Guid[] arrCustomerUIDs)
		{
			try
			{
				List<DbCustomer> customersData = customerService.GetCustomerByCustomerGuids(arrCustomerUIDs);
				List<ClientModel.Customer> customers = customersData?
					.Select(c => new ClientModel.Customer
					{
						CustomerName = c.CustomerName,
						CustomerUID = new Guid(c.CustomerUID.ToString()),
						CustomerType = Enum.Parse<CustomerType>(c.fk_CustomerTypeID.ToString()).ToString()
					})
					.ToList();
				return Ok(customers);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Get Customers for Customer Names
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="maxResults"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("CustomerSearch")]
		public IActionResult GetCustomersForCustomerNames([FromQuery] string filter, int maxResults = 20)
		{
			try
			{
				List<Tuple<DbCustomer, DbAccount>> customersData =
					customerService.GetCustomersByNameSearch(filter, maxResults);
				List<CustomerResponse> customers = customersData
					?.Select(customer => new CustomerResponse()
					{
						CustomerName = customer.Item1.CustomerName,
						CustomerUID = new Guid(customer.Item1.CustomerUID.ToString()),
						CustomerType = Enum.Parse<CustomerType>(customer.Item1.fk_CustomerTypeID.ToString()).ToString(),
						NetworkCustomerCode = customer.Item2?.NetworkCustomerCode,
						NetworkDealerCode = customer.Item1.NetworkDealerCode
					})
					?.ToList();
				return Ok(customers);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Get customers for asset by AssetUID
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("AssetCustomers")]
		public IActionResult GetAssetCustomersByAssetUID([FromQuery] Guid assetUID)
		{
			try
			{
				IEnumerable<AssetCustomerResponse> customersForAsset = customerService
					.GetAssetCustomerByAssetGuid(assetUID)
					?.Select((customer) => new AssetCustomerResponse
					{
						CustomerName = customer.CustomerName,
						CustomerUID = customer.CustomerUID,
						CustomerType = customer.CustomerType != -1
							? ((CustomerType)customer.CustomerType).ToString()
							: null,
						ParentCustomerUID = customer.ParentCustomerUID ?? null,
						ParentName = customer.ParentCustomerUID.HasValue ? customer.ParentName : null,
						ParentCustomerType = customer.ParentCustomerUID.HasValue && customer.CustomerType != -1
							? ((CustomerType)customer.ParentCustomerType).ToString()
							: null
					});
				return Ok(customersForAsset);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return BadRequest(ex.Message);
			}
		}

		#endregion

		#region Hierarchy

		/// <summary>
		/// Gets account hierarchy for a user
		/// </summary>
		/// <param name="JWTAssertion">JWT Assertion to extract user details</param>
		/// <param name="targetCustomerUid">The toplevel customeruid to filter by</param>
		/// <param name="topLevelsOnly">Get only the top level customers/dealers with NO children!</param>
		/// <returns></returns>
		[HttpGet]
		[Route("accounthierarchy")]
		public IActionResult GetAccountHierarchy([FromHeader(Name = "X-JWT-Assertion")] string JWTAssertion,
												string targetCustomerUid = "", bool topLevelsOnly = false)
		{
			try
			{
				if (string.IsNullOrEmpty(JWTAssertion))
				{
					logger.LogError(Messages.JWTTokenEmpty);
					return BadRequest(Messages.JWTTokenEmpty);
				}

				var jwt = new TPaaSJWT(JWTAssertion);

				var targetUserUid = jwt.UserUid;
				var sw = new Stopwatch();
				sw.Start();
				var customerHierarchyInformation = customerService.GetHierarchyInformationForUser(
					targetUserUid.ToString(), targetCustomerUid, topLevelsOnly);
				sw.Stop();

				logger.LogDebug("TIME TAKEN GetAccountHierarchy took {0} ms.", sw.ElapsedMilliseconds);
				return new JsonResult(customerHierarchyInformation,
					new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						DefaultValueHandling = DefaultValueHandling.Ignore
					})
				{ StatusCode = (int)HttpStatusCode.OK };
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Gets account hierarchy for a target user
		/// </summary>
		/// <param name="userUid">User uid to get the hierarchy for</param>
		/// <returns></returns>
		[HttpGet]
		[Route("accounthierarchy/targetuser")]
		public IActionResult GetAccountHierarchyForTargetUser([FromQuery] string userUid)
		{
			try
			{
				if (Guid.TryParse(userUid, out var targetUserUid))
				{
					var customerHierarchy = customerService.GetHierarchyInformationForUser(targetUserUid.ToString());
					return new JsonResult(customerHierarchy) { StatusCode = (int)HttpStatusCode.OK };
				}

				logger.LogError(Messages.InvalidUserUid + $" {userUid}");
				return BadRequest(Messages.InvalidUserUid);
			}
			catch (Exception ex)
			{
				logger.LogError(string.Format(Messages.ExceptionOccured, ex.Message, ex.StackTrace));
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Private Methods

		#region Validations

		private bool IsInvalidUpdateCustomer(UpdateCustomerEvent customer)
		{
			return string.IsNullOrEmpty(customer.CustomerName)
					&& string.IsNullOrEmpty(customer.BSSID)
					&& string.IsNullOrEmpty(customer.DealerAccountCode)
					&& string.IsNullOrEmpty(customer.DealerNetwork)
					&& string.IsNullOrEmpty(customer.NetworkCustomerCode)
					&& string.IsNullOrEmpty(customer.NetworkDealerCode)
					&& string.IsNullOrEmpty(customer.PrimaryContactEmail)
					&& string.IsNullOrEmpty(customer.FirstName)
					&& string.IsNullOrEmpty(customer.LastName)
					&& !customer.IsActive.HasValue;
		}

		#endregion

		#endregion
	}
}