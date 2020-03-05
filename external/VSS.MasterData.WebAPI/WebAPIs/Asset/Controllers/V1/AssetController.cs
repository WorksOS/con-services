using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.Asset.Filters;
using VSS.MasterData.WebAPI.Asset.Helpers;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.VisionLink.SearchAndFilter.Client.v1_6.Interfaces;
using VSS.VisionLink.SearchAndFilter.Interfaces.v1_6.DataContracts;
using DataModels = VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Asset.Controllers.V1
{
	[Route("v1/Asset")]
	public class AssetV1Controller : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly IControllerUtilities _controllerUtilities;
		private readonly IAssetServices _assetService;
		private readonly ISearchAndFilter _searchAndFilterClient;
		private Guid kiewitAMPCustomerUID;
	
		public AssetV1Controller(IAssetServices assetServices, IControllerUtilities controllerUtilities, IConfiguration configuration, ILogger logger, ISearchAndFilter searchAndFilterClient)
		{
			_controllerUtilities = controllerUtilities;
			_assetService = assetServices;
			_searchAndFilterClient = searchAndFilterClient;
			_configuration = configuration;
			_logger = logger;
			kiewitAMPCustomerUID = Guid.TryParse(_configuration["KiewitAMPCustomerUID"], out var guid) ? guid : Guid.Empty;
		}

		#region Public Methods

		#region Fetch

		[Produces("application/xml")]
		private ActionResult GetXMLResponce<T>(T obj)
		{
			if (Request.Headers.ContainsKey("Accept"))
			{
				Request.Headers["Accept"] = "application/xml";

			}
			else
			{
				Request.Headers.Add("Accept", "application/xml");
			}
			return StatusCode((int)HttpStatusCode.OK, obj);
		}
		//Fetch the details of an asset by passing either the AssetGuid or Legacy AssetID

		[Route("AssetList")]
		[HttpGet]
		[ParseUserGuidFromHeaderAttributeForVLConnect("userGuid")]
		public ActionResult GetAssetByLegacyAssetID(
			string assetIDorAssetUID = null,
			Guid? userGuid = null,
			bool bgetVLIasAssetUID = false,
			bool isMultiple = false,
			string assetBaseURI = "",
			long? pageNumber = 1,
			string makeCode = null,
			string serialNumber = null,
			bool isGetAssetDevice = false)
		{
			try
			{
				if (!userGuid.HasValue)
					return BadRequest("UserUID has not been provided");
				Guid? assetUID = null;

				Guid assetGuid = new Guid();
				long assetID = 0;

				if (!string.IsNullOrEmpty(assetIDorAssetUID))
				{
					if (!Guid.TryParse(assetIDorAssetUID, out assetGuid))
					{
						if (!long.TryParse(assetIDorAssetUID, out assetID))
							return BadRequest("Invalid assetIDorAssetUID");
					}
					else
						assetUID = (Guid?)assetGuid;
				}
				int? pageSize = 100;
				var assetdata = _assetService.GetAssetByAssetLegacyID((Guid)userGuid, assetID, assetUID, pageNumber, pageSize, makeCode, serialNumber);

				string requestURI = string.Empty;
				if (Request.Headers.TryGetValue("X-VisionLink-RequestUri", out StringValues headerValues1))
				{
					if (!string.IsNullOrEmpty(headerValues1.FirstOrDefault()))
					{
						requestURI = headerValues1.FirstOrDefault();
					}
				}

				string requestType = string.Empty;
				if (Request.Headers.TryGetValue("Accept", out StringValues headerValues2))
				{
					if (!string.IsNullOrEmpty(headerValues2.FirstOrDefault()))
					{
						requestType = headerValues2.FirstOrDefault();
					}
				}

				IEnumerable<string> headerValues = new List<string>();
				if (isMultiple)
				{
					if (!isGetAssetDevice)
					{
						if (assetdata != null && assetdata.Count > 0)
						{
							assetBaseURI = !string.IsNullOrEmpty(assetBaseURI) ? assetBaseURI : requestURI == null ? _configuration["LegacyAssetBaseURI"] : requestURI.ToString();

							var assets = new MultipleResponse.Assets
							{
								IsLastPage = assetdata.Count <= pageSize,
								AssetRecords = assetdata.Select(x => new
									MultipleResponse.Asset
								{
									Url = string.Concat(assetBaseURI, assetUID.HasValue || bgetVLIasAssetUID ? Guid.Parse(x.AssetUID).ToString() : x.LegacyAssetID > 0 ? x.LegacyAssetID.ToString() : new Guid(x.AssetUID).ToString()),
									VisionLinkIdentifier = assetUID.HasValue || bgetVLIasAssetUID ? Guid.Parse(x.AssetUID).ToString() : x.LegacyAssetID.ToString(),
									MakeCode = x.MakeCode ?? string.Empty,
									MakeName = x.MakeName ?? string.Empty,
									SerialNumber = x.SerialNumber ?? string.Empty,
									AssetID = x.AssetName ?? string.Empty,
									EquipmentVIN = x.EquipmentVIN ?? string.Empty,
									Model = x.Model ?? string.Empty,
									ProductFamily = x.ProductFamily ?? string.Empty,
									ManufactureYear = x.ModelYear ?? string.Empty
								}).Take((int)pageSize).ToList()

							};
							if (requestType != null)
							{
								if (requestType == "application/json")
								{
									return StatusCode((int)HttpStatusCode.OK, assets);
								}
								else
									return GetXMLResponce(assets);
							}
							else
								return GetXMLResponce(assets);
						}
						else
						{
							return StatusCode((int)HttpStatusCode.OK);
						}
					}
					else
					{
						MultiADResponse.Assets assets = new MultiADResponse.Assets
						{
							IsLastPage = true
						};
						if (assetdata != null)
						{
							string reqUri = requestURI.ToString();

							reqUri = !string.IsNullOrEmpty(reqUri) ? reqUri.Contains("/page") ? reqUri.Substring(0, reqUri.IndexOf("/page")) : reqUri : _configuration["LegacyAssetADBaseURI"];

							assetBaseURI = !string.IsNullOrEmpty(assetBaseURI) ? assetBaseURI : !string.IsNullOrEmpty(reqUri) ? reqUri : _configuration["LegacyAssetADBaseURI"];

							assets = new MultiADResponse.Assets
							{
								NavigationLinks = new List<MultiADResponse.Link>
									{
										new MultiADResponse.Link
										{
										  Href =  string.Concat(reqUri,"page/",pageNumber),
										  Relation = "self"
										},
										new MultiADResponse.Link
										{
										  Href =
											pageNumber == 1
											  ? string.Empty
											  : string.Concat(reqUri,"page/",pageNumber - 1),
										  Relation = "prev"
										},
										new MultiADResponse.Link
										{
										  Href =
											pageSize >= assetdata.Count
											  ? string.Empty
											  : string.Concat(reqUri,"page/",pageNumber + 1),
										  Relation = "next"
										}
									},
								IsLastPage = assetdata.Count <= pageSize,
								Asset = assetdata.Select(x => new
									 MultiADResponse.Asset
								{
									Url = string.Concat(assetBaseURI, assetUID != null && assetUID.HasValue || bgetVLIasAssetUID ? Guid.Parse(x.AssetUID).ToString() : x.LegacyAssetID > 0 ? x.LegacyAssetID.ToString() : new Guid(x.AssetUID).ToString()),
									VisionLinkIdentifier = assetUID != null && assetUID.HasValue || bgetVLIasAssetUID ? Guid.Parse(x.AssetUID).ToString() : x.LegacyAssetID.ToString(),
									MakeCode = x.MakeCode ?? string.Empty,
									MakeName = x.MakeName ?? string.Empty,
									SerialNumber = x.SerialNumber ?? string.Empty,
									AssetID = x.AssetName ?? string.Empty,
									Model = x.Model ?? string.Empty,
									ProductFamily = x.ProductFamily ?? string.Empty,
									ManufactureYear = x.ModelYear ?? string.Empty,
									DeviceType = x.DeviceType ?? string.Empty,
									DeviceSerialNumber = x.DeviceSerialNumber ?? string.Empty
								}).Take((int)pageSize).ToList()

							};
							var PagingLinks = new List<MultiADResponse.Link>();
							foreach (var ap in assets.NavigationLinks)
							{
								if (!string.IsNullOrEmpty(ap.Href))
								{
									PagingLinks.Add(
									new MultiADResponse.Link
									{
										Href = ap.Href,
										Relation = ap.Relation
									});
								}
							}
							assets.NavigationLinks = PagingLinks;

							if (requestType != null)
							{
								if (requestType == "application/json")
								{
									return StatusCode((int)HttpStatusCode.OK, assets);
								}
								else
								{
									return GetXMLResponce(assets);
								}
							}
							else
								return GetXMLResponce(assets);

						}
						else
						{
							return BadRequest("Asset not found or you do not have access to this Asset.");
						}
					}
				}
				else
				{
					SingleResponse.Asset asset = new SingleResponse.Asset();
					if (assetdata != null && assetdata.Count > 0)
					{

						var _assetRecords = assetdata.Select(x => new SingleResponse.Asset
						{
							VisionLinkIdentifier = assetUID != null && assetUID.HasValue || bgetVLIasAssetUID ? Guid.Parse(x.AssetUID).ToString() : x.LegacyAssetID.ToString(),
							MakeCode = x.MakeCode ?? string.Empty,
							MakeName = x.MakeName ?? string.Empty,
							SerialNumber = x.SerialNumber ?? string.Empty,
							AssetID = x.AssetName ?? string.Empty,
							Model = x.Model ?? string.Empty,
							ProductFamily = x.ProductFamily ?? string.Empty,
							ManufactureYear = x.ModelYear ?? string.Empty,
							DeviceType = x.DeviceType ?? string.Empty,
							DeviceSerialNumber = x.DeviceSerialNumber ?? string.Empty
						});

						asset = _assetRecords != null && _assetRecords.ToList().Count > 0 ? _assetRecords.First() : null;

						if (requestType != null)
						{
							if (requestType == "application/json")
							{
								return StatusCode((int)HttpStatusCode.OK, asset);
							}
							else
							{
								return GetXMLResponce(asset);
							}
								
						}
						else
						{
							return GetXMLResponce(asset);
						}
					}
					else
					{
						string errMsg = "No Asset exists with " + (assetID > 0 ? $"ID: {assetID}" : $"UID: {assetUID}");
						return StatusCode((int)HttpStatusCode.NotFound, errMsg);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("GetAssetByLegacyAssetID encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUIDs"></param>
		/// <param name="customerUid"></param>
		/// <param name="assetType"></param>
		/// <param name="status"></param>
		/// <param name="manufacturer"></param>
		/// <param name="model"></param>
		/// <param name="snContains"></param>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <param name="userGuid"></param>
		/// <param name="accountSelectionGuid"></param>
		/// <returns></returns>
		[Route("List")]
		[HttpGet]
		[ParseAssetGuidsFromHeader("assetUIDs")]
		[ParseCustomerGuidFromHeader("accountSelectionGuid")]
		public ActionResult GetAssets(
				[FromQuery] string[] assetUIDs = null, [FromQuery] string customerUid = null,
				[FromQuery] string[] assetType = null, [FromQuery] string[] status = null,
				[FromQuery] string[] manufacturer = null, [FromQuery] string[] model = null,
				[FromQuery] string snContains = null,
				[FromQuery] string pageSize = "10", [FromQuery] string pageNumber = "1",
				Guid? userGuid = null, Guid? accountSelectionGuid = null)
		{
			bool isIntegrator = false;
			try
			{
				int pageSizeInt;
				int pageNumberInt;
				Guid? customerGuid;
				List<Guid> customerGuids = new List<Guid>() { };

				_logger.LogDebug("Before JWT Call.Requestheaders:" + String.Join(",", Request.Headers.Select(x => x.Key + ":" + x.Value)));

				if (!Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					return BadRequest("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
				}

				TPaaSJWT jwt;
				try
				{
					jwt = new TPaaSJWT(headerValues);
				}
				catch 
				{
					jwt = null;
				}
					

				if (jwt != null)
				{
					if (userGuid == null || !userGuid.HasValue)
						userGuid = jwt.UserUid;

					_logger.LogDebug(string.Format("ApplicationName:{0},uuid:{1},encodedjwt:{2}", jwt.ApplicationName, jwt.UserUid, jwt.EncodedJWT));

					if (!string.IsNullOrEmpty(jwt.ApplicationName))
					{
						_logger.LogInformation("JWT has an appname " + jwt.ApplicationName);
						var customers = _assetService.GetCustomersForApplication(jwt.ApplicationName); // new List<Guid>() { new Guid("07D4A55244C5E311AA7700505688274D") };//

						if (customers!=null && customers.Count > 0)
						{
							_logger.LogInformation("CustomerCount>0" + new Guid(customers[0].ToString()));
							customerGuid = !string.IsNullOrEmpty(customerUid) ? new Guid(customerUid) : (Guid?)null;
							if (customerGuid.HasValue)
							{
								if (customers.Contains((Guid)customerGuid))
								{
									customerGuids.Add((Guid)customerGuid);
									isIntegrator = true;
								}
								else
									return BadRequest("Application does not have this customer mapped. Please contact your API administrator.");
							}
							else
							{
								customerGuids = customers;
								isIntegrator = true;
							}
						}
						if (isIntegrator)
						{
							try
							{
								_controllerUtilities.ValidatePageParameters(pageSize, pageNumber, out pageSizeInt, out pageNumberInt);
							}
							catch (Exception ex)
							{
								_logger.LogError("Get Assets parameter validation threw an exception", ex);
								return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);//BadRequest("Assets parameter validation failed.");
							}
							if (customerGuids != null && customerGuids.Any())
							{
								CustomerAssetsListData assets = _assetService.GetAssetsForCustomer(customerGuids, pageNumberInt, pageSizeInt);
								return Ok(assets);
							}
						}
						else
						{
							if (!userGuid.HasValue)
								return BadRequest("UserUID has not been provided");

							Guid[] assetGuids;
							WildcardMatches wildcardMatches;
							try
							{
								assetGuids = _controllerUtilities.ValidateAssetUIDParameters(assetUIDs);
								wildcardMatches = ValidateWildcardMatches(null, snContains);
								_controllerUtilities.ValidatePageParameters(pageSize, pageNumber, out pageSizeInt, out pageNumberInt);
								customerGuid = !string.IsNullOrEmpty(customerUid) ? new Guid(customerUid) : (Guid?)null;
							}
							catch (Exception ex)
							{
								_logger.LogError("Get Assets parameter validation threw an exception", ex);
								return BadRequest(" Assets parameter validation " + ex.Message); //StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);//
							}

							if (!assetGuids.Any())
								assetGuids = GetAssetUIDsFromFilters(
												userGuid.Value, accountSelectionGuid, customerGuid, assetType, status, manufacturer, model,
												wildcardMatches, pageSizeInt, pageNumberInt);

							if (assetGuids != null && assetGuids.Any())
							{
								var assets = GetAssetsFromAssetGuids(assetGuids, userGuid.Value);
								return Ok(assets);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Get AssetLists encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
			return Ok(isIntegrator ? (object)new List<DataModels.CustomerAsset>() : (object)new List<DataModels.Asset>());
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		[Route("List/MixedFleetAssets")]
		[HttpGet]
		[UserUIDParser("userGuid")]
		public ActionResult Get3PDataAssets(Guid? userGuid = null)
		{
			try
			{
				if (!userGuid.HasValue)
					return BadRequest("UserUID has not been provided");
				var assets = _assetService.GetHarvesterAssets();
				return Ok(assets);
			}
			catch (Exception ex)
			{
				_logger.LogError("Get3PDataAssets encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetParam"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		[Route("List")]
		[HttpPost]
		[UserUIDParser("userGuid")]
		[ChunkedEncodingFilter(typeof(AssetListParam), "assetParam")]
		public ActionResult GetAssets([FromBody] AssetListParam assetParam, Guid? userGuid = null)
		{
			try
			{
				if (!userGuid.HasValue)
					return BadRequest("UserUID has not been provided");
				Guid[] assetGuids;
				try
				{
					assetGuids = _controllerUtilities.ValidateAssetUIDParameters(assetParam.AssetUIDs.ToArray());
				}
				catch (Exception ex)
				{
					_logger.LogError("Get Assets parameter validation threw an exception", ex);
					return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
				}

				if (assetGuids != null && assetGuids.Any())
				{
					var assets = GetAssetsFromAssetGuids(assetGuids, userGuid.Value);
					return Ok(assets);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Get AssetList encountered an error", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
			return Ok(new List<DataModels.Asset>());
		}

		#endregion

		#region Save

		// POST: api/Asset
		/// <summary>
		/// Create asset
		/// </summary>
		/// <param name="asset">Asset model</param>
		/// <remarks>Create new asset</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPost]
		[ChunkedEncodingFilter(typeof(CreateAssetEvent), "asset")]
		public ActionResult CreateAsset([FromBody] CreateAssetEvent asset)
		{
			try
			{

				//var jwt = GetSampleJwt(); //use this for testing locally
				if (!Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					return BadRequest("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
				}

				TPaaSJWT jwt;
				try
				{ 
					jwt = new TPaaSJWT(headerValues); 
				}
				catch(Exception ex)
				{
					jwt = null;
				}
				

				if (jwt != null)
				{
					_logger.LogInformation("Creating asset for user: {0}", jwt.UserUid);
				}
				else
					return BadRequest("no jwt token");

				asset.ReceivedUTC = DateTime.UtcNow;

				_logger.LogDebug($"CreateAsset - Calling Application Name: {jwt.ApplicationName}, UserType: {jwt.UserType}");

				bool isCGIntegrator = jwt.UserType == "APPLICATION" && jwt.ApplicationName == _configuration["CGIntegratorAppName"];

				if (isCGIntegrator)
				{
					if (asset.AssetUID == null || asset.AssetUID.Equals(Guid.Empty) || !Guid.TryParse(asset.AssetUID.ToString(), out Guid g))
						return BadRequest("AssetUID must be given for VisionLink integrator");

					if (asset.OwningCustomerUID == null)
					{
						asset.OwningCustomerUID = new Guid();
					}
				}
				else if (!string.IsNullOrEmpty(jwt.ApplicationName))
				{
					asset.AssetUID = Guid.NewGuid();

					var customers = _assetService.GetCustomersForApplication(jwt.ApplicationName);
					if (customers?.Count > 0)
					{
						if (asset.OwningCustomerUID == null)
						{
							asset.OwningCustomerUID = customers.First();
						}
					}
					else
					{
						return BadRequest("Application does not have any customers mapped. Please contact your API administrator.");
					}

					Guid? existingAssetGuid = _assetService.GetAssetUid(asset.AssetUID.Value, asset.MakeCode, asset.SerialNumber);
					if (existingAssetGuid.HasValue)
						return StatusCode((int)HttpStatusCode.Conflict, (existingAssetGuid.Value.ToString()));
				}
				else return BadRequest("jwt application name is empty");


				Guid? existingAsset = _assetService.GetAssetUid(asset.AssetUID.Value, asset.MakeCode, asset.SerialNumber);
				if (existingAsset.HasValue)
				{
					if (isCGIntegrator)
					{
						var updatePayload = new UpdateAssetEvent
						{
							AssetUID = existingAsset.Value,
							OwningCustomerUID = asset.OwningCustomerUID,
							LegacyAssetID = asset.LegacyAssetID,
							AssetName = asset.AssetName,
							Model = asset.Model,
							AssetType = asset.AssetType,
							IconKey = asset.IconKey,
							EquipmentVIN = asset.EquipmentVIN,
							ModelYear = asset.ModelYear,
							ActionUTC = DateTime.UtcNow,
							ObjectType = asset.ObjectType,
							Category = asset.Category,
							ProjectStatus = asset.ProjectStatus,
							SortField = asset.SortField,
							Source = asset.Source,
							UserEnteredRuntimeHours = asset.UserEnteredRuntimeHours,
							Classification = asset.Classification,
							PlanningGroup = asset.PlanningGroup
						};

						var updateResult = UpdateAssetInfo(updatePayload);

						return updateResult.GetType().Name == "OkObjectResult"
								? StatusCode((int)HttpStatusCode.Conflict, (existingAsset.Value.ToString()))
								: updateResult;
					}
					return Conflict(new { message = $"Asset already exists" });
				}

				if (!_assetService.IsValidMakeCode(asset.MakeCode.ToUpper()))
				{
					return BadRequest($"Asset make code '{asset.MakeCode.ToUpper()}' is not valid.");

				}
				if (_assetService.CreateAsset(asset))
				{
					return Ok(asset.AssetUID.ToString());
				}

				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				_logger.LogError("Create Asset Exception: " + ex.ToString());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}


		// PUT: api/Asset
		/// <summary>
		/// Update asset
		/// </summary>
		/// <param name="assetEvent">UpdateAsset model</param>
		/// <remarks>Updates existing asset using AssetUID</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPut]
		[ChunkedEncodingFilter(typeof(UpdateAssetEvent), "asset")]
		public ActionResult UpdateAssetInfo([FromBody] UpdateAssetEvent assetEvent)
		{
			try
			{

				if (HasNoDataToUpdate(assetEvent))
				{
					return BadRequest("Update Request should have atleast one data to update");
				}

				if (!Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					return BadRequest("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
				}

				TPaaSJWT jwt;
				try 
				{
					jwt = new TPaaSJWT(headerValues);
				}
				catch
				{
					jwt = null;
				}
					

				if (jwt != null)
					_logger.LogInformation("Creating asset for user: {0}", jwt.UserUid);
				else
					return BadRequest("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");

				_logger.LogDebug($"UpdateAsset - Calling Application Name: {jwt.ApplicationName}, UserType: {jwt.UserType}");

				ClientModel.Asset assetDetails = _assetService.GetAsset(assetEvent.AssetUID.Value);

				if (assetDetails == null || assetDetails.StatusInd == 0)
					return BadRequest("Asset does not exist");

				bool isCGIntegrator = jwt.UserType == "APPLICATION" && jwt.ApplicationName == _configuration["CGIntegratorAppName"];

				if (isCGIntegrator)
				{
					if (assetEvent.OwningCustomerUID == null || assetEvent.OwningCustomerUID == Guid.Empty)
					{
						if (!string.IsNullOrEmpty(assetDetails.OwningCustomerUID) && Guid.Parse(assetDetails.OwningCustomerUID) == kiewitAMPCustomerUID)
						{
							// get current AssetId from DB and update
							assetEvent.AssetName = assetDetails.AssetName;
						}
					}
					else if (assetEvent.OwningCustomerUID == kiewitAMPCustomerUID)
					{
						// get current AssetId from DB and update
						assetEvent.AssetName = assetDetails.AssetName;
					}
				}

				if (assetEvent.LegacyAssetID == 0) // retain it
				{
					assetEvent.LegacyAssetID = assetDetails.LegacyAssetID;
				}

				assetEvent.ReceivedUTC = DateTime.UtcNow;

				if (_assetService.UpdateAsset(assetEvent))
				{
					return Ok(assetEvent.AssetUID.ToString());
				}

				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}

			catch (Exception ex)
			{
				_logger.LogError("Update Asset Exception: " + ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		/// DELETE: api/Asset/
		/// <summary>
		/// Delete asset
		/// </summary>
		/// <param name="AssetUID"></param>
		/// <param name="ActionUTC"></param>
		/// <returns></returns>
		[Route("")]
		[HttpDelete]
		public ActionResult DeleteAsset(Guid? AssetUID, DateTime? ActionUTC)
		{
			if (AssetUID == Guid.Empty || AssetUID == null || !Guid.TryParse(AssetUID.ToString(), out Guid g))
				return BadRequest("AssetUID field values must be valid");
			try
			{
				var assetDetails = _assetService.GetAsset(AssetUID.Value);

				if (assetDetails == null || assetDetails.StatusInd == 0)
				{
					return BadRequest("Asset does not exist");
				}

				var asset = new DeleteAssetPayload
				{
					AssetUID = AssetUID.Value,
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				};

				if (_assetService.DeleteAsset(asset))
				{
					return Ok(asset.AssetUID.ToString());
				}
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}

			catch (Exception ex)
			{
				_logger.LogError(" Delete Asset Exception: " + ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		#endregion Save

		#endregion Public Methods

		#region Private Methods

		[ExcludeFromCodeCoverage]
		private Guid[] GetAssetUIDsFromFilters(Guid userUid, Guid? accountSelectionUid, Guid? customerGuid,
			string[] assetType, string[] assetStatus, string[] manufacturer, string[] model,
			WildcardMatches wildcardMatches,
			int pageSize, int pageNumber)
		{
			try
			{
				var query = new AssetFleetQueryParameters
				{
					UserUID = userUid,
					AssetFamilyType = assetType,
					AccountSelectionUID = accountSelectionUid,
					CustomerUID = customerGuid.HasValue ? new Guid[] { customerGuid.Value } : new Guid[] { },
					Status = assetStatus,
					Manufacturer = manufacturer,
					Model = model,
					Wildcards = wildcardMatches,
					PageNo = pageNumber,
					PageSize = pageSize
				};

				var response = _searchAndFilterClient.QueryFleet(query);
				if (!response.AssetUIDs.Any())
				{
					return null;
				}

				var assetUIDs = response.AssetUIDs.Distinct();
				return assetUIDs.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError($"{ex.Message}");
				throw;
			}
		}

		/// <summary>
		/// helper method that reads assets from DB's for passed in asset Guids. Makes dummy records with just assetUID when DB does not contain asset.
		/// </summary>
		/// <returns>same number of assets as assetGuids whether asset exists or not</returns>
		private List<DataModels.Asset> GetAssetsFromAssetGuids(Guid[] assetGuids, Guid userGuid)
		{
			var assets = _assetService.GetAssets(assetGuids, userGuid)
				.GroupBy(asset => new Guid(asset.AssetUID)).Select(group => group.First())//basically a distinctBy
				.ToDictionary(x => new Guid(x.AssetUID), x => { x.AssetUID = new Guid(x.AssetUID).ToString(); return x; });

			int assetGuidCount = assetGuids.Count();
			var assetArray = new DataModels.Asset[assetGuidCount];

			for (int i = 0; i < assetGuidCount; ++i)
			{
				if (assets.ContainsKey(assetGuids[i]))
					assetArray[i] = assets[assetGuids[i]];
				else
					assetArray[i] = new DataModels.Asset { AssetUID = assetGuids[i].ToString() };
			}

			return assetArray.ToList();
		}

		private WildcardMatches ValidateWildcardMatches(string assetIDContains, string snContains)
		{
			WildcardMatches wilcardMatches = null;
			if (assetIDContains != null || snContains != null)
				wilcardMatches = new WildcardMatches
				{
					AssetIDContains = assetIDContains,
					SNContains = snContains
				};
			return wilcardMatches;
		}

		private bool HasNoDataToUpdate(UpdateAssetEvent asset)
		{
			return asset.AssetName ==	MasterDataConstants.InvalidStringValue
					&& asset.AssetType == MasterDataConstants.InvalidStringValue
					&& asset.EquipmentVIN == MasterDataConstants.InvalidStringValue
					&& asset.IconKey == MasterDataConstants.InvalidIntValue
					&& asset.LegacyAssetID == MasterDataConstants.InvalidIntValue
					&& asset.Model == MasterDataConstants.InvalidStringValue
					&& asset.ModelYear == MasterDataConstants.InvalidIntValue
					&& asset.OwningCustomerUID == Guid.Empty
					&& asset.ObjectType == MasterDataConstants.InvalidStringValue
					&& asset.Category == MasterDataConstants.InvalidStringValue
					&& asset.ProjectStatus == MasterDataConstants.InvalidStringValue
					&& asset.Source == MasterDataConstants.InvalidStringValue
					&& asset.SortField == MasterDataConstants.InvalidStringValue
					&& asset.UserEnteredRuntimeHours == MasterDataConstants.InvalidStringValue
					&& asset.Classification == MasterDataConstants.InvalidStringValue
					&& asset.PlanningGroup == MasterDataConstants.InvalidStringValue;
		}
		#endregion Private Methods
	}
}
