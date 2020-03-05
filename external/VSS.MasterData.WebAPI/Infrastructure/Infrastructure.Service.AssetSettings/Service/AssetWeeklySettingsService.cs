using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.AssetSettings;
using CommonModel.Error;
using CommonModel.Exceptions;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Enums;
using Infrastructure.Service.AssetSettings.Interfaces;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Service
{
	public abstract class AssetWeeklySettingsService
	{
		protected IWeeklyAssetSettingsRepository _weekRepo;
		protected IAssetSettingsTypeHandler<AssetSettingsBase> _Converter;
		protected AssetSettingsOverlapTemplate _assetSettingsOverlap;
		protected GroupType _groupType;
		protected IAssetSettingsPublisher _assetSettingsPublisher;
		protected IValidationHelper _validationHelper;
		protected ILoggingService _loggingService;
		private readonly ITransactions _transactions;


		protected abstract List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(string assestUIDs, DateTime startDate, DateTime endDate);
		protected abstract List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUID(string assetUIDs);
		protected abstract List<AssetErrorInfo> DoValidation(string[] AssetUID, DateTime? startDate, DateTime? endDate);
		protected abstract List<AssetErrorInfo> DoValidation(AssetSettingsBase[] assetSettings);

		protected AssetWeeklySettingsService(ITransactions transactions, ILoggingService loggingService)
		{
			this._loggingService = loggingService;
			this._loggingService.CreateLogger<AssetWeeklySettingsService>();
			this._transactions = transactions;
		}


		public async Task<GetAssetWeeklyTargetsResponse> GetAssetSettings(string[] assetUIDs, DateTime? startDate, DateTime? endDate)
		{
			GetAssetWeeklyTargetsResponse response = null;
			try
			{
				var isValidRequest = DoValidation(assetUIDs, startDate, endDate);

				if (assetUIDs.All(assetUID => isValidRequest.Select(request => request.AssetUID).Contains(assetUID)))
				{
					//Condition Where All the Asset Has Error, Throw Domain Exception Then
					throw new DomainException
					{
						Errors = isValidRequest
					};
				}

				if (isDatePresent(startDate, endDate))
				{
					_loggingService.Debug(string.Format("DatePresent - Call To DB For Extracting AssetWeeklySettingsBased on StartDate : {0} And EndDate : {1}", startDate.Value, endDate.Value), "AssetWeeklySettingsService.GetAssetSettings");
					var assetSettings = GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(string.Join(",", assetUIDs.Where(assetUID => !isValidRequest.Select(request => request.AssetUID).Contains(assetUID))).WrapCommaSeperatedStringsWithUnhex(), startDate.Value, endDate.Value);
					response = new GetAssetWeeklyTargetsResponse(_Converter.ExtractToAssetUtilizationTargets(assetSettings, _groupType).ToList());
				}
				else
				{
					_loggingService.Debug(string.Format("DatePresent - Call To DB For Extracting AssetWeeklySettings Based on AssertUID {0}", string.Join(",", assetUIDs.Where(assetUID => !isValidRequest.Select(request => request.AssetUID).Contains(assetUID)))), "AssetWeeklySettingsService.GetAssetSettings");
					var assetSettingsByAssetUID = GetAssetSettingsForTargetTypeWithAssetUID(string.Join(",", assetUIDs.Where(assetUID => !isValidRequest.Select(request => request.AssetUID).Contains(assetUID))).WrapCommaSeperatedStringsWithUnhex());
					response = new GetAssetWeeklyTargetsResponse(_Converter.ExtractToAssetUtilizationTargets(assetSettingsByAssetUID, _groupType).ToList());
				}
				if (isValidRequest.Count > 0)
					response.Errors = isValidRequest;
			}
			catch (DomainException ex)
			{
				_loggingService.Error(ex.Message, "AssetWeeklySettingsService.GetAssetSettings", ex);
				return new GetAssetWeeklyTargetsResponse(new List<AssetErrorInfo>(ex.Errors.OfType<AssetErrorInfo>().ToList()));
			}
			catch (ArgumentException ex)
			{
				_loggingService.Error(ex.Message, "AssetWeeklySettingsService.GetAssetSettings", ex);
				return new GetAssetWeeklyTargetsResponse(new AssetErrorInfo { Message = "Arguments Are Wrong", ErrorCode = 400102 });
			}
			return response;
		}

		protected List<AssetSettingsGetDBResponse> BuildAssetSetting(AssetSettingsBase target, List<AssetSettingsGetDBResponse> assetTargets)
		{
			List<AssetSettingsGetDBResponse> assetWeeklySettings = new List<AssetSettingsGetDBResponse>();
			_loggingService.Debug(string.Format("Call To DB For Extracting AssetWeeklySettingsBased on StartDate : {0} And EndDate : {1}", target.StartDate, target.EndDate.Value), "AssetWeeklySettingsService.SaveAssetSetting");
			//var assetTargets = GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(target.AssetUID.ToStringWithoutHyphens().WrapCommaSeperatedStringsWithUnhex(), target.StartDate, target.EndDate.Value);
			var assetTargetsByAssetUID = assetTargets.Where(x => x.AssetUID == target.AssetUID).ToList();
			if (!isRecordExists(assetTargetsByAssetUID, _groupType))
			{
				/*
                 * Record Doesn't Exist for the Week.. 
                 * Insert a New Record, 
                 * publish Inserted AssetConfigWeeklyUID into kafka 
                 * Return
                 */
				// using (var scope = new TransactionScope())
				{
					_loggingService.Debug("No Records Insert a New Record", "AssetWeeklySettingsService.SaveAssetSetting");
					//var response = new List<AssetSettingsGetDBResponse>();
					var targets = _Converter.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
					// _weekRepo.InsertAssetTargets(targets);
					targets.ForEach(value => { value.Status = true; value.OperationType = AssetSettingsOperationType.Insert; });
					// if (_utils.PublishToKafka(targets))
					//scope.Complete();
					assetWeeklySettings.AddRange(targets);
					//return response;
				}
			}
			else
			{
				assetWeeklySettings = _assetSettingsOverlap.HandleOverlap(target, assetTargetsByAssetUID, _groupType);
			}
			return assetWeeklySettings;
		}

		public async Task<EditAssetTargetResponse> EditAssetSettings(AssetSettingsBase[] targets)
		{
			var processedAssetUID = new List<string>();
			try
			{
				//if (isValidRequest.Select(request => request.AssetUID).Distinct().All(requestIds => assetUIDs.Contains(requestIds)))
				var isValidRequest = DoValidation(targets);
				var response = new List<AssetSettingsGetDBResponse>();
				var assetUIDList = targets.Select(target => target.AssetUID.ToString()).Distinct();
				if (isValidRequest.Any() && isValidRequest.Select(request => request.AssetUID).Distinct().All(requestIds => assetUIDList.Contains(requestIds)))
					throw new DomainException
					{
						Errors = isValidRequest
					};

				var validTargets = targets.Where(assetTarget => !isValidRequest.Any(request => request.AssetUID.Contains(assetTarget.AssetUID.ToString())));

				var assetTargets = GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(string.Join(",", validTargets.Select(x => x.AssetUID.ToStringWithoutHyphens().WrapCommaSeperatedStringsWithUnhex())), targets.First().StartDate, targets.First().EndDate.Value);

				foreach (var target in validTargets)
				{
					response.AddRange(BuildAssetSetting(target, assetTargets));
				}

				this.PersistAndPublishWeeklySettings(response);

				return new EditAssetTargetResponse(response.Select(target => target.AssetUID.ToString()).Distinct().ToList());
			}
			catch (DomainException ex)
			{
				return new EditAssetTargetResponse(ex.Errors.OfType<AssetErrorInfo>().ToList());
			}
			catch (Exception e)
			{
				return new EditAssetTargetResponse(new AssetErrorInfo { Message = "Unexpected_Error", ErrorCode = 500100 });
			}
		}


		private List<AssetWeeklySettingsDto> BuildAssetWeeklySettings(List<AssetSettingsGetDBResponse> assetWeeklySettings, DateTime currentUTC)
		{
			var assetWeeklySettingsList = new List<AssetWeeklySettingsDto>();
			assetWeeklySettingsList.AddRange(assetWeeklySettings.Select(assetConfig => new AssetWeeklySettingsDto
			{
				AssetConfigTypeID = (int)Enum.Parse(typeof(AssetTargetType), assetConfig.ConfigType),
				InsertUTC = currentUTC,
				UpdateUTC = currentUTC,
				AssetWeeklyConfigUID = Guid.Parse(assetConfig.AssetWeeklyConfigUID),
				AssetUID = assetConfig.AssetUID,
				EndDate = assetConfig.EndDate,
				StartDate = assetConfig.StartDate,
				SundayConfigValue = assetConfig.Sunday,
				MondayConfigValue = assetConfig.Monday,
				TuesdayConfigValue = assetConfig.Tuesday,
				WednesdayConfigValue = assetConfig.Wednesday,
				ThursdayConfigValue = assetConfig.Thursday,
				FridayConfigValue = assetConfig.Friday,
				SaturdayConfigValue = assetConfig.Saturday,
				StatusInd = assetConfig.Status
			}));
			return assetWeeklySettingsList;
		}

		private void PersistAndPublishWeeklySettings(List<AssetSettingsGetDBResponse> assetSettingsGetDBResponses)
		{
			var currentDateTime = DateTime.UtcNow;

			var deleteSettings = assetSettingsGetDBResponses.Where(x => x.OperationType == AssetSettingsOperationType.Delete).ToList();
			var upsertSettings = assetSettingsGetDBResponses.Where(x => x.OperationType == AssetSettingsOperationType.Insert || x.OperationType == AssetSettingsOperationType.Update).ToList();

			List<Action> actions = new List<Action>();

			//delete query
			if (deleteSettings.Count > 0)
			{
				var deleteWeeklySettings = this.BuildAssetWeeklySettings(deleteSettings, currentDateTime);
				actions.Add(() => _transactions.Delete(deleteWeeklySettings));
			}

			//upsert query
			if (upsertSettings.Count > 0)
			{
				var upsertWeeklySettingsList = this.BuildAssetWeeklySettings(upsertSettings, currentDateTime);
				actions.Add(() => _transactions.Upsert<AssetWeeklySettingsDto>(upsertWeeklySettingsList));
			}

			List<AssetWeeklyTargetsDto> targetsDto = new List<AssetWeeklyTargetsDto>();
			foreach (var value in assetSettingsGetDBResponses)
			{
				targetsDto.Add(new AssetWeeklyTargetsDto
				{
					AssetTargetUID = Guid.Parse(value.AssetWeeklyConfigUID),
					AssetUID = value.AssetUID,
					EndDate = value.EndDate,
					StartDate = value.StartDate,
					TargetType = (AssetTargetType)Enum.Parse(typeof(AssetTargetType), value.ConfigType),
					SundayTargetValue = value.Sunday,
					MondayTargetValue = value.Monday,
					TuesdayTargetValue = value.Tuesday,
					WednesdayTargetValue = value.Wednesday,
					ThursdayTargetValue = value.Thursday,
					FridayTargetValue = value.Friday,
					SaturdayTargetValue = value.Saturday,
					Status = value.Status
				});
			}

			actions.Add(() => _assetSettingsPublisher.publishAssetWeeklySettings(targetsDto));

			if (actions.Count > 0)
			{
				_transactions.Execute(actions);
			}
		}

		private static bool isDatePresent(DateTime? startDate, DateTime? endDate)
		{
			return startDate.HasValue || endDate.HasValue;
		}
		private static bool isRecordExists(List<AssetSettingsGetDBResponse> assetTargets, GroupType type)
		{
			if (type == GroupType.AssetTargets)
			{
				return assetTargets.Any(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString())) &&
									(assetTargets.Any(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.IdletimeHours.ToString())));
			}
			else
			{
				return assetTargets.Any(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.PayloadinTonnes.ToString())) &&
									(assetTargets.Any(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.VolumeinCuMeter.ToString())))
									&& (assetTargets.Any(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.CycleCount.ToString())));
			}
		}
	}
}
