using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonApiLibrary;
using CommonApiLibrary.Filters;
using CommonModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace AssetSettings.Controller
{
	/// <summary>
	/// 
	/// </summary>
	[Route("v1/assetsettings")]
	public class AssetSettingsListController : ApiControllerBase
	{
		private readonly IAssetSettingsListService _assetSettingsListService;
		private readonly ILoggingService _loggingService;
		private readonly Configurations _configurations;

		/// <summary>
		/// Initialize the AssetSettings Controller
		/// </summary>
		/// <param name="assetSettingsListService"></param>
		/// <param name="loggingService"></param>
		public AssetSettingsListController(IOptions<Configurations> configurations, IAssetSettingsListService assetSettingsListService, ILoggingService loggingService)
		{
			this._configurations = configurations.Value;
			this._assetSettingsListService = assetSettingsListService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}

		/// <summary>
		/// Fetches the list of the essential assets
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetSettingsListResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetSettingsListResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetSettingsListResponse), (int)HttpStatusCode.InternalServerError)]
		// GET: api/AssetWorkDefinition
		public async Task<ActionResult<AssetSettingsListResponse>> Get([FromQuery] AssetSettingsListRequest request)
		{
			if (request == null)
			{
				request = new AssetSettingsListRequest();
			}
			if (request.PageNumber <= 0)
			{
				if (_configurations.ApplicationSettings.DefaultPageNumber.HasValue)
				{
					request.PageNumber = _configurations.ApplicationSettings.DefaultPageNumber.Value;
				}
			}
			if (request.PageSize <= 0)
			{
				if (_configurations.ApplicationSettings.DefaultPageSize.HasValue)
				{
					request.PageSize = _configurations.ApplicationSettings.DefaultPageSize.Value;
				}
			}

			request.CustomerUid = base.GetCustomerContext(Request);

			request.UserUid = base.GetUserContext(Request);

			this._loggingService.Debug("Started fetching essential assets for request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);
			var result = await this._assetSettingsListService.FetchEssentialAssets(request);
			this._loggingService.Debug("End fetching essential assets", MethodInfo.GetCurrentMethod().Name);
			return base.SendResponse(HttpStatusCode.OK, result);
		}
	}
}
