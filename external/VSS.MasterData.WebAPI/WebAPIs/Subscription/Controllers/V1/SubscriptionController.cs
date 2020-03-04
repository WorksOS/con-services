using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Utilities.Enums;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.Subscription.Controllers.V1
{
	/// <summary>
	/// This Controller handles CRUD methods for Asset,Customer and Project Subscriptions.
	/// </summary>
	[Route("v1")]
	[ApiController]
	public class SubscriptionV1Controller : ControllerBase
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ISubscriptionService subscriptionService;

		private const int DefaultPayloadMaxSize = 1500;
		private readonly int BulkPayloadMaxCount;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="subscriptionService"></param>
		/// <param name="configuration"></param>
		/// <param name="logger"></param>
		public SubscriptionV1Controller(ISubscriptionService subscriptionService, IConfiguration configuration,
										ILogger logger)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.subscriptionService = subscriptionService;
			BulkPayloadMaxCount = !string.IsNullOrWhiteSpace(configuration["BulkPayloadMaxCount"])
				? Convert.ToInt32(configuration["BulkPayloadMaxCount"])
				: DefaultPayloadMaxSize;
		}

		#region Bulk Asset Subscription

		// POST: api/assetSubscriptions
		/// <summary>
		/// Create Multiple Asset Subscriptions
		/// </summary>
		/// <param name="assetSubscriptions">Multiple AssetSubscription Model</param>
		/// <remarks>Create Multiple Asset Subscriptions</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Assets")]
		[HttpPost]
		public ActionResult CreateAssetSubscriptions([FromBody] CreateAssetSubscriptions assetSubscriptions)
		{
			try
			{
				if (assetSubscriptions.CreateAssetSubscriptionEvents != null &&
					assetSubscriptions.CreateAssetSubscriptionEvents.Count > BulkPayloadMaxCount)
				{
					logger.LogInformation("Bulk CreateAssetSubscription payload count exceeds the threshold limit.");
					return BadRequest("Bulk CreateAssetSubscription payload count exceeds the threshold limit.");
				}

				SubscriptionSource subscriptionSource;

				foreach (var subscription in assetSubscriptions.CreateAssetSubscriptionEvents)
				{
					if (!Enum.TryParse(subscription.Source, true, out subscriptionSource))
					{
						logger.LogInformation("Invalid source value for the SubscriptionUID:- " +
											subscription.SubscriptionUID);
						return BadRequest("Invalid source value for the SubscriptionUID:- " +
										subscription.SubscriptionUID);
					}

					if (!subscription.DeviceUID.HasValue)
					{
						logger.LogInformation("Provide DeviceUID for the SubscriptionUID " +
											subscription.SubscriptionUID);
						return BadRequest("Provide DeviceUID for the SubscriptionUID " + subscription.SubscriptionUID);
					}

					subscription.StartDate = subscription.StartDate.ToMySqlDateTimeOverflowCorrection();
					subscription.EndDate = subscription.EndDate.ToMySqlDateTimeOverflowCorrection();
					subscription.ReceivedUTC = DateTime.UtcNow;
				}

				if (subscriptionService.CreateAssetSubscriptions(assetSubscriptions.CreateAssetSubscriptionEvents))
				{
					return Ok();
				}

				logger.LogInformation("Unable to save to db.Make sure request is not duplicated.");
				return BadRequest("Unable to save to db. Make sure request is not duplicated.");
			}

			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid Asset Subscription Type"))
				{
					logger.LogInformation("Invalid Asset Subscription Type");
					return BadRequest("Invalid Asset Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		// PUT: api/assetSubscriptions
		/// <summary>
		/// Update Multiple Asset Subscriptions
		/// </summary>
		/// <param name="assetSubscriptions">Multiple AssetSubscription Model</param>
		/// <remarks>Update Multiple existing Asset Subscriptions</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Assets")]
		[HttpPut]
		public ActionResult UpdateAssetSubscriptions([FromBody] UpdateAssetSubscriptions assetSubscriptions)
		{
			try
			{
				if (assetSubscriptions.UpdateAssetSubscriptionEvents != null &&
					assetSubscriptions.UpdateAssetSubscriptionEvents.Count > BulkPayloadMaxCount)
				{
					logger.LogInformation("Bulk UpdateAssetSubscription payload count exceeds the threshold limit. ");
					return BadRequest("Bulk UpdateAssetSubscription payload count exceeds the threshold limit.");
				}

				SubscriptionSource subscriptionSource;

				foreach (var subscription in assetSubscriptions.UpdateAssetSubscriptionEvents)
				{
					if (!Enum.TryParse(subscription.Source, true, out subscriptionSource))
					{
						logger.LogInformation("Invalid source value for the SubscriptionUID:- " +
											subscription.SubscriptionUID);
						return BadRequest("Invalid source value for the SubscriptionUID:-" +
										subscription.SubscriptionUID);
					}

					if (!subscription.CustomerUID.HasValue || !subscription.AssetUID.HasValue ||
						!subscription.StartDate.HasValue || !subscription.EndDate.HasValue ||
						!subscription.DeviceUID.HasValue)
					{
						logger.LogInformation("All Fields are Mandatory for the SubscriptionUID " +
											subscription.SubscriptionUID);
						return BadRequest("All Fields are Mandatory for the SubscriptionUID " +
										subscription.SubscriptionUID);
					}

					subscription.StartDate = subscription.StartDate.ToMySqlDateTimeOverflowCorrection();
					subscription.EndDate = subscription.EndDate.ToMySqlDateTimeOverflowCorrection();
					subscription.ReceivedUTC = DateTime.UtcNow;
				}

				if (subscriptionService.UpdateAssetSubscriptions(assetSubscriptions.UpdateAssetSubscriptionEvents))
				{
					return Ok();
				}

				logger.LogInformation(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
				return BadRequest(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
			}

			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid Asset Subscription Type"))
				{
					logger.LogInformation("Invalid Asset Subscription Type");
					return BadRequest("Invalid Asset Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		#endregion

		/// <summary>
		/// Create Asset Subscription
		/// </summary>
		/// <param name="assetSubscription">CreateAssetSubscription model</param>
		/// <remarks>Create new Asset Subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Asset")]
		[HttpPost]
		public ActionResult CreateAssetSubscription([FromBody] CreateAssetSubscriptionEvent assetSubscription)
		{
			try
			{
				if (string.IsNullOrEmpty(assetSubscription.Source))
				{
					assetSubscription.Source = SubscriptionSource.Store.ToString();
				}

				if (!Enum.TryParse(assetSubscription.Source, true, out SubscriptionSource _))
				{
					logger.LogInformation($"Invalid Source Value:{assetSubscription.Source}");
					return BadRequest("Invalid Source Value");
				}

				if (subscriptionService.CheckExistingSubscription(assetSubscription.SubscriptionUID.Value,
					"AssetSubscriptionEvent"))
				{
					logger.LogInformation("Asset Subscription already exists!");
					return BadRequest("Asset Subscription already exists!");
				}

				assetSubscription.StartDate = assetSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				assetSubscription.EndDate = assetSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				assetSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.CreateAssetSubscription(assetSubscription))
				{
					return Ok();
				}

				logger.LogWarning("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid Asset Subscription Type"))
				{
					logger.LogWarning("Invalid Asset Subscription Type");
					return BadRequest("Invalid Asset Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Update Asset Subscription
		/// </summary>
		/// <param name="assetSubscription">UpdateAssetSubscription model</param>
		/// <remarks>Updates existing Asset subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Asset")]
		[HttpPut]
		public ActionResult UpdateAssetSubscription([FromBody] UpdateAssetSubscriptionEvent assetSubscription)
		{
			try
			{
				if (!string.IsNullOrEmpty(assetSubscription.Source) &&
					!Enum.TryParse(assetSubscription.Source, true, out SubscriptionSource _))
				{
					logger.LogWarning("Invalid Source Value");
					return BadRequest("Invalid Source Value");
				}

				assetSubscription.StartDate = assetSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				assetSubscription.EndDate = assetSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				assetSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.UpdateAssetSubscription(assetSubscription))
				{
					return Ok();
				}

				logger.LogWarning("Update Asset Subscription Request Failed");
				return BadRequest("Update Asset Subscription Request Failed");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Asset Subscription not exists!"))
				{
					logger.LogWarning("Asset Subscription not exists!");
					return BadRequest("Asset Subscription not exists!");
				}

				if (ex.Message.Contains("Invalid Asset Subscription Type"))
				{
					logger.LogWarning("Invalid Asset Subscription Type");
					return BadRequest("Invalid Asset Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Create Customer Subscription
		/// </summary>
		/// <param name="customerSubscription">CreateCustomerSubscriptionEvent model</param>
		/// <remarks>Create new customer Subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Customer")]
		[HttpPost]
		public ActionResult CreateCustomerSubscription([FromBody] CreateCustomerSubscriptionEvent customerSubscription)
		{
			try
			{
				if (subscriptionService.CheckExistingSubscription(customerSubscription.SubscriptionUID.Value,
					"CustomerSubscriptionEvent"))
				{
					logger.LogInformation("Customer Subscription already exists!");
					return BadRequest("Customer Subscription already exists!");
				}

				customerSubscription.StartDate = customerSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				customerSubscription.EndDate = customerSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				customerSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.CreateCustomerSubscription(customerSubscription))
				{
					return Ok();
				}

				logger.LogInformation("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid Customer Subscription Type"))
				{
					logger.LogInformation("Invalid Customer Subscription Type");
					return BadRequest("Invalid Customer Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Update Customer Subscription
		/// </summary>
		/// <param name="customerSubscription">UpdateCustomerSubscriptionEvent model</param>
		/// <remarks>Updates existing customer subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Customer")]
		[HttpPut]
		public ActionResult UpdateCustomerSubscription([FromBody] UpdateCustomerSubscriptionEvent customerSubscription)
		{
			try
			{
				customerSubscription.StartDate = customerSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				customerSubscription.EndDate = customerSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				customerSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.UpdateCustomerSubscription(customerSubscription))
				{
					return Ok();
				}

				logger.LogInformation(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
				return BadRequest(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Customer Subscription not exists!"))
				{
					logger.LogInformation("Customer Subscription not exists!");
					return BadRequest("Customer Subscription not exists!");
				}

				if (ex.Message.Contains("Update Customer Subscription Request should have atleast one field to update"))
				{
					logger.LogInformation(
						"Update Customer Subscription Request should have atleast one field to update");
					return BadRequest("Update Customer Subscription Request should have atleast one field to update");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Create Project Subscription
		/// </summary>
		/// <param name="projectSubscription">CreateProjectSubscriptionEvent model</param>
		/// <remarks>Create new project Subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Project")]
		[HttpPost]
		public ActionResult CreateProjectSubscription([FromBody] CreateProjectSubscriptionEvent projectSubscription)
		{
			try
			{
				if (subscriptionService.CheckExistingSubscription(projectSubscription.SubscriptionUID.Value,
					"ProjectSubscriptionEvent"))
				{
					logger.LogInformation("Project Subscription already exists!");
					return BadRequest("Project Subscription already exists!");
				}

				projectSubscription.StartDate = projectSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				projectSubscription.EndDate = projectSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				projectSubscription.ReceivedUTC = DateTime.UtcNow;


				if (subscriptionService.CreateProjectSubscription(projectSubscription))
				{
					return Ok();
				}

				logger.LogInformation("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid Project Subscription Type"))
				{
					logger.LogInformation("Invalid Project Subscription Type");
					return BadRequest("Invalid Project Subscription Type");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Update Project Subscription
		/// </summary>
		/// <param name="projectSubscription">UpdateProjectSubscriptionEvent model</param>
		/// <remarks>Updates existing project subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Project")]
		[HttpPut]
		public ActionResult UpdateProjectSubscription([FromBody] UpdateProjectSubscriptionEvent projectSubscription)
		{
			try
			{
				projectSubscription.StartDate = projectSubscription.StartDate.ToMySqlDateTimeOverflowCorrection();
				projectSubscription.EndDate = projectSubscription.EndDate.ToMySqlDateTimeOverflowCorrection();
				projectSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.UpdateProjectSubscription(projectSubscription))
				{
					return Ok();
				}

				logger.LogInformation(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
				return BadRequest(
					"SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Project Subscription not exists!"))
				{
					logger.LogInformation("Project Subscription not exists!");
					return BadRequest("Project Subscription not exists!");
				}

				if (ex.Message.Contains("Invalid Project Subscription Type"))
				{
					logger.LogInformation("Invalid Project Subscription Type");
					return BadRequest("Invalid Project Subscription Type");
				}

				if (ex.Message.Contains("Update Project Subscription Request should have atleast one field to update"))
				{
					logger.LogInformation(
						"Update Project Subscription Request should have atleast one field to update");
					return BadRequest("Update Project Subscription Request should have atleast one field to update");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Associate Project Subscription
		/// </summary>
		/// <param name="associateProjectSubscription">AssociateProjectSubscriptionEvent model</param>
		/// <remarks>Associate project with project Subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Project/AssociateProjectSubscription")]
		[HttpPost]
		public ActionResult AssociateProjectSubscription(
			[FromBody] AssociateProjectSubscriptionEvent associateProjectSubscription)
		{
			try
			{
				associateProjectSubscription.EffectiveDate =
					associateProjectSubscription.EffectiveDate.ToMySqlDateTimeOverflowCorrection();
				associateProjectSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.AssociateProjectSubscription(associateProjectSubscription))
				{
					return Ok();
				}

				logger.LogInformation("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid ProjectSubscriptionUID"))
				{
					logger.LogInformation("Invalid ProjectSubscriptionUID");
					return BadRequest("Invalid ProjectSubscriptionUID");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Dissociate Project Subscription
		/// </summary>
		/// <param name="dissociateProjectSubscription">DissociateProjectSubscriptionEvent model</param>
		/// <remarks>Dissociate project with project Subscription</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Project/DissociateProjectSubscription")]
		[HttpPost]
		public ActionResult DissociateProjectSubscription(
			[FromBody] DissociateProjectSubscriptionEvent dissociateProjectSubscription)
		{
			try
			{
				dissociateProjectSubscription.EffectiveDate =
					dissociateProjectSubscription.EffectiveDate.ToMySqlDateTimeOverflowCorrection();
				dissociateProjectSubscription.ReceivedUTC = DateTime.UtcNow;

				if (subscriptionService.DissociateProjectSubscription(dissociateProjectSubscription))
				{
					return Ok();
				}

				logger.LogInformation("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Invalid ProjectSubscriptionUID"))
				{
					logger.LogInformation("Invalid ProjectSubscriptionUID");
					return BadRequest("Invalid ProjectSubscriptionUID");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Get all the Subscription for an Asset
		/// </summary>
		/// <param name="assetGuid">Asset Guid</param>
		/// <remarks>Gets all the existing Subscription with Start and End Date</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Asset/{assetGuid:guid}")]
		[HttpGet]
		public ActionResult GetSubscriptionByAssetId(Guid assetGuid)
		{
			try
			{
				if (!ValidateGUID(assetGuid))
				{
					logger.LogInformation("Invalid Input");
					return BadRequest("Invalid Input");
				}

				var assetSubscription = subscriptionService.GetSubscriptionForAsset(assetGuid);

				return Ok(assetSubscription);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Get all the Subscription for a Customer
		/// </summary>
		/// <param name="customerGuid">Customer GUID</param>
		/// <remarks>Gets all the existing Subscription with Start and End Date</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Customer/{customerGuid:guid}")]
		[HttpGet]
		public ActionResult GetSubscriptionByCustomerId(Guid customerGuid)
		{
			try
			{
				if (!ValidateGUID(customerGuid))
				{
					logger.LogInformation("Invalid Input");
					return BadRequest("Invalid Input");
				}

				IEnumerable<CustomerSubscriptionModel> subscriptions =
					subscriptionService.GetSubscriptionForCustomer(customerGuid);
				var subscriptionList = new CustomerSubscriptionList() {Subscriptions = subscriptions.ToList()};
				return Ok(subscriptionList);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Get all the Active Project Subscription for a Customer
		/// </summary>
		/// <param name="customerGuid">Customer GUID</param>
		/// <remarks>Gets all the existing active Project Subscription with Start and End Date</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("Project/{customerGuid:guid}")]
		[HttpGet]
		public ActionResult GetActiveProjectSubscriptionByCustomerId(Guid customerGuid)
		{
			try
			{
				if (!ValidateGUID(customerGuid))
				{
					logger.LogInformation("Invalid Input");
					return BadRequest("Invalid Input");
				}

				IEnumerable<ActiveProjectCustomerSubscriptionModel> subscriptions =
					subscriptionService.GetActiveProjectSubscriptionForCustomer(customerGuid);
				var subscriptionList = new ActiveProjectCustomerSubscriptionList()
					{Subscriptions = subscriptions.ToList()};
				return Ok(subscriptionList);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return BadRequest(ex.Message);
			}
		}

		private bool ValidateGUID(Guid guid)
		{
			if (guid == Guid.Empty)
			{
				return false;
			}

			return true;
		}
	}
}