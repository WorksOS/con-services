using System.ComponentModel;

namespace Infrastructure.Common.DeviceSettings.Enums
{
    public enum ErrorCodes
    {
        #region InternalServerError - Starts with 500

        #region Unexpected Error - Postfixed with 100

        [Description("An Unexpected Error has occurred")]
        UnexpectedError = 5001001,

        #endregion

        #endregion

        #region Bad Request - Starts with 400

        #region Request - Null Case
        [Description("Request is null")]
        RequestNull = 400300,
        #endregion

        #region AssetValidator - Postfixed with 031

        [Description("AssetUIDs is null")]
        AssetUIDListNull = 4000311,

        [Description("Invalid AssetUID")]
        InvalidAssetUID = 4000312,

        [Description("Following AssetUIDs not in guid format : {0}")]
        InvalidGuidFormatForAssetUID = 4000313,

        #endregion

        #region DeviceType and Parameter Group Validator - Postfixed with 120

        [Description("Invalid Device Type {0}")]
        InvalidDeviceType = 4001201,

        [Description("Device Type is null or empty")]
        DeviceTypeNull = 4001202,

        [Description("Invalid Device Parameter Group")]
        InvalidDeviceParamGroup = 4001203,

        [Description("Invalid Parameter Group for Device Type")]
        InvalidParameterGroupForDeviceType = 4001204,

        #endregion

        #region DeviceParamGroupValidator - Postfixed with 121

        [Description("{0} field/attribute is missing or misspelled")]
        DeviceAttributeMissing = 4001210,

        [Description("No Attribute mentioned")]
        NoDeviceAttributeMentioned = 4001211,

        #endregion

        #region Moving Threshold - Postfixed with 122

        [Description("Moving Threshold duration should be equal to or between {0} and {1}")]
        MovingThresholdInvalidDuration = 4001220,

        [Description("Moving Threshold Settings should be either one of the following : {0}")]
        MovingThresholdInvalidSettings = 4001221,

        [Description("Moving Threshold radius should be equal to or between {0} and {1}")]
        MovingThresholdInvalidRadius = 4001222,

        [Description("Moving Threshold speed should be equal to or between {0} and {1}")]
        MovingThresholdInvalidSpeed = 4001223,

        [Description("Moving Started / Stopped Threshold should be equal to or between {0} and {1}")]
        MovingThresholdInvalidStoppedThreshold = 4001224,

        #endregion

        #region Reporting Schedule - Postfixed with 123

        [Description("Reporting Schedule Reporting Time should be equal to or between {0} and {1}")]
        ReportingScheduleInvalidReportingTime = 4001230,

        [Description("Reporting Schedule Location Reporting Frequency should be either one of the following : {0}")]
        ReportingScheduleInvalidDailyLocationReportingFrequency = 4001231,

        [Description("Reporting Schedule Hour Meter Fuel Report should be either one of the following : {0}")]
        ReportingScheduleInvalidHourMeterFuelReport = 4001232,

        #endregion

        #region Meters - Postfixed with 124

        [Description("Service Meter configuration should be either one of the following : {0}")]
        MetersInvalidSmhOdometerConfig = 4001240,

        [Description("Device Type {0} doesn't support updating Hour Meter")]
        MetersInvalidDeviceTypeForHourMeter = 4001241,

        [Description("Device Type {0} doesn't support updating Odo Meter")]
        MetersInvalidDeviceTypeForOdoMeter = 4001242,

        [Description("Hour Meter proposed value should be equal to or between {0} and {1}")]
        MetersInvalidProposedValueForHourMeter = 4001243,

        [Description("Odo Meter proposed value should be equal to or between {0} and {1}")]
        MetersInvalidProposedValueForOdoMeter = 4001244,

        [Description("Hours Meter updation not allowed for DataLink")]
        MetersUpdationNotAllowedForHoursMeter = 4001245,

        [Description("Odo Meter updation not allowed for DataLink")]
        MetersUpdationNotAllowedForOdoMeter = 4001246,

        [Description("Odo Meter current value should be equal to or between {0} and {1}")]
        MetersInvalidCurrentValueForOdoMeter = 4001247,

        [Description("Hours Meter current value should be equal to or between {0} and {1}")]
        MetersInvalidCurrentValueForHoursMeter = 4001248,

