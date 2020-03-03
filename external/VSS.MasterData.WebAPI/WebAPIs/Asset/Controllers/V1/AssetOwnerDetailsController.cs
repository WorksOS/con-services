using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
//using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.Asset.Filters;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.Asset.Controllers.V1
{
	[Route("v1/AssetOwnerDetails")]
	public class AssetOwnerDetailsV1Controller : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IAssetOwnerServices _assetOwnerRepository;
		private readonly IAssetServices _assetServices;
		
		#region Constructors
		public AssetOwnerDetailsV1Controller(IAssetOwnerServices assetOwnerRepository, IAssetServices assetServices, ILogger logger)
		{
			_assetOwnerRepository = assetOwnerRepository;
			_assetServices = assetServices;
			_logger = logger;
		}
		#endregion Constructors
		#region Public Methods

		// POST: api/assetowner
		/// <summary>
		/// Create / Update/ Delete an asset's Owner Information
		/// </summary>
		/// <param name="assetOwner">AssetOwner model</param>
		/// <remarks>Create/Update/Delete AssetOwner</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPost]
		public ActionResult AssetOwner([FromBody] AssetOwnerEvent assetOwner)
		{
			try
			{
				bool result;
				string validateMsg = validateAsetOwner(assetOwner);

				if (String.IsNullOrEmpty(validateMsg))
				{
					assetOwner.ReceivedUTC = DateTime.UtcNow;
					assetOwner.Action = DetermineAction(assetOwner.AssetUID.Value, assetOwner.Action);
					if(assetOwner.Action== Operation.Create)
					{
						if ((assetOwner.AssetOwnerRecord.DealerUID == null || assetOwner.AssetOwnerRecord.DealerUID == Guid.Empty) && 
							(assetOwner.AssetOwnerRecord.CustomerUID == null ||  assetOwner.AssetOwnerRecord.CustomerUID == Guid.Empty ) &&
							(assetOwner.AssetOwnerRecord.AccountUID == null || assetOwner.AssetOwnerRecord.AccountUID == Guid.Empty))
						{
							_logger.LogInformation(string.Format("Atlest one guid should be mandatory", assetOwner));
							return BadRequest("Atlest one guid is mandatory");
						}
					}
					result = PerformAction(assetOwner, assetOwner.Action);
					if (result)
					{
						_logger.LogInformation("AssetOwnerEvent for Asset:" + assetOwner.AssetUID + " Action: " + assetOwner.Action + " resulted in a success ");
						return Ok();
					}
				}
				else
					return BadRequest(validateMsg);
				
				_logger.LogError($"Failed to process message for asset {assetOwner.AssetUID}");
				throw new Exception("Failed to process message");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// To get Asset Owner details for given assetuid
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		public ActionResult GetAssetOwner(Guid assetUID)
		{
			try
			{
				if (assetUID == Guid.Empty)
				{
					_logger.LogInformation($"AssetUID is mandatory AssetUID :{assetUID}");
					return BadRequest("AssetUID is mandatory");
				}

				var userGuid = GetUserContext();

				if (userGuid == null || !userGuid.HasValue || _assetServices.ValidateAuthorizedCustomerByAsset(userGuid.Value, assetUID))
				{
					_logger.LogInformation(string.Format("Unauthorized User Access-Get AssetOwner called for {0}", assetUID));

					return BadRequest("Unauthorized User Access");
				}
 
				if (!_assetOwnerRepository.CheckExistingAssetOwner(assetUID))
				{
					_logger.LogInformation($"AssetUID does not exist AssetUID:{assetUID}");
					return BadRequest("No Such AssetUID exist");
				}
				var existingAssetOwner = _assetOwnerRepository.GetExistingAssetOwner(assetUID);
				return Ok(existingAssetOwner);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}
		#endregion Public Methods

		#region Private Methods

		private Guid? GetUserContext()
		{
			string userIdString = null;

			TPaaSJWT jwt = null;
			if (Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
			{
				try
				{
					jwt = new TPaaSJWT(headerValues);
				}
				catch (Exception ex)
				{
					jwt = null;
				}	
			}

			if (jwt != null && jwt.IsApplicationUserToken)
				userIdString = jwt.UserUid.ToString();
			else if (Request.Headers.TryGetValue("X-VisionLink-UserUid", out headerValues))
			{
				userIdString = headerValues.First();
			}

			return string.IsNullOrEmpty(userIdString) ? (Guid?)null : new Guid(userIdString);
		}
		private Operation DetermineAction(Guid assetGuid, Operation operation)
		{
			bool isExist = _assetOwnerRepository.CheckExistingAssetOwner(assetGuid);
			 
			if (isExist  && operation == Operation.Create)
			{
				operation = Operation.Update;
			}
			if ( !isExist && operation == Operation.Update)
			{
				operation = Operation.Create;
			}
			return operation;
		}

		private bool PerformAction(AssetOwnerEvent assetOwner, Operation operation)
		{
			bool result = false;
			switch (operation)
			{
				case Operation.Create:
					result = _assetOwnerRepository.CreateAssetOwnerEvent(assetOwner);
					break;
				case Operation.Update:
					result = Update(assetOwner);
					break;
				case Operation.Delete:
					result = _assetOwnerRepository.DeleteAssetOwnerEvent(assetOwner);
					break;
			}
			return result;
		}

		private bool Update(AssetOwnerEvent assetOwner)
		{
			AssetOwnerInfo existingAssetOwner = _assetOwnerRepository.GetExistingAssetOwner(assetOwner.AssetUID.Value);

			var compareResult = CheckExistingAssetOwnerForUpdate(existingAssetOwner, assetOwner);
			if (!compareResult)
			{
				Guid? customerUid = existingAssetOwner.CustomerUID == null ? (Guid?)null : new Guid(existingAssetOwner.CustomerUID);
				Guid? accountUid = existingAssetOwner.AccountUID == null? (Guid?)null : new Guid(existingAssetOwner.AccountUID);
				Guid? dealerUid = existingAssetOwner.DealerUID == null ? (Guid?)null : new Guid(existingAssetOwner.DealerUID);
				var assetOwnerEvent = new AssetOwnerEvent
				{
					AssetUID = assetOwner.AssetUID,
					AssetOwnerRecord = new ClientModel.AssetOwner
					{
						DealerUID = assetOwner.AssetOwnerRecord.DealerUID == null ? dealerUid : (assetOwner.AssetOwnerRecord.DealerUID == Guid.Empty ? null : assetOwner.AssetOwnerRecord.DealerUID),
						DealerName = assetOwner.AssetOwnerRecord.DealerName == null ? existingAssetOwner.DealerName : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.DealerName) ? null : assetOwner.AssetOwnerRecord.DealerName),
						CustomerName = assetOwner.AssetOwnerRecord.CustomerName == null ? existingAssetOwner.CustomerName : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.CustomerName) ? null : assetOwner.AssetOwnerRecord.CustomerName),
						AccountName = assetOwner.AssetOwnerRecord.AccountName == null ? existingAssetOwner.AccountName : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.AccountName) ? null : assetOwner.AssetOwnerRecord.AccountName),
						DealerAccountCode = assetOwner.AssetOwnerRecord.DealerAccountCode == null ? existingAssetOwner.DealerAccountCode : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.DealerAccountCode) ? null : assetOwner.AssetOwnerRecord.DealerAccountCode),
						NetworkCustomerCode = assetOwner.AssetOwnerRecord.NetworkCustomerCode == null ? existingAssetOwner.NetworkCustomerCode : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.NetworkCustomerCode) ? null : assetOwner.AssetOwnerRecord.NetworkCustomerCode),
						NetworkDealerCode = assetOwner.AssetOwnerRecord.NetworkDealerCode == null ? existingAssetOwner.NetworkDealerCode : (string.IsNullOrWhiteSpace(assetOwner.AssetOwnerRecord.NetworkDealerCode) ? null : assetOwner.AssetOwnerRecord.NetworkDealerCode),
						CustomerUID = assetOwner.AssetOwnerRecord.CustomerUID == null ? customerUid : (assetOwner.AssetOwnerRecord.CustomerUID == Guid.Empty ? null : assetOwner.AssetOwnerRecord.CustomerUID),
						AccountUID = assetOwner.AssetOwnerRecord.AccountUID == null ? accountUid : (assetOwner.AssetOwnerRecord.AccountUID == Guid.Empty ? null : assetOwner.AssetOwnerRecord.AccountUID)
					},
					Action = Operation.Update,
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				};
				return _assetOwnerRepository.UpdateAssetOwnerEvent(assetOwnerEvent);
			}
			return compareResult;
		}

		private bool CheckExistingAssetOwnerForUpdate(AssetOwnerInfo existingAssetOwner, AssetOwnerEvent assetowner)
		{
			bool areEqual = false;

			string customerUid = assetowner.AssetOwnerRecord.CustomerUID == null ? null : (assetowner.AssetOwnerRecord.CustomerUID == Guid.Empty ? null : new Guid(assetowner.AssetOwnerRecord.CustomerUID.ToString()).ToString("N").ToUpper());
			string dealerUid = assetowner.AssetOwnerRecord.DealerUID == null ? null : (assetowner.AssetOwnerRecord.DealerUID == Guid.Empty ? null : new Guid(assetowner.AssetOwnerRecord.DealerUID.ToString()).ToString("N").ToUpper());
			string accountUid = assetowner.AssetOwnerRecord.AccountUID == null ? null : (assetowner.AssetOwnerRecord.AccountUID == Guid.Empty ? null : new Guid(assetowner.AssetOwnerRecord.AccountUID.ToString()).ToString("N").ToUpper());
			
			string networkCustomerCode = assetowner.AssetOwnerRecord.NetworkCustomerCode;
			string dealerAccountCode = assetowner.AssetOwnerRecord.DealerAccountCode;
			string networkDealerCode = assetowner.AssetOwnerRecord.NetworkDealerCode;
			string accountName = assetowner.AssetOwnerRecord.AccountName;
			string customerName = assetowner.AssetOwnerRecord.CustomerName;
			string dealerName = assetowner.AssetOwnerRecord.DealerName;

			if (existingAssetOwner.DealerUID == dealerUid && existingAssetOwner.CustomerUID == customerUid && existingAssetOwner.NetworkCustomerCode == networkCustomerCode
			   && existingAssetOwner.AccountUID == accountUid && existingAssetOwner.DealerAccountCode == dealerAccountCode && existingAssetOwner.NetworkDealerCode == networkDealerCode
			   && existingAssetOwner.AccountName == accountName && existingAssetOwner.CustomerName == customerName && existingAssetOwner.DealerName == dealerName)
				areEqual = true;
			return areEqual;
		}

		private string validateAsetOwner(AssetOwnerEvent assetOwner)
		{
			string errorMsg=string.Empty;
			if (!ValidateGUID(assetOwner.AssetUID.Value))
				errorMsg = "AssetUID is mandatory";

			else if (assetOwner.AssetOwnerRecord.DealerUID == null && assetOwner.AssetOwnerRecord.CustomerUID == null && assetOwner.AssetOwnerRecord.AccountUID==null)
			{
				_logger.LogInformation(string.Format("Atlest one guid should be mandatory", assetOwner));
				errorMsg = "Atlest one guid is mandatory";
			}
			else if(assetOwner.Action == Operation.Delete && !_assetOwnerRepository.CheckExistingAssetOwner(assetOwner.AssetUID.Value))
			{
				_logger.LogInformation($"Delete AssetOwnerDetail called for non-existing asset {assetOwner.AssetUID}");
				errorMsg = "No Such AssetOwnerDetail exist.";
			}
			return errorMsg;
		}
		private bool ValidateGUID(Guid guid)
		{
			if (guid == Guid.Empty)
			{
				return false;
			}
			return true;
		}

		#endregion Private Methods

	}
}