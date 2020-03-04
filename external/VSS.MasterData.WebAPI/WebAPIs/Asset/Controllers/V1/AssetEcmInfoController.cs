using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
//using Newtonsoft.Json;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;

namespace VSS.MasterData.WebAPI.Asset.Controllers.V1
{
	[Route("v1/AssetEcmInfo")]
	public class AssetEcmInfoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IAssetECMInfoServices _assetECMInfoRepository;
		private readonly IAssetServices _assetServices;

		public AssetEcmInfoController(IAssetECMInfoServices assetECMInfoRepository, IAssetServices assetServices, ILogger logger)
		{
			_assetECMInfoRepository = assetECMInfoRepository;
			_assetServices = assetServices;
			_logger = logger;
		}

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
				catch (Exception esx)
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		public ActionResult GetAssetECMInfoByAssetUID(Guid assetUID)
		{
			try
			{
				if (!ValidateGUID(assetUID))
					return BadRequest("Invalid Asset UID");

				AssetECMInfoResponse assetecm = new AssetECMInfoResponse();
				var assetecmdata = new List<AssetECM>();

				try
				{
					_logger.LogInformation($"Get AssetECMInfo called for {assetUID}");

					var userGuid = GetUserContext();

					if (userGuid == null || !userGuid.HasValue || _assetServices.ValidateAuthorizedCustomerByAsset(userGuid.Value, assetUID))
					{
						_logger.LogInformation(string.Format("Unauthorized User Access-Get AssetECMInfo called for {0}", assetUID));

						return BadRequest("Unauthorized User Access");
					}
					assetecmdata = _assetECMInfoRepository.GetAssetECMInfo(assetUID);
				}
				catch (JsonException ex)
				{
					_logger.LogError("Unauthorized User Access", ex);
					return BadRequest("Unauthorized User Access");
				}
				catch (Exception ex)
				{
					_logger.LogError("Get AssetECMInfo threw an exception", ex);
					return StatusCode((int)HttpStatusCode.InternalServerError, ex);
				}
				assetecm.AssetUID = assetUID;
				assetecm.AssetECMInfo = new List<AssetECMInfo>();
				foreach (var ecm in assetecmdata)
				{
					assetecm.AssetECMInfo.Add
					  (new AssetECMInfo
					  {
						  ECMSerialNumber = ecm.SerialNumber,
						  FirmwarePartNumber = ecm.PartNumber,
						  ECMDescription = String.IsNullOrWhiteSpace(ecm.Description) ? "N/A" : ecm.Description,
						  SyncClockEnabled = ecm.SyncClockEnabled == true ? "Yes" : "No",
						  SyncClockLevel = ecm.SyncClockEnabled == true ? ecm.SyncClockLevel == true ? "Master" : "Slave" : "N/A"
					  });
				}
				return StatusCode((int)HttpStatusCode.OK, assetecm);
			}
			catch (Exception ex)
			{
				_logger.LogError("GetAssetEcmByAssetUID encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
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
