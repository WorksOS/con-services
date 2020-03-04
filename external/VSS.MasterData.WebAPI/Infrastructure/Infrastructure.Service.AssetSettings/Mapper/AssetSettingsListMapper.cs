using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Response.AssetTargets;
using DbModel.AssetSettings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Service.AssetSettings.Mapper
{
	public class AssetSettingsListMapper : IMappingAction<AssetSettingsListDto, AssetSettingsDetails>
	{
		private readonly List<string> _clearOffPendingValueForDeviceTypes;
		private readonly int _clearOffPendingValueGreaterThanNoOfDays;
		
		public AssetSettingsListMapper(IOptions<Configurations> configurations)
		{
			_clearOffPendingValueForDeviceTypes = configurations.Value.ApplicationSettings.ClearOffPendingValueForDeviceTypes.Split(',').ToList();
			_clearOffPendingValueGreaterThanNoOfDays = configurations.Value.ApplicationSettings.ClearOffPendingValueGreaterThanNoOfDays;
		}

		private bool CheckForClearingOffPendingValue(string deviceType, DateTime pendingTimestamp)
		{
			return _clearOffPendingValueForDeviceTypes.Contains(deviceType) && DateTime.UtcNow.Subtract(pendingTimestamp).Days > _clearOffPendingValueGreaterThanNoOfDays;
		}

		public void Process(AssetSettingsListDto dto, AssetSettingsDetails model, ResolutionContext context)
		{
			if (dto.ReportingSchedulePendingUpdatedOn.HasValue ||
					dto.SpeedingThresholdPendingUpdatedOn.HasValue ||
					dto.MovingThresholdPendingUpdatedOn.HasValue ||
					dto.MaintenanceModePendingUpdatedOn.HasValue ||
					dto.MetersPendingUpdatedOn.HasValue ||
					dto.SwitchesPendingUpdatedOn.HasValue ||
					dto.FaultCodeReportingPendingUpdatedOn.HasValue ||
					dto.AssetSecurityPendingUpdatedOn.HasValue)
			{
				model.PendingDeviceConfigInfo = new PendingDeviceConfigDetails
				{
					FaultCodeReporting = dto.FaultCodeReportingPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.FaultCodeReportingPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.FaultCodeReportingPendingUpdatedOn
					} : null,
					ReportingSchedule = dto.ReportingSchedulePendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.ReportingSchedulePendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.ReportingSchedulePendingUpdatedOn
					} : null,
					Meters = dto.MetersPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.MetersPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.MetersPendingUpdatedOn
					} : null,
					Switches = dto.SwitchesPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.SwitchesPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.SwitchesPendingUpdatedOn
					} : null,
					MaintenanceMode = dto.MaintenanceModePendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.MaintenanceModePendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.MaintenanceModePendingUpdatedOn
					} : null,
					MovingThresholds = dto.MovingThresholdPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.MovingThresholdPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.MovingThresholdPendingUpdatedOn
					} : null,
					SpeedingThresholds = dto.SpeedingThresholdPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.SpeedingThresholdPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.SpeedingThresholdPendingUpdatedOn
					} : null,
					AssetSecurity = dto.AssetSecurityPendingUpdatedOn.HasValue && !CheckForClearingOffPendingValue(dto.DeviceType, dto.AssetSecurityPendingUpdatedOn.Value) ? new GroupDetail
					{
						LastUpdatedOn = dto.AssetSecurityPendingUpdatedOn
					} : null
				};
			}
		}
	}
}
