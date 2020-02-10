using AssetSettings.Helpers;
using ClientModel.AssetSettings.Request;
using ClientModel.WorkDefinition;
using CommonApiLibrary;
using Infrastructure.Service.AssetSettings.Interfaces;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.Asset.WebAPI.Helpers;

namespace AssetSettings.Controller
{
	[Route("v1/WorkDefinition")]
	public class WorkDefinitionV1Controller : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		//private readonly IConfiguration _configuration;
		//private readonly List<string> assetTopicNames;
		//private readonly IMapper _mapper;
		private readonly IWorkDefinitionServices _wdRepo;
		private readonly IRequestValidator<AssetSettingValidationRequestBase> _requestValidator;

		public WorkDefinitionV1Controller(IWorkDefinitionServices wdRepo, ILoggingService loggingService, IRequestValidator<AssetSettingValidationRequestBase> requestValidator)
		{
			_wdRepo = wdRepo;
			_loggingService = loggingService;
			_requestValidator = requestValidator;
		}

		#region Public Methods

		// POST: api/workdefinition
		/// <summary>
		/// Create Work Definition
		/// </summary>
		/// <param name="workdefinition">CreateWorkDefinitionEvent model</param>
		/// <remarks>Create new WorkDefinition</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPost]
		public async Task<ActionResult> CreateWorkDefinition(WorkDefinitionEvent workdefinition)
		{
			workdefinition = await base.ReadRequestContentAsync(workdefinition);

			workdefinition.ReceivedUTC = DateTime.UtcNow;

			#region Workaround to fix CustomJsonConverter save default value as -999999
			var serializedWorkDefinition = JsonHelper.SerializeObjectToJson(workdefinition, new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
			workdefinition = JsonConvert.DeserializeObject<WorkDefinitionEvent>(serializedWorkDefinition);
			#endregion

			var result = await _requestValidator.Validate(new AssetSettingValidationRequestBase
			{
				AssetUIDs = new List<string> { workdefinition.AssetUID.ToString() },
				DeviceType = workdefinition.DeviceType,
				GroupName = "WorkDefinitions"
			});


			if (result.Any(x => x.IsInvalid))
			{
				_loggingService.Warn(result.First().Message + " for AssetUID : " + workdefinition.AssetUID.ToString(), "WorkDefinitionV1Controller.CreateWorkDefinition");
				return BadRequest(result.First().Message);
			}

			if (_wdRepo.WorkDefinitionExist(workdefinition.AssetUID))
				return BadRequest("Work Definition already exist for given asset.");
			_wdRepo.CreateWorkDefinition(workdefinition);

			return StatusCode((int)HttpStatusCode.OK);
		}

		// PUT: api/workdefinition
		/// <summary>
		/// Update Work Definition
		/// </summary>
		/// <param name="workdefinition">UpdateWorkDefinitionEvent model</param>
		/// <remarks>Updates existing WorkDefinition</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPut]
		public async Task<ActionResult> UpdateWorkDefinition(WorkDefinitionEvent workdefinition)
		{
			try
			{
				workdefinition = await base.ReadRequestContentAsync(workdefinition);

				workdefinition.ReceivedUTC = DateTime.UtcNow;

				#region Workaround to fix CustomJsonConverter save default value as -999999
				var serializedWorkDefinition = JsonHelper.SerializeObjectToJson(workdefinition, new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
				workdefinition = JsonConvert.DeserializeObject<WorkDefinitionEvent>(serializedWorkDefinition);
				#endregion

				var result = await _requestValidator.Validate(new AssetSettingValidationRequestBase
				{
					AssetUIDs = new List<string> { workdefinition.AssetUID.ToString() },
					DeviceType = workdefinition.DeviceType,
					GroupName = "WorkDefinitions"
				});

				if (result.Any(x => x.IsInvalid))
				{
					_loggingService.Warn(result.First().Message + " for AssetUID : " + workdefinition.AssetUID.ToString(), "WorkDefinitionV1Controller.UpdateWorkDefinition");
					return BadRequest(result.First().Message);
				}

				_wdRepo.UpdateWorkDefinition(workdefinition);
				return Ok();
			}
			catch (Exception ex)
			{
				_loggingService.Error("An exception has occurred : ", "WorkDefinitionV1Controller.UpdateWorkDefinition", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Get an asset's WorkDefinition Type
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		[Route("{assetUID}")]
		[HttpGet]
		public async Task<ActionResult> GetWorkDefinition(Guid assetUID, string deviceType = null)
		{
			try
			{
				if (assetUID == Guid.Empty)
				{
					_loggingService.Info($"AssetUID is mandatory AssetUID :{assetUID}", "WorkDefinitionV1Controller.GetWorkDefinition");
					return BadRequest("AssetUID is mandatory");
				}

				var validationResult = await _requestValidator.Validate(new AssetSettingValidationRequestBase
				{
					AssetUIDs = new List<string> { assetUID.ToString() },
					DeviceType = deviceType,
					GroupName = "WorkDefinitions"
				});

				var workDefinitionDto = _wdRepo.GetWorkDefinition(assetUID);
				if (workDefinitionDto != null)
				{
					var workDefinitionEvent = new WorkDefinitionEvent
					{
						WorkDefinitionType = workDefinitionDto.WorkDefinitionType,
						AssetUID = workDefinitionDto.AssetUID,
						SensorNumber = workDefinitionDto.SwitchNumber,
						StartIsOn = workDefinitionDto.SwitchWorkStartState,
						ActionUTC = workDefinitionDto.InsertUTC,
						ReceivedUTC = workDefinitionDto.InsertUTC
					};
					if (validationResult.Any(x => x.IsInvalid))
					{
						_loggingService.Warn(validationResult.First().Message + " for AssetUID : " + assetUID.ToString(), "WorkDefinitionV1Controller.GetWorkDefinition");
						workDefinitionEvent.ErrorMessage = validationResult.First().Message;
					}
					return StatusCode((int)HttpStatusCode.OK, workDefinitionEvent);
				}
				return StatusCode((int)HttpStatusCode.NotFound);
			}
			catch (Exception ex)
			{
				_loggingService.Error("An exception has occurred : ", "WorkDefinitionV1Controller.GetWorkDefinition", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError);
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
		#endregion Public Methods 
	}
}
