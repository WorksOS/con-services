using CommonModel.Enum;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AssetSettingsRepository.Helpers
{
	public class Constants
    {
        public static IReadOnlyDictionary<AssetSettingsFilters, string> AssetSettingsFilterConfig = new ReadOnlyDictionary<AssetSettingsFilters, string>(
            new Dictionary<AssetSettingsFilters, string>
            {
                { AssetSettingsFilters.All, "(a.AssetName LIKE '%{0}%' OR a.SerialNumber LIKE '%{0}%')" },
                { AssetSettingsFilters.AssetId, "a.AssetName LIKE '%{0}%'" },
                { AssetSettingsFilters.AssetSerialNumber, "a.SerialNumber LIKE '%{0}%'" },
                { AssetSettingsFilters.DeviceType, "ast.TypeName = '{0}'" },
                { AssetSettingsFilters.AssetUID, "ca.fk_AssetUID IN ({0})" },
                { AssetSettingsFilters.SubAccountCustomerUid, "a.AssetUID IN ({0})" }
            });

        public static IReadOnlyDictionary<AssetSettingsSortColumns, string> AssetSettingsSortConfig = new ReadOnlyDictionary<AssetSettingsSortColumns, string>(
            new Dictionary<AssetSettingsSortColumns, string>
            {
                        { AssetSettingsSortColumns.AssetId, "AssetName" },
                        { AssetSettingsSortColumns.AssetSerialNumber, "SerialNumber" },
                        { AssetSettingsSortColumns.AssetModel, "Model" },
                        { AssetSettingsSortColumns.AssetMakeCode, "MakeCode" },
                        { AssetSettingsSortColumns.DeviceSerialNumber, "DeviceSerialNumber" },
                        { AssetSettingsSortColumns.TargetStatus, "TargetStatus" },
                        { AssetSettingsSortColumns.DailyLocationReportingFrequency, "DailyLocationReportingFrequency"},
                        { AssetSettingsSortColumns.DailyReportingTime, "DailyReportingTime"},
                        { AssetSettingsSortColumns.DiagnosticReportFrequency, "DiagnosticReportFrequency" },
                        { AssetSettingsSortColumns.EventDiagnosticFilterInterval, "EventDiagnosticFilterInterval" },
                        { AssetSettingsSortColumns.HighSeverityEvents, "HighSeverityEvents" },
                        { AssetSettingsSortColumns.HourMeterFuelReport, "HourMeterFuelReport" },
                        { AssetSettingsSortColumns.HoursMeter, "HoursMeter" },
						{ AssetSettingsSortColumns.LowSeverityEvents, "LowSeverityEvents" },
                        { AssetSettingsSortColumns.MaintenanceModeDuration, "MaintenanceModeDuration" },
                        { AssetSettingsSortColumns.MediumSeverityEvents, "MediumSeverityEvents" },
                        { AssetSettingsSortColumns.MovingOrStoppedThreshold, "MovingOrStoppedThreshold" },
                        { AssetSettingsSortColumns.MovingThresholdsDuration, "MovingThresholdsDuration" },
                        { AssetSettingsSortColumns.MovingThresholdsRadius, "MovingThresholdsRadius" },
                        { AssetSettingsSortColumns.NextSentEventInHours, "NextSentEventInHours" },
						{ AssetSettingsSortColumns.Odometer, "Odometer" },
						{ AssetSettingsSortColumns.ReportAssetStartStop, "ReportAssetStartStop" },
                        { AssetSettingsSortColumns.SentUTC, "SentUTC" },
                        { AssetSettingsSortColumns.SMHOdometerConfig, "SMHOdometerConfig" },
                        { AssetSettingsSortColumns.SpeedThreshold, "SpeedThreshold" },
                        { AssetSettingsSortColumns.SpeedThresholdDuration, "SpeedThresholdDuration" },
                        { AssetSettingsSortColumns.SpeedThresholdEnabled, "SpeedThresholdEnabled" },
                        { AssetSettingsSortColumns.StartTime, "StartTime" },
                        { AssetSettingsSortColumns.Status, "Status" },
                        { AssetSettingsSortColumns.ConfiguredSwitches, "ConfiguredSwitches" },
                        { AssetSettingsSortColumns.GlobalGram, "GlobalGram" },
                        { AssetSettingsSortColumns.WorkDefinition, "WorkDefinition" },
                        { AssetSettingsSortColumns.SecurityStatus, "SecurityStatus" },
                        { AssetSettingsSortColumns.SecurityMode, "SecurityMode" },
                        { AssetSettingsSortColumns.DeviceType, "DeviceType" }
            });
    }
}
