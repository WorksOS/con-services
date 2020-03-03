using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.Asset.Controllers.V1
{
	[Route("v1")]
	public class SupportAssetV1Controller : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IControllerUtilities _controllerUtilities;
		private readonly IAssetServices _assetRepository;
		public readonly ISupportAssetServices _supportAssetServices;
		
		public SupportAssetV1Controller(IAssetServices assetRepository, IControllerUtilities controllerUtilities, ILogger logger,ISupportAssetServices supportAssetServices)
		{
			_controllerUtilities = controllerUtilities;
			_assetRepository = assetRepository;
			_logger = logger;
			_supportAssetServices = supportAssetServices;
		}
		#region Public Methods
		[Route("AssetDevice/list")]
		[HttpGet]
		public ActionResult GetAssetsForSupportUser(
					[FromQuery] string searchString = null,
					[FromQuery] string pageSize = "10",
					[FromQuery] string pageNumber = "1")
		{
			if (!String.IsNullOrEmpty(searchString))
			{
				try
				{
					int pageSizeInt;
					int pageNumberInt;

					try
					{
						_controllerUtilities.ValidatePageParameters(pageSize, pageNumber, out pageSizeInt, out pageNumberInt);
					}
					catch (Exception ex)
					{
						_logger.LogError("Get Assets for support user parameter validation threw an exception", ex);
						return BadRequest(ex.Message);
					}
					var assets = _assetRepository.GetAssetsforSupportUser(searchString, pageNumberInt, pageSizeInt);
					return Ok(assets);
				}
				catch (Exception ex)
				{
					_logger.LogError("Get Assets for Support User threw an exception", ex);
					return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
				}
			}
		
			else
			{
				return Ok(new AssetDeviceForSupportUserListD { TotalNumberOfPages = 0, PageNumber = 0, AssetDevices = null });
			}
		}

		[Route("AssetDetails")]
		[HttpGet]
		public ActionResult GetAssetDetail([FromQuery] Guid? assetUID = null, [FromQuery] Guid? deviceUID = null)
		{
			try
			{
				if (assetUID == null && deviceUID == null)
					return BadRequest("AssetUID/DeviceUID has not been provided");
				try
				{
					if (assetUID != null && deviceUID != null)
					{
						_controllerUtilities.ValidateAssetUIDParameters(new[] { assetUID.ToString(), deviceUID.ToString() });
					}
					else if (assetUID != null)
					{
						_controllerUtilities.ValidateAssetUIDParameters(new[] { assetUID.ToString() });
					}
					else if (deviceUID != null)
					{
						_controllerUtilities.ValidateAssetUIDParameters(new[] { deviceUID.ToString() });
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("Get Assets parameter validation threw an exception", ex);
					return BadRequest(ex.Message);
				}

				if (assetUID != null || deviceUID != null)
				{
					AssetSubscriptionModel subscription = null;
					List<ClientModel.AssetCustomer> lstAssetCustomers = null;
					List<AssetDetail> assetDetails = (List<AssetDetail>)_assetRepository.GetAssetDetail(assetUID, deviceUID);
					if (assetDetails == null)
					{
						_logger.LogInformation($"No asset with UID {assetUID} exists");
						return StatusCode((int)HttpStatusCode.NoContent, assetUID);
					}
					List<AssetDeviceDetail> lstAssetDetails = new List<AssetDeviceDetail>();
					foreach (AssetDetail assetDetail in assetDetails)
					{
						if (assetDetail.AssetUID != null)
						{
							subscription = new AssetSubscriptionModel();
							subscription = GetSubscriptionfromAPI(new Guid(assetDetail.AssetUID));

							lstAssetCustomers = new List<ClientModel.AssetCustomer>();
							lstAssetCustomers = GetAssetCustomersfromAPI(new Guid(assetDetail.AssetUID));
						}

						AssetInfo assetInfo = null;
						ClientModel.DeviceModel deviceInfo = null;

						if (assetDetail != null)
						{
							if (assetDetail.AssetUID != null)
							{
								assetInfo = new AssetInfo
								{
									AssetName = assetDetail.AssetName,
									AssetType = assetDetail.AssetTypeName,
									AssetUID = assetDetail.AssetUID != null ? (Guid?)new Guid(assetDetail.AssetUID) : null,
									MakeCode = assetDetail.MakeCode,
									Model = assetDetail.Model,
									ModelYear = assetDetail.ModelYear,
									SerialNumber = assetDetail.SerialNumber
								};
							}

							deviceInfo = new ClientModel.DeviceModel
							{
								DeviceSerialNumber = assetDetail.DeviceSerialNumber,
								DeviceState = ((DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), assetDetail.DeviceState)).ToString(), //assetDetail.DeviceState,
								DeviceType = assetDetail.DeviceType, 
								DeviceUID = assetDetail.DeviceUID != null ? (Guid?)new Guid(assetDetail.DeviceUID) : null
							};
						}
						AssetDeviceDetail assetDet = new AssetDeviceDetail
						{
							AssetInfo = assetInfo,
							DeviceInfo = deviceInfo,
							Subscription = subscription,
							AccountInfo = lstAssetCustomers
						};
						lstAssetDetails.Add(assetDet);
					}
					return Ok(lstAssetDetails);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("GetAssetDetail encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
			return Ok(new List<VSS.MasterData.WebAPI.DbModel.Asset>());
		}
		
		[Route("AssetDeviceDetails")]
		[HttpPost]
		public ActionResult GetAssetDeviceDetails([FromBody]AssetDeviceRequest assetUIDs)
		{
			List<Guid> assetGuids = new List<Guid>();
			List<AssetDetail> assetDetails = new List<AssetDetail>();
			try
			{

				if (assetUIDs == null || assetUIDs.AssetUIDs == null || assetUIDs.AssetUIDs.Count() == 0)
					return BadRequest("AssetUID has not been provided");
				try
				{
					if (assetUIDs != null && assetUIDs.AssetUIDs != null && assetUIDs.AssetUIDs.Count() > 0)
					{
						assetGuids = _controllerUtilities.ValidateAssetUIDParameters(assetUIDs.AssetUIDs.ToArray()).ToList();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("Get Assets parameter validation threw an exception", ex);
					return BadRequest("Invalid Input");
				}
				if (assetGuids != null && assetGuids.Count() > 0)
				{
					assetDetails = _supportAssetServices. GetAssetDetailFromAssetGuids(assetGuids);
					assetDetails.ForEach(assetDetail =>
					{
						assetDetail.AssetUID = string.IsNullOrWhiteSpace(assetDetail.AssetUID) ? null : Guid.Parse(assetDetail.AssetUID).ToString();
						assetDetail.DeviceUID = string.IsNullOrWhiteSpace(assetDetail.DeviceUID) ? null : Guid.Parse(assetDetail.DeviceUID).ToString();
						assetDetail.DeviceState = ((DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), assetDetail.DeviceState)).ToString(); //assetDetail.DeviceState,
					});
				}
				else
					return BadRequest("Valid assetUID has not been provided");
			}
			catch (Exception ex)
			{
				_logger.LogError("GetAssetDetailsFromAssetGuids encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
			}
			return Ok(assetDetails);
		}

		#endregion

		#region Private Methods

		private List<ClientModel.AssetCustomer> GetAssetCustomersfromAPI(Guid assetGUID)
		{
			try
			{
				var data = _supportAssetServices.GetAssetCustomerByAssetGuid(assetGUID);
				
				return data;// Newtonsoft.Json.JsonConvert.DeserializeObject<List<AssetCustomer>>(data.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return null;
			}
		}

		private AssetSubscriptionModel GetSubscriptionfromAPI(Guid assetGUID)
		{
			try
			{
				var data = _supportAssetServices.GetSubscriptionForAsset(assetGUID);
				return data;//Newtonsoft.Json.JsonConvert.DeserializeObject<AssetSubscriptionModel>(data.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return null;
			}
		}

		#endregion Private Methods

	}
}