        [Description("Hours meter value can't be set backward")]
        MetersBackwardUpdationNotAllowedForHoursMeter = 4001249,

        [Description("Hours Meter proposed value can have atmost {0} decimal digit")]
        MetersInvalidPrecisionValueForHourMeter = 40012410,
        #endregion

        #region Maintenance Mode - Postfixed with 125

        [Description("Duration should be equal to or between {0} and {1}")]
        MaintenanceModeInvalidDuration = 4001250,

        [Description("Reporting Schedule Reporting Time should be equal to or between {0} and {1}")]
        MaintenanceModeInvalidStartTime = 4001251,

        #endregion

        #region Fault Code Reporting - Postfixed with 126

        [Description("Low Severity Event should be either one of the following : {0}")]
        FaultCodeReportingInvalidLowSeverityEvents = 4001260,

        [Description("Medium Severity Event should be either one of the following : {0}")]
        FaultCodeReportingInvalidMediumSeverityEvents = 4001261,

        [Description("High Severity Event should be either one of the following : {0}")]
        FaultCodeReportingInvalidHighSeverityEvents = 4001262,

        [Description("Diagnostic Report Frequency should be either one of the following : {0}")]
        FaultCodeReportingInvalidDiagnosticReportFrequency = 4001263,

        [Description("Next Sent Event Hours should be equal to or between {0} and {1}")]
        FaultCodeReportingInvalidNextSentEvent = 4001264,

        [Description("Diagnostic Filter Interval should be equal to or between {0} and {1}")]
        FaultCodeReportingInvalidDiagnosticFilterInterval = 4001265,

        #endregion

        #region Speeding Thresholds - Postfixed with 127

        [Description("Speeding Thresholds duration should be equal to or between {0} and {1}")]
        SpeedingThresholdsInvalidDuration = 4001270,

        [Description("Speeding Threshold should be equal to or between {0} and {1}")]
        SpeedingThresholdsInvalidValue = 4001271,

        #endregion

        #region Asset Security - Postfixed with 128

        [Description("Asset Security's Security Status should be either one of the following : {0}")]
        AssetSecurityInvalidSecurityStatus = 4001280,

        #endregion

        #region CustomerUIDValidator - Postfixed with 041

        [Description("CustomerUid is null")]
        CustomerUIDNull = 4000411,

        #endregion

        #region AssetDeviceMapping Invalid - Postfixed With 130
        [Description("AssetDeviceType Mapping is Missing")]
        AssetDeviceMappingMissing = 4001300,
        [Description("AssetDeviceType Mapping is Invalid")]
        AssetDeviceMappingInvalid = 4001301,
        #endregion

        #region UserUIDValidator - Postfixed with 051

        [Description("UserUid is null")]
        UserUIDNull = 4000511,

        #endregion        

        #region SwitchesValidator - Postfixed With 140
        [Description("{0}, SwitchName is null")]
        SwitchNameNull = 400140,

        [Description("{0}, SwitchName is Invalid, It can contain 64 Characters With Space Allowed")]
        SwitchNameInvalid = 400141,

        [Description("{0}, SwitchOpen is Invalid, It can contain 64 Characters With Space Allowed")]
        SwitchOpenInvalid = 400142,

        [Description("{0}, SwitchClosed is Invalid, It can contain 64 Characters With Space Allowed")]
        SwitchClosedInvalid = 400143,

        [Description("{0}, Sensitivity Can Contain Values Between 0 And 6550")]
        SwitchSensitivityInvalid = 400144,

        [Description("{0}, Sensitivity Can Contain Values Between 0 And 6550")]
        SwitchSensitivityDualStateSwitchInvalid = 400153,

        [Description("{0}, Sensitivity is Null")]
        SwitchSensitivityNull = 400146,

        [Description("{0}, SwitchActiveStatus is Invalid")]
        SwitchActiveStatusInvalid = 400148,

        [Description("{0}, SwitchMonitoringStatus is Invalid")]
        SwitchMonitoringStatusInvalid = 400147,

        [Description("SwitchParameter, SwitchParameter is Null")]
        SwitchParameterNull = 400146,

        [Description("Atleast OneConfiguration Needs to Be Set Before An Update Message Can be Sent To The Device")]
        SwitchAtleastOneConfigurationValueRequired = 400145,

