using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetSettingsRepository
{
    public class Queries
    {
        /// <summary>
        ///Asset Targets List Query
        /// </summary>

		public const string FetchAssetsForCustomerAndUserUId = @"SELECT SQL_CALC_FOUND_ROWS
																	*
																FROM
																	(SELECT 
																		ast.AssetUID,
																			ast.DeviceUID,
																			HEX(a.AssetUID) AS AssetUIDString,
																			a.AssetName AS AssetName,
																			a.SerialNumber AS SerialNumber,
																			a.IconKey AS IconKey,
																			a.MakeCode AS MakeCode,
																			a.Model AS Model,
																			a.AssetID,
																			ast.TypeName as DeviceType,
																			d.SerialNumber as DeviceSerialNumber,
																			ast.EventDiagnosticFilterInterval,
																			ast.GlobalGram,
																			ast.HighSeverityEvents,
																			ast.DailyLocationReportingFrequency,
																			ast.DailyReportingTime,
																			ast.DiagnosticReportFrequency,
																			ast.HourMeterFuelReport,
																			ast.LowSeverityEvents,
																			ast.MaintenanceModeDuration,
																			ast.MediumSeverityEvents,
																			ast.MovingOrStoppedThreshold,
																			ast.MovingThresholdsDuration,
																			ast.MovingThresholdsRadius,
																			ast.NextSentEventInHours,
																			ast.ReportAssetStartStop,
																			ast.SentUTC,
																			ast.SMHOdometerConfig,
																			ast.SpeedThreshold,
																			ast.SpeedThresholdDuration,
																			ast.SpeedThresholdEnabled,
																			ast.StartTime,
																			ast.Status,
																			ast.SecurityMode,
																			ast.SecurityStatus,
																			ast.ConfiguredSwitches,
																			ast.ReportingSchedulePendingUpdatedOn,
																			ast.MovingThresholdPendingUpdatedOn,
																			ast.MaintenanceModePendingUpdatedOn,
																			ast.FaultCodeReportingPendingUpdatedOn,
																			ast.SwitchesPendingUpdatedOn,
																			ast.SpeedingThresholdPendingUpdatedOn,
																			ast.MetersPendingUpdatedOn,
																			ast.AssetSecurityPendingUpdatedOn,
																			IF(EXISTS( (SELECT 
																					AssetWeeklyConfigID AS AssetId
																				FROM
																					md_asset_AssetWeeklyConfig
																				WHERE
																					fk_AssetUID = a.AssetUID AND EndDate >= DATE(UTC_TIMESTAMP())
																				LIMIT 1) UNION (SELECT 
																					AssetConfigID AS AssetId
																				FROM
																					md_asset_AssetConfig
																				WHERE
																					fk_AssetUID = a.AssetUID)), 1, 0) AS 'TargetStatus',
																			(SELECT 
																					fk_WorkDefinitionTypeID
																				FROM
																					md_asset_AssetWorkDefinition
																				WHERE
																					fk_AssetUID = a.AssetUID
																				ORDER BY StartDate DESC
																				LIMIT 1) AS 'WorkDefinition',
																			CAST(IFNULL(hMeter.RuntimeHours, 0.0) AS DECIMAL) AS 'HoursMeter',
																			CAST(IFNULL(oMeter.OdometerKilometers, 0.0) AS DECIMAL) AS 'Odometer'																			
																	FROM
																		(SELECT 
																			a.AssetUID,
																			d.DeviceUID,
																			DT.TypeName,
																			CAST(MAX(CASE
																					WHEN A.AttributeName = 'EventDiagnosticFilterInterval' THEN DC.AttributeValue
																				END)
																				AS UNSIGNED) AS 'EventDiagnosticFilterInterval',
																			CAST(MAX(CASE
																					WHEN
																						A.AttributeName = 'GlobalGram'
																					THEN
																						CASE
																							WHEN LOWER(DC.AttributeValue) = 'true' THEN 1
																							ELSE 0
																						END
																				END)
																				AS UNSIGNED) AS 'GlobalGram',
																			MAX(CASE
																				WHEN A.AttributeName = 'HighSeverityEvents' THEN DC.AttributeValue
																			END) AS 'HighSeverityEvents',
																			MAX(CASE
																				WHEN A.AttributeName = 'DailyLocationReportingFrequency' THEN DC.AttributeValue
																			END) AS 'DailyLocationReportingFrequency',
																			CAST(MAX(CASE
																					WHEN A.AttributeName = 'DailyReportingTime' THEN DC.AttributeValue
																				END)
																				AS TIME) AS 'DailyReportingTime',
																			MAX(CASE
																				WHEN A.AttributeName = 'DiagnosticReportFrequency' THEN DC.AttributeValue
																			END) AS 'DiagnosticReportFrequency',
																			MAX(CASE
																				WHEN A.AttributeName = 'HourMeterFuelReport' THEN DC.AttributeValue
																			END) AS 'HourMeterFuelReport',
																			MAX(CASE
																				WHEN A.AttributeName = 'LowSeverityEvents' THEN DC.AttributeValue
																			END) AS 'LowSeverityEvents',
																			MAX(CASE
																				WHEN A.AttributeName = 'MaintenanceModeDuration' THEN DC.AttributeValue
																			END) AS 'MaintenanceModeDuration',
																			MAX(CASE
																				WHEN A.AttributeName = 'MediumSeverityEvents' THEN DC.AttributeValue
																			END) AS 'MediumSeverityEvents',
																			CAST(IFNULL(MAX(CASE
																					WHEN A.AttributeName = 'MovingOrStoppedThreshold' THEN DC.AttributeValue
																				END), @MovingOrStoppedThreshold)
																				AS DECIMAL(20,17)) AS 'MovingOrStoppedThreshold',
																			CAST(IFNULL(MAX(CASE
																					WHEN A.AttributeName = 'MovingThresholdsDuration' THEN DC.AttributeValue
																				END), @MovingThresholdsDuration)
																				AS UNSIGNED) AS 'MovingThresholdsDuration',
																			CAST(IFNULL(MAX(CASE
																					WHEN A.AttributeName = 'MovingThresholdsRadius' THEN DC.AttributeValue
																				END), @MovingThresholdsRadius)
																				AS DECIMAL) AS 'MovingThresholdsRadius',
																			CAST(MAX(CASE
																					WHEN A.AttributeName = 'NextSentEventInHours' THEN DC.AttributeValue
																				END)
																				AS UNSIGNED) AS 'NextSentEventInHours',
																			MAX(CASE
																				WHEN A.AttributeName = 'ReportAssetStartStop' THEN DC.AttributeValue
																			END) AS 'ReportAssetStartStop',
																			MAX(CASE
																				WHEN A.AttributeName = 'SentUTC' THEN DC.AttributeValue
																			END) AS 'SentUTC',
																			MAX(CASE
																				WHEN A.AttributeName = 'SMHOdometerConfig' THEN DC.AttributeValue
																			END) AS 'SMHOdometerConfig',
																			MAX(CASE
																				WHEN A.AttributeName = 'SpeedThreshold' THEN DC.AttributeValue
																			END) AS 'SpeedThreshold',
																			MAX(CASE
																				WHEN A.AttributeName = 'SpeedThresholdDuration' THEN DC.AttributeValue
																			END) AS 'SpeedThresholdDuration',
																			MAX(CASE
																				WHEN A.AttributeName = 'SpeedThresholdEnabled' THEN DC.AttributeValue
																			END) AS 'SpeedThresholdEnabled',
																			MAX(CASE
																				WHEN A.AttributeName = 'StartTime' THEN DC.AttributeValue
																			END) AS 'StartTime',
																			MAX(CASE
																				WHEN A.AttributeName = 'Status' THEN DC.AttributeValue
																			END) AS 'Status',
																			MAX(CASE
																				WHEN A.AttributeName = 'SecurityMode' THEN DC.AttributeValue
																			END) AS 'SecurityMode',
																			MAX(CASE
																				WHEN A.AttributeName = 'SecurityStatus' THEN DC.AttributeValue
																			END) AS 'SecurityStatus',
																			COUNT(DISTINCT CASE
																            WHEN
																	            (((A.AttributeName = 'SwitchEnabled'
																		            AND DC.AttributeValue = 'true')
																		            OR (A.AttributeName = 'SwitchActiveState'
																		            AND (DC.AttributeValue != 'NotConfigured' AND (DC.AttributeValue <> '' OR  DC.AttributeValue <> null)))) AND (DTP.DefaultValueJson NOT LIKE '%""isTampered"":1%'))
																            THEN
																	            DC.fk_DeviceTypeParameterID
															                END) AS 'ConfiguredSwitches',
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'ReportingSchedule'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS ReportingSchedulePendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'MovingThresholds'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS MovingThresholdPendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'MaintenanceMode'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS MaintenanceModePendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'FaultCodeReporting'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS FaultCodeReportingPendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'Switches'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS SwitchesPendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'SpeedingThresholds'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS SpeedingThresholdPendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'Meters'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS MetersPendingUpdatedOn,
																			MAX(CASE
																				WHEN
																					DPG.GroupName = 'AssetSecurity'
																						AND (DC.LastAttrEventUTC IS NULL
																						OR DC.FutureAttrEventUTC > DC.LastAttrEventUTC)
																				THEN
																					DC.FutureAttrEventUTC
																			END) AS AssetSecurityPendingUpdatedOn
																	FROM
																		md_customer_CustomerUser AS uc
																	INNER JOIN md_customer_CustomerAsset AS ac ON uc.fk_CustomerUID = ac.fk_CustomerUID
																		AND uc.fk_CustomerUID = UNHEX(@CustomerUID)
																		AND uc.fk_UserUID = UNHEX(@UserUid)
																	INNER JOIN md_asset_Asset AS a ON (ac.fk_AssetUID) = (a.AssetUID)
																		AND a.StatusInd = @StatusInd
																	INNER JOIN md_asset_AssetDevice AS ad ON (a.AssetUID) = (ad.fk_AssetUID)
																	INNER JOIN md_device_Device d ON (ad.fk_DeviceUID) = (d.DeviceUID)
																	INNER JOIN md_device_DeviceType as DT ON (DT.DeviceTypeID = d.fk_DeviceTypeID) AND (@DeviceType IS null OR DT.TypeName = @DeviceType)
																	LEFT JOIN md_device_DeviceConfig DC ON d.DeviceUID = DC.fk_DeviceUID
																	LEFT JOIN md_device_DeviceParamAttr DPA ON DPA.DeviceParamAttrID = DC.fk_DeviceParamAttrID
																	LEFT JOIN md_device_Attribute A ON A.AttributeId = DPA.fk_AttributeId
																	LEFT JOIN md_device_DeviceTypeParameter DTP ON DTP.DeviceTypeParameterID = DC.fk_DeviceTypeParameterID AND DTP.fk_DeviceTypeID = DT.DeviceTypeID
																	LEFT JOIN md_device_DeviceParameter DP ON DP.DeviceParameterID = DTP.fk_DeviceParameterID
																	LEFT JOIN md_device_DeviceParamGroupParameter DPGP ON DPGP.fk_DeviceParameterID = DP.DeviceParameterID
																	LEFT JOIN md_device_DeviceParamGroup DPG ON DPG.DeviceParamGroupID = DPGP.fk_DeviceParamGroupID
																	GROUP BY a.AssetUID , d.DeviceUID) ast
																	JOIN md_asset_Asset AS a ON (ast.AssetUID) = (a.AssetUID)
																	JOIN md_device_Device d ON (ast.DeviceUID) = (d.DeviceUID)
																	LEFT JOIN msg_md_assetsmu_Latestsmu hMeter ON (hMeter.AssetUID) = (a.AssetUID)
																		AND (hMeter.RuntimeHours IS NOT NULL)
																	LEFT JOIN msg_md_assetsmu_Latestsmu oMeter ON (oMeter.AssetUID) = (a.AssetUID)
																		AND (oMeter.OdometerKilometers IS NOT NULL)
																	WHERE 
																		1=1 
																		{2}) AS AssetDeviceConfigurations
                                                                            {1} -- ORDER BY
                                                                            {0}; ";
		/*,
																			
		 * */
		//add to above query
		public const string SelectFoundRows = "SELECT found_rows();";
        public const string OrderByClause = "ORDER BY {0} {1}";
        public const string LimitClause = "LIMIT {0}, {1}";



        public const string FetchAssetUIdsWithUserCustomerAndAssets = @"SELECT 
                                                                                HEX(CA.fk_AssetUID)
                                                                            FROM
                                                                                md_customer_CustomerAsset AS CA
																			INNER JOIN
																				md_customer_CustomerUser AS CU ON CA.fk_CustomerUID = CU.fk_CustomerUID
                                                                            WHERE
                                                                                CA.fk_AssetUID in ({0}) 
                                                                                AND CA.fk_CustomerUID = UNHEX(@CustomerUid) 
                                                                                AND CU.fk_UserUID = UNHEX(@UserUid)";

        public const string FetchDeviceTypesForCustomerAndUser = @"SELECT TypeName AS DeviceType,
                                                                          COUNT(TypeName) AS AssetCount
                                                                            FROM
                                                                                md_customer_CustomerAsset AS ca
                                                                                    INNER JOIN
																				md_customer_CustomerUser AS cu on cu.fk_CustomerUID = ca.fk_CustomerUID
                                                                                    INNER JOIN
                                                                                md_asset_AssetDevice AS ad ON(ca.fk_AssetUID) = (ad.fk_AssetUID)
                                                                                    INNER JOIN
                                                                                md_device_Device d ON(ad.fk_DeviceUID) = (d.DeviceUID)
																					Inner Join 
																				md_device_DeviceType dt ON dt.DeviceTypeID = d.fk_DeviceTypeID
                                                                            WHERE
                                                                                ca.fk_CustomerUID = UNHEX(@CustomerGuid)
                                                                                AND cu.fk_UserUID = UNHEX(@UserGuid)
                                                                                {3}
                                                                                {0}
                                                                                GROUP BY TypeName
                                                                                {1};
                                                                                {2}";

        public const string FetchAssetUIDsForSubAccountCustomerUID = @"SELECT 
                                                                            ac.fk_AssetUID
                                                                        FROM
                                                                            md_customer_CustomerAsset ac
                                                                                INNER JOIN
                                                                            md_asset_AssetDevice ad ON ac.fk_AssetUID = ad.fk_AssetUID
                                                                                INNER JOIN
                                                                            md_device_Device d ON ad.fk_DeviceUID = d.DeviceUID
																				Inner Join 
																			md_device_DeviceType dt ON dt.DeviceTypeID = d.fk_DeviceTypeID
                                                                        WHERE
                                                                            fk_CustomerUID = UNHEX(@SubAccountCustomerUid)
                                                                            AND (@DeviceType IS NULL OR dt.TypeName = @DeviceType)
                                                                        ORDER BY TypeName";

        public const string FetchDeviceTypesForCustomerAndUserAndSwitchesGroup = @"SELECT TypeName AS DeviceType, 
                                                                                          COUNT(TypeName) AS AssetCount
                                                                                    FROM
                                                                                        md_customer_CustomerAsset AS ca
																							INNER JOIN
																						md_customer_CustomerUser AS cu on cu.fk_CustomerUID = ca.fk_CustomerUID
                                                                                            INNER JOIN
                                                                                        md_asset_AssetDevice AS ad ON(ca.fk_AssetUID) = (ad.fk_AssetUID)
                                                                                            INNER JOIN
                                                                                        md_device_Device d ON(ad.fk_DeviceUID) = (d.DeviceUID)
                                                                                    Inner Join md_device_DeviceType dt ON dt.DeviceTypeID = d.fk_DeviceTypeID
                                                                                    INNER JOIN md_device_DeviceTypeParameter dtp on dtp.fk_DeviceTypeID = dt.DeviceTypeID
                                                                                    INNER JOIN md_device_DeviceParameter dp ON dp.DeviceParameterID = dtp.fk_DeviceParameterID
                                                                                    INNER JOIN md_device_DeviceParamGroupParameter dpgp On dpgp.fk_DeviceParameterID = dp.DeviceParameterID
                                                                                    INNER JOIN md_device_DeviceParamGroup dpg On dpg.DeviceParamGroupID = dpgp.fk_DeviceParamGroupID
                                                                                    WHERE
                                                                                        ca.fk_CustomerUID = UNHEX(@CustomerGuid)
                                                                                        AND cu.fk_UserUID = UNHEX(@UserGuid)
                                                                                        {3}
                                                                                        AND dpg.GroupName = 'Switches'
                                                                                        {0}
                                                                                    GROUP BY TypeName
                                                                                        {1};
                                                                                        {2}";




		#region Subscription Validation



		public const string FETCH_SUBSCRIPTION_SERVICETYPE = @"SELECT 
																fk_ServiceTypeID AS ServiceTypeID,
																HEX(AssetSubscriptionUID) AS ServiceUIDString,
																HEX(fk_AssetUID) AS AssetUIDString,
																HEX(fk_CustomerUID) AS CustomerUIDString,
																HEX(fk_DeviceUID) AS DeviceUIDString,
																fk_SubscriptionSourceID AS SubscriptionSourceID,
																StartDate,
																EndDate
															FROM
																md_subscription_AssetSubscription as SSP 
															WHERE 
																fk_AssetUID IN ({0}) and EndDate >= @endDate";

		#endregion
	}
}
