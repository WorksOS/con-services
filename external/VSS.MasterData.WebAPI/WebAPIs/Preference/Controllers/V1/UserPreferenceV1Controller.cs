using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.KafkaModel;
using VSS.MasterData.WebAPI.Preference.Filters;

namespace VSS.MasterData.WebAPI.Preference.Controllers.V1
{
	/// <summary>
	/// This Controller handles CRUD methods for PreferenceKey and UserPreference.
	/// </summary>
	[Route("v1")]
	[ApiController]
	public class UserPreferenceV1Controller : ControllerBase
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly IPreferenceService _preferenceService;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="preferenceService"></param>
		/// <param name="configuration"></param>
		/// <param name="logger"></param>
		public UserPreferenceV1Controller(IPreferenceService preferenceService, IConfiguration configuration,
										ILogger logger)
		{
			this.configuration = configuration;
			this.logger = logger;
			_preferenceService = preferenceService;
		}

		#region User Preference

		/// <summary>
		/// Create Target User Preference
		/// </summary>
		/// <param name="preferencePayload"></param>
		/// <param name="allowUpdate"></param>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("TargetUser")]
		[HttpPost]
		public ActionResult CreateTargetUserPreference([FromBody] CreateUserPreferencePayload preferencePayload,
														bool allowUpdate = false)
		{
			try
			{
				if (!preferencePayload.TargetUserUID.HasValue)
				{
					logger.LogError("Target UserUID has not been provided");
					return BadRequest("Target UserUID has not been provided");
				}

				return DoUpsert(preferencePayload, allowUpdate);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Update Target User preference
		/// </summary>
		/// <param name="preferencePayload"></param>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("TargetUser")]
		[HttpPut]
		public ActionResult UpdateTargetUserPreference([FromBody] UpdateUserPreferencePayload preferencePayload)
		{
			try
			{
				if (!preferencePayload.TargetUserUID.HasValue)
				{
					logger.LogError("Target UserUID has not been provided");
					return BadRequest("Target UserUID has not been provided");
				}

				var userPreferenceEvent = new UpdateUserPreferenceEvent
				{
					PreferenceKeyUID = preferencePayload.PreferenceKeyUID,
					PreferenceKeyName = preferencePayload.PreferenceKeyName,
					PreferenceJson = preferencePayload.PreferenceJson,
					UserUID = preferencePayload.TargetUserUID,
					SchemaVersion = preferencePayload.SchemaVersion,
					ActionUTC = preferencePayload.ActionUTC.Value
				};
				return PerformUpdateUserPreference(userPreferenceEvent);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while updating the targetuserpreference:{ex.Message + ex.StackTrace}");
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Create User Preference
		/// </summary>
		/// <param name="preferencePayload"></param>
		/// <param name="userGuid"></param>
		/// <param name="allowUpdate"></param>
		/// <remarks>Create new User Preference</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("User")]
		[ParseRequestUserUIDFromHeader("userGuid")]
		[HttpPost]
		public ActionResult CreateUserPreference([FromBody] CreateUserPreferencePayload preferencePayload,
												Guid? userGuid = null, bool allowUpdate = false)
		{
			try
			{
				if (!userGuid.HasValue)
				{
					logger.LogError("UserUID has not been provided");
					return BadRequest("UserUID has not been provided");
				}

				preferencePayload.TargetUserUID = userGuid;
				return DoUpsert(preferencePayload, allowUpdate);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500, ex.Message);
			}
		}

		/// <summary>
		/// Update user preference
		/// </summary>
		/// <param name="preferencePayload"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		[Route("User")]
		[ParseRequestUserUIDFromHeader("userGuid")]
		[HttpPut]
		public ActionResult UpdateUserPreference([FromBody] UpdateUserPreferencePayload preferencePayload,
												Guid? userGuid = null)
		{
			try
			{
				if (!userGuid.HasValue)
				{
					logger.LogError("UserUID has not been provided");
					return BadRequest("UserUID has not been provided");
				}

				preferencePayload.TargetUserUID = userGuid;
				var userPreferenceEvent = new UpdateUserPreferenceEvent
				{
					PreferenceKeyUID = preferencePayload.PreferenceKeyUID,
					PreferenceKeyName = preferencePayload.PreferenceKeyName,
					PreferenceJson = preferencePayload.PreferenceJson,
					UserUID = preferencePayload.TargetUserUID,
					SchemaVersion = preferencePayload.SchemaVersion,
					ActionUTC = preferencePayload.ActionUTC.Value
				};
				return PerformUpdateUserPreference(userPreferenceEvent);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while updating the UpdateUserPreference:{ex.Message + ex.StackTrace}");
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Delete User Preference
		/// </summary>
		/// <param name="preferencekeyname"></param>
		/// <param name="preferencekeyuid"></param>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		[Route("User")]
		[HttpDelete]
		public ActionResult DeleteUserPreference(string preferencekeyname = null, Guid? preferencekeyuid = null,
												Guid? userGuid = null)
		{
			if (!userGuid.HasValue)
			{
				logger.LogError("UserUID has not been provided");
				return BadRequest("UserUID has not been provided");
			}

			if (!preferencekeyuid.HasValue && string.IsNullOrEmpty(preferencekeyname))
			{
				logger.LogError("preferenceKeyUID or preferenceKeyName should be given");
				return BadRequest("preferenceKeyUID or preferenceKeyName should be given");
			}

			try
			{
				var delEvent = new DeleteUserPreferenceEvent
				{
					PreferenceKeyUID = preferencekeyuid,
					PreferenceKeyName = preferencekeyname,
					ActionUTC = DateTime.UtcNow,
					UserUID = userGuid.Value
				};

				bool? isSuccess = _preferenceService.DeleteUserPreference(delEvent);
				if (isSuccess.HasValue && isSuccess == true)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey does not exist");
					return BadRequest("PreferenceKey does not exist");
				}

				logger.LogError("Unable to delete in db");
				return BadRequest("Unable to delete in db");
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500, ex.Message);
			}
		}

		private ActionResult DoUpsert(CreateUserPreferencePayload preferencePayload, bool allowUpdate)
		{
			var isExist = _preferenceService.DoesUserPreferenceExist(preferencePayload.TargetUserUID.Value,
				preferencePayload.PreferenceKeyUID, preferencePayload.PreferenceKeyName);

			if (allowUpdate && isExist)
			{
				var userPreferenceEvent = new UpdateUserPreferenceEvent
				{
					PreferenceKeyUID = preferencePayload.PreferenceKeyUID,
					PreferenceKeyName = preferencePayload.PreferenceKeyName,
					PreferenceJson = preferencePayload.PreferenceJson,
					UserUID = preferencePayload.TargetUserUID,
					SchemaVersion = preferencePayload.SchemaVersion,
					ActionUTC = preferencePayload.ActionUTC.Value
				};
				var updateResult = PerformUpdateUserPreference(userPreferenceEvent);
				return updateResult;
			}
			else if (isExist == true && allowUpdate == false)
			{
				logger.LogError("UserPreference already exist");
				return BadRequest("UserPreference already exist");
			}

			#region Create UserPreference

			if (!isExist)
			{
				var preferenceEvent = new CreateUserPreferenceEvent
				{
					PreferenceKeyUID = preferencePayload.PreferenceKeyUID,
					PreferenceKeyName = preferencePayload.PreferenceKeyName,
					PreferenceJson = preferencePayload.PreferenceJson,
					UserUID = preferencePayload.TargetUserUID,
					SchemaVersion = preferencePayload.SchemaVersion,
					ActionUTC = preferencePayload.ActionUTC.Value
				};
				bool? isSuccess = _preferenceService.CreateUserPreference(preferenceEvent);
				if (isSuccess.HasValue && isSuccess.Value)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey does not Exist");
					return BadRequest("PreferenceKey does not Exist");
				}
			}

			#endregion

			logger.LogError("Unable to save row to database");
			return BadRequest("Unable to save row to database");
		}

		private ActionResult PerformUpdateUserPreference(UpdateUserPreferenceEvent userPreferenceEvent)
		{
			try
			{
				bool? isSuccess = _preferenceService.UpdateUserPreference(userPreferenceEvent);
				if (isSuccess.HasValue && isSuccess.Value)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey does not Exist");
					return BadRequest("PreferenceKey does not Exist");
				}

				logger.LogError("Unable to save row to database");
				return BadRequest("Unable to save row to database");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("UserPreference does not Exist"))
				{
					return BadRequest("UserPreference does not Exist");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Get Target User Preferences
		/// </summary>
		/// <param name="userUid">User GUID</param>
		/// <param name="schemaVersion">(optional) version</param>
		/// <param name="keyName">(optional) key name of user preferences</param>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("TargetUser")]
		[HttpGet]
		public ActionResult<UserPreference> GetTargetUserPreferencesForUserAndKey(
			string userUid, string schemaVersion = null, string keyName = null)
		{
			try
			{
				if (!Guid.TryParse(userUid, out var targetUserUid))
				{
					logger.LogError("Invalid UserUID {0}", userUid);
					return BadRequest("Invalid UserUID");
				}

				List<UserPreference> preferences;
				try
				{
					preferences = _preferenceService.GetUserPreferencesForUser(targetUserUid, schemaVersion, keyName);
				}
				catch (Exception ex)
				{
					logger.LogError(ex.Message + ex.StackTrace);
					return StatusCode(500, new Exception("Unable to read from db "));
				}

				if (!string.IsNullOrEmpty(keyName))
				{
					var preference = preferences.FirstOrDefault();
					return Ok(preference);
				}

				return Ok(preferences);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An exception has occurred");
				return StatusCode(500);
			}
		}

		/// <summary>
		/// Get User Preferences
		/// </summary>
		/// <param name="userUID">User GUID</param>
		/// <param name="schemaVersion">(optional) version</param>
		/// <param name="keyName">(optional) key name of user preferences</param>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("User")]
		[ParseRequestUserUIDFromHeader("userUID")]
		[HttpGet]
		public ActionResult GetUserPreferencesForUserAndKey(Guid? userUID = null, string schemaVersion = null,
															string keyName = null)
		{
			try
			{
				if (!userUID.HasValue)
				{
					logger.LogError("UserUID has not been provided");
					return BadRequest("UserUID has not been provided");
				}

				List<UserPreference> preferences;
				try
				{
					preferences = _preferenceService.GetUserPreferencesForUser(userUID.Value, schemaVersion, keyName);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Unable to read from db");
					return StatusCode(500, new Exception("Unable to read from db"));
				}

				var preferenceList = new UserPreferenceList { UserPreferences = preferences };

				if (!string.IsNullOrEmpty(keyName))
				{
					var preference = preferenceList.UserPreferences.FirstOrDefault();
					return Ok(preference);
				}

				return Ok(preferenceList);
			}
			catch (Exception ex)
			{
				logger.LogError("Exception Occured " + ex);
				return BadRequest(ex.Message);
			}
		}

		#endregion

		#region Preference Key

		// POST: v1/user/key
		/// <summary>
		/// Create User Preference Key
		/// </summary>
		/// <param name="preferenceEvent">CreateUserPreferenceKeyEvent model</param>
		/// <remarks>Create new User Preference Key</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("User/key")]
		[HttpPost]
		public ActionResult CreatePreferenceKey([FromBody] CreatePreferenceKeyEvent preferenceEvent)
		{
			try
			{
				bool? isSuccess = _preferenceService.CreatePreferenceKey(preferenceEvent);
				if (isSuccess.HasValue && isSuccess == true)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey already exist");
					return BadRequest("PreferenceKey already exist");
				}

				logger.LogError("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500, ex.Message);
			}
		}


		/// <summary>
		/// Update User Preference Key
		/// </summary>
		/// <param name="preferenceEvent"></param>
		/// <response code="200">Ok</response>
		[Route("User/key")]
		[HttpPut]
		public ActionResult UpdatePreferenceKey([FromBody] UpdatePreferenceKeyEvent preferenceEvent)
		{
			try
			{
				bool? isSuccess = _preferenceService.UpdatePreferenceKey(preferenceEvent);
				if (isSuccess.HasValue && isSuccess == true)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey does not exist");
					return BadRequest("PreferenceKey does not exist");
				}

				logger.LogError("Unable to save to db. Make sure request is not duplicated and all keys exist");
				return BadRequest("Unable to save to db. Make sure request is not duplicated and all keys exist");
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("PreferenceKey Name Already Exist"))
				{
					return BadRequest("PreferenceKey Name Already Exist");
				}

				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500, ex.Message);
			}
		}

		/// <summary>
		/// Delete User Preference Key
		/// </summary>
		/// <param name="preferenceKeyGuid"></param>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("User/key/{preferenceKeyGuid}")]
		[HttpDelete]
		public ActionResult DeletePreferenceKey(string preferenceKeyGuid = null)
		{
			try
			{
				Guid preferenceKeyUid;
				if (!Guid.TryParse(preferenceKeyGuid, out preferenceKeyUid))
				{
					logger.LogError("PreferenceKeyGuid is not valid");
					return BadRequest("PreferenceKeyGuid is not valid");
				}

				var preferenceEvent = new DeletePreferenceKeyEvent()
				{
					PreferenceKeyUID = preferenceKeyUid,
					ActionUTC = DateTime.UtcNow
				};

				bool? isSuccess = _preferenceService.DeletePreferenceKey(preferenceEvent);
				if (isSuccess.HasValue && isSuccess == true)
				{
					return Ok();
				}
				else if (isSuccess == null)
				{
					logger.LogError("PreferenceKey does not exist");
					return BadRequest("PreferenceKey does not exist");
				}

				logger.LogError("Unable to delete in db");
				return BadRequest("Unable to delete in db");
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode(500, ex.Message);
			}
		}

		#endregion User Preference Key
	}
}