        [Description("SwitchParameter, SwitchParameter {0} is Invalid")]
        InvalidParameters = 400149,

        [Description("InvalidAttribute, Switch Attribute  is Invalid")]
        InvalidAttributes = 400150,

        [Description("{0}, Switch Number is Invalid")]
        SwitchNumberInvalid = 400151,

        [Description("Request Is Invalid")]
        RequestInvalid = 400152,

        [Description("{0}, SwitchOpen is Null")]
        SwitchOpenNull = 400154,

        [Description("{0}, SwitchClose is Null")]
        SwitchCloseNull = 400155,

        [Description("{0}, Tampered Switch, Not Allowed to Edit")]
        SwitchTamperedNotAllowedToEdit = 400155,

        [Description("{0}, DeviceType does not support dual state switches")]
        DeviceTypeNotSupported = 400156,

        #endregion

        #region Asset Device Configuration Attribute Validator - Postfixed With 150

        [Description("SwitchActiveStatus is Invalid")]
        AttributeValidation_SwitchActiveStatusInvalid = 4001501,

        [Description("SwitchMonitoringStatus is Invalid")]
        AttributeValidation_SwitchMonitoringStatusInvalid = 4001502,

        [Description("Sensitivity Can Contain Values Between 0 And 6550")]
        AttributeValidation_SwitchSensitivityInvalid = 4001503,

        [Description("Switch Number is Invalid")]
        AttributeValidation_SwitchNumberInvalid = 4001504,

        [Description("Report Asset Start/Stop is Invalid")]
        AttributeValidation_ReportAssetStartStopInvalid = 4001505,

        [Description("Daily Reporting Time is Invalid")]
        AttributeValidation_DailyReportingTimeInvalid = 4001506,

        [Description("Daily Location Reporting Frequency is Invalid")]
        AttributeValidation_DailyLocationReportingFrequencyInvalid = 4001507,

        [Description("Hour Meter Fuel Report is Invalid")]
        AttributeValidation_HourMeterFuelReportInvalid = 4001508,

        [Description("Global Gram is Invalid")]
        AttributeValidation_GlobalGramInvalid = 4001509,

        [Description("Hours Meter is Invalid")]
        AttributeValidation_HoursMeterInvalid = 40015010,

        [Description("Low Severity Event is Invalid")]
        AttributeValidation_LowSeverityEventsInvalid = 40015011,

        [Description("Medium Severity Events is Invalid")]
        AttributeValidation_MediumSeverityEventsInvalid = 40015012,

        [Description("High Severity Events is Invalid")]
        AttributeValidation_HighSeverityEventsInvalid = 40015013,

        [Description("Diagnostic Report Frequency is Invalid")]
        AttributeValidation_DiagnosticReportFrequencyInvalid = 40015014,

        [Description("Event Diagnostic Filter Interval is Invalid")]
        AttributeValidation_EventDiagnosticFilterIntervalInvalid = 40015015,

        [Description("Next Sent EventIn Hours is Invalid")]
        AttributeValidation_NextSentEventInHoursInvalid = 40015016,
        #endregion
		#region Subscription - Postfixed with 150

		[Description("Asset Subscription doesn't allow configuring {0}")]
		AssetSubscriptionIsInvalid = 4001500,
        

		#endregion


        #region Device Ping Errors
        [Description("There is already an open Ping Request available for this Asset & Device combination")]
        DuplicatePingRequest = 400201,

        [Description("Ping Request not found for this Asset & Device combination")]
        PingRequestNotFound = 400202,

        [Description("Ping Request not saved due some error")]
        PingRequestNotSaved = 400203,

        [Description("AssetUID is null")]
        AssetUIDIsNull = 400204,

        [Description("DeviceUID is null")]
        DeviceUIDIsNull = 400205,

        [Description("Invalid Request. Asset/Device configuration does not exists")]
        AssetDeviceConfigNotExists = 400206,

        [Description("Device Type Family Not Supported")]
        DeviceTypeNotSupportedError = 400207,

        [Description("Message Construction Failed")]
        MsgConstructionFailed = 400208,

            [Description("Device Type Family Not Supported")]
        DeviceTypeFamilyNotSupported = 400209

        #endregion
        #endregion
    }
}
