namespace DeviceConfigRepository.MySql
{
    public class Queries
    {

        #region User Asset

        //Remove the Commented joins when Asset table is available
        public const string FetchValidAssetUIDs = @"SELECT 
                                                        hex(ca.fk_AssetUID)
                                                    FROM
                                                        md_asset_AssetDevice as ad
                                                            INNER JOIN 
                                                        md_device_Device as d on ad.fk_DeviceUID = d.DeviceUID
                                                            INNER JOIN
                                                        md_device_DeviceType as dt on dt.DeviceTypeID = d.fk_DeviceTypeID
                                                            INNER JOIN
                                                        md_customer_CustomerAsset as ca on ad.fk_AssetUID = ca.fk_AssetUID
                                                            INNER JOIN
                                                        md_customer_CustomerUser as cu ON ca.fk_CustomerUID = cu.fk_CustomerUID
                                                        --     INNER JOIN
                                                        -- Asset as a ON ca.fk_AssetUID = a.AssetUID
                                                    WHERE
                                                        ad.fk_AssetUID IN ({0}) 
                                                            AND dt.TypeName = @TypeName
                                                            AND ca.fk_CustomerUID = unhex(@CustomerUIDString)
                                                            AND cu.fk_UserUID = unhex(@UserUIDString)
                                                        --     AND a.StatusInd = 1;";

        #endregion

        #region Related To Asset Device

        public const string FetchAssetDeviceWithAssetUID = @"Select DISTINCT DT.TypeName As DeviceType, HEX(AD.fk_AssetUID) As AssetID, HEX(AD.fk_DeviceUID) As DeviceID, D.ModuleType From md_asset_AssetDevice AD
                                                            Inner Join md_device_Device D On AD.fk_DeviceUID = D.DeviceUID
                                                            INNER JOIN md_device_DeviceType DT ON DT.DeviceTypeID = D.fk_DeviceTypeID
                                                            WHERE AD.fk_AssetUID IN ({0})";

        public const string FetchAssetDeviceWithDeviceUID = @"Select DISTINCT DT.TypeName As md_device_DeviceType, HEX(AD.fk_AssetUID) As AssetID, HEX(AD.fk_DeviceUID) As DeviceID, D.ModuleType From md_asset_AssetDevice AD
                                                            Inner Join md_device_Device D On AD.fk_DeviceUID = D.DeviceUID
                                                            INNER JOIN md_device_DeviceType DT ON DT.DeviceTypeID = D.fk_DeviceTypeID
                                                            WHERE AD.fk_DeviceUID IN ({0})";

        public const string FetchAssetDeviceWithAssetUIDAndDeviceType = @"Select DISTINCT DT.TypeName As DeviceType, HEX(AD.fk_AssetUID) As AssetID, HEX(AD.fk_DeviceUID) As DeviceID, D.ModuleType From md_asset_AssetDevice AD
                                                            Inner Join md_device_Device D On AD.fk_DeviceUID = D.DeviceUID
                                                            INNER JOIN md_device_DeviceType DT ON DT.DeviceTypeID = D.fk_DeviceTypeID
                                                            WHERE AD.fk_AssetUID IN ({0})
                                                            AND DT.TypeName = '{1}'";

        #endregion

        #region Related to md_device_Device Config

        public const string FetchDeviceParameterGroups = @"SELECT
                                                            dpg.DeviceParamGroupId AS Id, dpg.GroupName AS Name, dt.TypeName as TypeName, dpg.IsMultiDeviceTypeSupport as IsMultiDeviceTypeSupport, dpg.IsDeviceParamGroup as IsDeviceParamGroup
                                                        FROM
                                                            md_device_DeviceParamGroup AS dpg
                                                                INNER JOIN
                                                            md_device_DeviceTypeParamGroup AS dtpg ON dpg.DeviceParamGroupId = dtpg.fk_DeviceParamGroupId
                                                                INNER JOIN
                                                            md_device_DeviceType AS dt ON dt.DeviceTypeID = dtpg.fk_DeviceTypeID
                                                        WHERE
                                                            dt.TypeName = @TypeName";

        public const string FetchDeviceParameters = @"SELECT
                                                        dp.DeviceParameterID AS Id, 
                                                        dp.ParameterName AS Name
                                                    FROM
                                                        md_device_DeviceParamGroupParameter AS dpgp
                                                            INNER JOIN
                                                        md_device_DeviceTypeParameter AS dtp ON dpgp.fk_DeviceParameterID = dtp.fk_DeviceParameterID
                                                            INNER JOIN
                                                        md_device_DeviceParameter dp ON dtp.fk_DeviceParameterID = dp.DeviceParameterID
                                                            INNER JOIN
                                                        md_device_DeviceType AS dt ON dt.DeviceTypeID = dtp.fk_DeviceTypeID
                                                    WHERE
                                                        dpgp.fk_DeviceParamGroupID = @ParameterGroupId
                                                            AND dt.TypeName = @TypeName";

        public const string FetchDeviceParametersByDeviceTypeId = @"SELECT dp.DeviceParameterID AS Id,
                                                                           dp.ParameterName     AS Name,
                                                                           dpg.GroupName        AS ParameterGroupName
                                                                   FROM md_device_DeviceParamGroupParameter    AS dpgp
                                                                           INNER JOIN md_device_DeviceTypeParameter AS dtp
                                                                           ON dpgp.fk_DeviceParameterID = dtp.fk_DeviceParameterID
                                                                           INNER JOIN md_device_DeviceParameter dp
                                                                           ON dtp.fk_DeviceParameterID = dp.DeviceParameterID
                                                                           INNER JOIN md_device_DeviceType AS dt ON dt.DeviceTypeID = dtp.fk_DeviceTypeID
                                                                           INNER JOIN md_device_DeviceParamGroup dpg
                                                                           ON dpg.DeviceParamGroupID = dpgp.fk_DeviceParamGroupID
                                                                   WHERE dt.TypeName = @TypeName
                                                                   UNION
                                                                   SELECT 0 AS Id, GroupName AS ParameterName, GroupName AS GroupName
                                                                   FROM md_device_DeviceParamGroup
                                                                       AS dpg
                                                                       INNER JOIN
                                                                   md_device_DeviceTypeParamGroup AS dtpg ON dpg.DeviceParamGroupId = dtpg.fk_DeviceParamGroupId
                                                                       INNER JOIN
                                                                   md_device_DeviceType AS dt1 ON dt1.DeviceTypeID = dtpg.fk_DeviceTypeID
                                                                   WHERE     IsDeviceParamGroup = 0 AND dt1.TypeName = @TypeName";

        public const string FetchDeviceParametersWithAttributes = @"SELECT 
                                                                    dp.DeviceParameterID AS Id, dp.ParameterName AS Name,
                                                                    A.AttributeName,
                                                                    A.AttributeId
                                                                FROM
                                                                    md_device_DeviceParamGroupParameter AS dpgp
                                                                        INNER JOIN
                                                                    md_device_DeviceTypeParameter AS dtp ON dpgp.fk_DeviceParameterID = dtp.fk_DeviceParameterID
                                                                        INNER JOIN
                                                                    md_device_DeviceParameter dp ON dtp.fk_DeviceParameterID = dp.DeviceParameterID
                                                                        INNER JOIN
                                                                    md_device_DeviceType AS dt ON dt.DeviceTypeID = dtp.fk_DeviceTypeID
		                                                                INNER JOIN 
                                                                    md_device_DeviceParamAttr AS DPA ON DPA.fk_DeviceParameterID = dp.DeviceParameterID
		                                                                INNER JOIN
                                                                    md_device_Attribute AS A ON A.AttributeID = DPA.fk_AttributeID
                                                    WHERE
                                                        dpgp.fk_DeviceParamGroupID = @ParameterGroupId
                                                            AND dt.TypeName = @TypeName";

        public const string SELECT_DEVICE_BY_MODULECODE = "SELECT HEX(DeviceUID) as DeviceUID, SerialNumber FROM md_device_Device WHERE SerialNumber = @SerialNumber";

        public const string FetchDeviceTypeByTypeName = @"SELECT
                                                            dt.TypeName
                                                        FROM
                                                            md_device_DeviceType AS dt
                                                        WHERE
                                                            dt.TypeName = @TypeName";

        public const string FetchAllDeviceTypes = @"SELECT
                                                            dt.TypeName
                                                        FROM
                                                            md_device_DeviceType AS dt";

        public const string FetchParameterGroupById = @"SELECT
                                                            dpg.DeviceParamGroupID
                                                        FROM
                                                            md_device_DeviceParamGroup AS dpg
                                                        WHERE
                                                            DeviceParamGroupID = @Id";

        public const string FetchAllParameterGroups = @"SELECT
                                                            dpg.DeviceParamGroupID AS ID , dpg.GroupName AS Name
                                                        FROM
                                                            md_device_DeviceParamGroup AS dpg";

        public const string FetchDeviceTypesGroupsParametersAttributes = @"SELECT DISTINCT
                                                                            A.AttributeID,
                                                                            A.AttributeName,
                                                                            DP.DeviceParameterID,
                                                                            DP.ParameterName,
                                                                            DPG.DeviceParamGroupID,
                                                                            DPG.GroupName,
                                                                            DT.TypeName,
                                                                            DTP.DeviceTypeParameterID,
                                                                            DPA.DeviceParamAttrID,
                                                                            DTP.DefaultValueJSON As DefaultValueJSON,
																			DTP.IncludeInd
                                                                        FROM
                                                                            md_device_DeviceType AS DT
                                                                                INNER JOIN
                                                                            md_device_DeviceTypeParameter AS DTP ON DTP.fk_DeviceTypeID = DT.DeviceTypeID
                                                                                INNER JOIN
                                                                            md_device_DeviceParameter DP ON DTP.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamAttr AS DPA ON DPA.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_Attribute AS A ON A.AttributeID = DPA.fk_AttributeID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamGroupParameter AS DPGP ON DPGP.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_DeviceTypeParamGroup AS DTGP ON DTGP.fk_DeviceParamGroupID = DPGP.fk_DeviceParamGroupID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamGroup AS DPG ON DPG.DeviceParamGroupID = DPGP.fk_DeviceParamGroupID LIMIT 2000;";

        public const string SELECT_DEVICETYPEPARAMETERATTRIBUTE_FOR_A_DEVICE = @"Select DC.LastAttrEventUTC, DC.DeviceConfigID, DTP.DeviceTypeParameterID, DPA.DeviceParamAttrID from md_device_DeviceTypeParameter DTP 
                                                                                INNER JOIN md_device_DeviceParameter DP ON DP.DeviceParameterID = DTP.fk_DeviceParameterID 
                                                                                INNER JOIN md_device_DeviceParamAttr DPA ON DPA.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN md_device_Attribute A ON A.AttributeID = DPA.fk_AttributeID
                                                                                INNER JOIN md_device_DeviceConfig DC ON DC.fk_DeviceTypeParameterID = DTP.DeviceTypeParameterID AND DC.fk_DeviceParamAttrID = DPA.DeviceParamAttrID
                                                                                WHERE DTP.fk_DeviceTypeID = {0} AND DP.ParameterName = '{1}' AND A.AttributeName = '{2}' AND DC.fk_DeviceUID = unhex('{3}')";

        public const string SELECT_DEVICETYPEPARAMETERATTRIBUTE = @"Select DC.DeviceConfigID, DTP.DeviceTypeParameterID, DPA.DeviceParamAttrID, DC.FutureAttrEventUTC, DC.LastAttrEventUTC from md_device_DeviceTypeParameter DTP 
                                                                    INNER JOIN md_device_DeviceParameter DP ON DP.DeviceParameterID = DTP.fk_DeviceParameterID 
                                                                    INNER JOIN md_device_DeviceParamAttr DPA ON DPA.fk_DeviceParameterID = DP.DeviceParameterID
                                                                    INNER JOIN md_device_Attribute A ON A.AttributeID = DPA.fk_AttributeID
                                                                    INNER JOIN md_device_DeviceConfig DC ON DC.fk_DeviceTypeParameterID = DTP.DeviceTypeParameterID AND DC.fk_DeviceParamAttrID = DPA.DeviceParamAttrID
                                                                    WHERE DTP.fk_DeviceTypeID = {0} AND DP.ParameterName = '{1}' AND A.AttributeName = '{2}';";
        #endregion

        #region Device Config



        public const string FetchDeviceConfig = @"SELECT
                                                    DC.DeviceConfigID AS DeviceConfigID,
                                                    HEX(DC.fk_DeviceUID) AS DeviceUIDString,
                                                    HEX(AD.fk_AssetUID) AS AssetUIDString,
                                                    DC.fk_DeviceTypeParameterID AS DeviceTypeParameterID,
                                                    DC.fk_DeviceParamAttrID AS DeviceParameterAttributeId,
                                                    DC.AttributeValue AS CurrentAttributeValue,
													DC.FutureAttributeValue AS FutureAttributeValue,
                                                    DC.InsertUTC AS RowInsertedUTC,
													DC.UpdateUTC AS RowUpdatedUTC,
													DC.FutureAttrEventUTC AS FutureAttrEventUTC,
													DC.LastAttrEventUTC AS LastAttrEventUTC,
                                                    A.AttributeName AS AttributeName,
                                                    DP.ParameterName AS ParameterName
                                                FROM
                                                    md_device_DeviceConfig AS DC
                                                        INNER JOIN
                                                    md_asset_AssetDevice AS AD ON DC.fk_DeviceUID = AD.fk_DeviceUID
                                                        INNER JOIN
                                                    md_device_DeviceParamAttr DPA ON DPA.DeviceParamAttrID = DC.fk_DeviceParamAttrID
                                                        INNER JOIN
                                                    md_device_Attribute A ON A.AttributeID = DPA.fk_AttributeID
                                                        INNER JOIN
                                                    md_device_DeviceParameter DP ON DP.DeviceParameterID = DPA.fk_DeviceParameterID
                                                WHERE
                                                    AD.fk_AssetUID IN ({0})
                                                        AND DC.fk_DeviceTypeParameterID IN @DeviceTypeParameterIDs
                                                        AND DC.fk_DeviceParamAttrID IN @DeviceParamAttrIDs;";

        public const string InsertDeviceConfig = @"INSERT INTO md_device_DeviceConfig(fk_DeviceUID, fk_DeviceTypeParameterID, fk_DeviceParamAttrID, FutureAttributeValue, FutureAttrEventUTC, InsertUTC, UpdateUTC)
                                                    SELECT
                                                        (SELECT
                                                                fk_DeviceUID
                                                            FROM
                                                                md_asset_AssetDevice AS AD INNER JOIN
                                                                md_device_Device AS D ON AD.fk_DeviceUID = D.DeviceUID INNER JOIN
                                                                md_device_DeviceTypeParameter AS DTP ON DTP.fk_DeviceTypeId = D.fk_DeviceTypeId
                                                            WHERE
                                                                AD.fk_AssetUID = UNHEX('{0}') AND DTP.DeviceTypeParameterId = @DeviceTypeParameterID),
                                                        @DeviceTypeParameterID,
                                                        @DeviceParamAttrID,
                                                        @FutureAttributeValue,
                                                        @FutureAttrEventUTC,
                                                        @InsertUTC,
                                                        @UpdateUTC;";

        public const string UpdateDeviceConfig = @"UPDATE md_device_DeviceConfig
                                                    SET
                                                        FutureAttributeValue = @FutureAttributeValue,
                                                        FutureAttrEventUTC = @FutureAttrEventUTC,
                                                        UpdateUTC = @UpdateUTC
                                                    WHERE
                                                        DeviceConfigId = @DeviceConfigID;";

        public const string UPDATE_DEVICECONFIG_FORATTRIBUTEVALUE = @"UPDATE md_device_DeviceConfig
                                                                    SET
                                                                        AttributeValue = @AttributeValue,
                                                                        UpdateUTC = @UpdateUTC,
                                                                        LastAttrEventUTC = @LastAttrEventUTC
                                                                    WHERE
                                                                        DeviceConfigId = @DeviceConfigID;";

        public const string UPSERT_DEVICE_CONFIG = @"INSERT INTO md_device_DeviceConfig
                                                                (fk_DeviceUID, fk_DeviceTypeParameterID, fk_DeviceParamAttrID, AttributeValue, FutureAttributeValue, InsertUTC, UpdateUTC, LastAttrEventUTC, FutureAttrEventUTC)
                                                     VALUES (UNHEX(@fk_DeviceUID), @fk_DeviceTypeParameterID, @fk_DeviceParamAttrID, @AttributeValue, @FutureAttributeValue, @InsertUTC, @UpdateUTC, @LastAttrEventUTC, @FutureAttrEventUTC)
                                                     ON DUPLICATE KEY UPDATE AttributeValue = @AttributeValue, LastAttrEventUTC = @LastAttrEventUTC, UpdateUTC = @UpdateUTC;";

        public const string SELECT_DEVICECONFIG_FORPARAMETERIDS = @"";

        public const string FETCH_MAINTENANCE_MODE_ON_DEVICES_RECORD = @"SELECT DC1.AttributeValue, DC1.DeviceConfigID, DC1.LastAttrEventUTC, HEX(DC1.fk_DeviceUID) as DeviceUIDString, DP.ParameterName, HEX(AD.fk_AssetUID) as AssetUIDString, DT.TypeName as DeviceTypeName
                                                                        from md_device_DeviceConfig DC1  
                                                                        JOIN md_device_DeviceParamAttr DPA on DC1.fk_DeviceParamAttrID = DPA.DeviceParamAttrID
                                                                        JOIN md_device_DeviceParameter DP on DPA.fk_DeviceParameterID = DP.DeviceParameterID
                                                                        JOIN md_asset_AssetDevice AD ON DC1.fk_DeviceUID = AD.fk_DeviceUID
                                                                        JOIN md_device_Device D ON DC1.fk_DeviceUID = D.DeviceUID
                                                                        JOIN md_device_DeviceType DT on D.fk_DeviceTypeID = DT.DeviceTypeID
                                                                        WHERE DP.ParameterName in @ParamsList  AND DC1.LastAttrEventUTC != '' AND DC1.LastAttrEventUTC IS NOT NULL AND DC1.LastAttrEventUTC <= now()";

        public const string CHECK_EXISTING_DEVICE_PROPERTIES_QUERY = "select hex({0}) as DeviceUID, {1} as DeviceSerialNumber, {2} as DeviceType, {3} as DeviceState, {4} as DeregisteredUTC, {5} as  ModuleType, {6} as MainboardSoftwareVersion," +
                                                            "{7} as FirmwarePartNumber, {8} as GatewayFirmwarePartNumber ,{9} as DataLinkType ,{10} as CellModemIMEI, {11} as DevicePartNumber, {12} as CellularFirmwarePartnumber, {13} as NetworkFirmwarePartnumber," +
                                                            "{14} as SatelliteFirmwarePartnumber from {15} where {16}={17}";

        public const string CheckForExistingDeviceQuery = "select hex({0}) as DeviceUID, {1} as DeviceSerialNumber, {2} as DeviceType, {3} as DeviceState, {4} as DeregisteredUTC, {5} as  ModuleType, " +
                                                            "{6} as DataLinkType " +
                                                            "from {7} where {8}={9}";
        public static readonly string ReadDeviceTypeByType_Query = "select dt.TypeName,dt.DeviceTypeID,dt.DefaultValueJson,de.fk_DeviceTypeFamilyID from md_device_DeviceType dt where dt.DeviceType={0}";

        #endregion


        #region Asset Security History

        public const string INSERT_ASSET_SECURITY_HISTORY_SECURITYMODE = @"INSERT INTO AssetSecurityHist(fk_AssetUID, SecurityModeID, fk_UserUID, StatusUpdateUTC, RowUpdatedUTC) VALUES(UNHEX(@AssetUIDString), @SecurityModeId, UNHEX(@UserUIDString), @StatusUpdateUTC, @RowUpdatedUTC);";
        public const string INSERT_ASSET_SECURITY_HISTORY_SECURITYSTATUS = @"INSERT INTO AssetSecurityHist(fk_AssetUID, SecurityStatusID, fk_UserUID, StatusUpdateUTC, RowUpdatedUTC) VALUES(UNHEX(@AssetUIDString), @SecurityStatusId, UNHEX(@UserUIDString), @StatusUpdateUTC, @RowUpdatedUTC);";
        public const string UPDATE_ASSET_SECURITY_HISTORY_SECURITYMODE = @"UPDATE AssetSecurityHist SET SecurityModeID = @SecurityModeId, RowUpdatedUTC = @RowUpdatedUTC WHERE AssetSecurityHistID = @AssetSecurityHistID;";
        public const string UPDATE_ASSET_SECURITY_HISTORY_SECURITYSTATUS = @"UPDATE AssetSecurityHist SET SecurityStatusId = @SecurityStatusId, RowUpdatedUTC = @RowUpdatedUTC WHERE AssetSecurityHistID = @AssetSecurityHistID;";
        public const string SELECT_ASSET_SECURITY_HISTORY = @"SELECT AssetSecurityHistID, HEX(fk_AssetUID) as AssetUIDString, SecurityStatusID, SecurityModeID, HEX(fk_UserUID) as UserUIDString, StatusUpdateUTC, RowUpdatedUTC FROM AssetSecurityHist WHERE fk_AssetUID = UNHEX(@AssetUIDString) AND fk_UserUID = UNHEX(@UserUIDString) AND StatusUpdateUTC = @StatusUpdateUTC;";
        #endregion

        #region DeviceConfigMessage

        public const string SELECT_DEVICECONFIGMESSAGE = @"Select DeviceConfigMessageID, hex(DeviceConfigMessageUID) as DeviceConfigMessageUIDString, hex(fk_DeviceUID) as DeviceUIDString, hex(fk_UserUID) as UserUIDString, fk_DeviceTypeID as DeviceTypeID, EventUTC, MessageContent, fk_StatusID as StatusID, LastMessageUTC from DeviceConfigMessage where DeviceConfigMessageUID = unhex('{0}')";


        public const string UPDATE_DEVICECONFIGMESSAGE_STATUS = @"Update DeviceConfigMessage Set  fk_StatusID = 1 where DeviceConfigMessageUID = unhex('{0}')";

        public const string INSERT_DEVICECONFIGMESSAGE = @"INSERT INTO DeviceConfigMessage(DeviceConfigMessageUID, fk_DeviceUID, fk_DeviceTypeID, EventUTC, MessageContent, fk_StatusID, LastMessageUTC)
VALUES(UNHEX('{0}'), UNHEX('{1}'),  @fk_DeviceTypeID, @EventUTC, @MessageContent, @fk_StatusID, @LastMessageUTC);";

        public const string INSERT_LOGINUSER = @"INSERT INTO LoginUser(UserUID, EmailID, FirstName, LastName, StatusInd, LastUserUTC) VALUES(UNHEX(@UserUIDString), @EmailID, @FirstName, @LastName, @StatusInd, @LastUserUTC);";
        public const string UPDATE_LOGINUSER = @"UPDATE LoginUser SET EmailID = @EmailID, FirstName = @FirstName, LastName = @LastName, StatusInd = @StatusInd, LastUserUTC = @LastUserUTC WHERE UserUID = UNHEX(@UserUIDString);";
        public const string SELECT_LOGINUSER_BY_USERUID = @"SELECT HEX(UserUID) AS UserUIDString, EmailID, FirstName, LastName, StatusInd, LastUserUTC FROM LoginUser WHERE UserUID = UNHEX(@UserUIDString);";

        #endregion

        #region DevicePingAckMessage

        public const string SELECT_DEVICEPINGACKMESSAGE = @"SELECT 
                                                                HEX(DevicePingACKMessageUID) AS DevicePingACKMessageUID,
                                                                HEX(fk_DevicePingLogUID) AS DevicePingLogUID,
                                                                HEX(fk_DeviceUID) AS DeviceUID,
                                                                HEX(fk_AssetUID) AS AssetUID,
                                                                fk_AcknowledgeStatusID As AcknowledgeStatusID,
                                                                AcknowledgeTimeUTC,
                                                                RowUpdatedUTC
                                                            FROM md_device_DevicePingACKMessage 
                                                            WHERE DevicePingACKMessageUID  = UNHEX('{0}')";


        public const string SELECT_DEVICEPINGLOGBYMESSAGEID = @"Select HEX(dpl.fk_DeviceUID) As DeviceUID, 
	                                                                HEX(dpl.fk_AssetUID) As AssetUID, 
	                                                                dpl.fk_RequestStatusID As RequestStatusID, 
	                                                                HEX(dpl.DevicePingLogUID) As DevicePingLogUID, 
	                                                                RequestTimeUTC, 
	                                                                RequestExpiryTimeUTC
                                                                From md_device_DevicePingACKMessage dam
                                                                Inner Join md_device_DevicePingLog dpl on dam.fk_DevicePingLogUID = dpl.DevicePingLogUID
                                                                Where dam.fk_DevicePingLogUID = UNHEX('{0}')
                                                                And dam.DevicePingACKMessageUID <> UNHEX('{1}')
                                                                And fk_AcknowledgeStatusID = @PendingStatusID;";

        public const string UPDATE_DEVICEPINGACKMESSAGE_STATUS = @"UPDATE md_device_DevicePingACKMessage 
                                                                    SET  fk_AcknowledgeStatusID = @AcknowledgeStatusID,
                                                                        AcknowledgeTimeUTC = @AcknowledgeTimeUTC, 
                                                                        RowUpdatedUTC = @RowUpdatedUTC
                                                                    WHERE DevicePingACKMessageUID = UNHEX('{0}') And fk_AcknowledgeStatusID=@FilterStatus";

        public const string INSERT_DEVICEPINGACKMESSAGE = @"INSERT INTO md_device_DevicePingACKMessage(DevicePingACKMessageUID, fk_DevicePingLogUID, fk_DeviceUID, fk_AssetUID, fk_AcknowledgeStatusID, AcknowledgeTimeUTC, RowUpdatedUTC)
                                                            VALUES(UNHEX('{0}'), UNHEX('{1}'),  UNHEX('{2}'), UNHEX('{3}'), @fk_AcknowledgeStatusID, @AcknowledgeTimeUTC, @RowUpdatedUTC);";


        public const string UPDATE_DEVICEPINGLOG_REQUESTSTATUS = @"UPDATE md_device_DevicePingLog
                                                                    SET
                                                                        fk_RequestStatusID = @RequestStatusID,
                                                                        RowUpdatedUTC = @RowUpdatedUTC
                                                                    WHERE
                                                                        DevicePingLogUID = UNHEX('{0}') And fk_RequestStatusID={1};";
        #endregion

        #region Device Ping Log
        public const string FetchPingRequestStatusQuery = @"Select HEX(dpl.fk_DeviceUID) As DeviceUIDString, 
                                                                HEX(dpl.fk_AssetUID) As AssetUIDString, 
                                                                dpl.fk_RequestStatusID As RequestStatusID, 
                                                                HEX(dpl.DevicePingLogUID) As DevicePingLogUIDString, 
                                                                RequestTimeUTC, 
                                                                RequestExpiryTimeUTC  
                                                          From md_device_DevicePingLog dpl
                                                          Where dpl.fk_AssetUID = {0} And dpl.fk_DeviceUID = {1} Order By RequestTimeUTC Desc Limit 1;";

        public const string InsertPingRequestQuery = @"INSERT INTO md_device_DevicePingLog(DevicePingLogUID, fk_DeviceUID, fk_AssetUID, fk_RequestStatusID, RequestTimeUTC, RequestExpiryTimeUTC, RowUpdatedUTC)
                                                          VALUES({0}, {1}, {2}, @fk_RequestStatusID, @RequestTimeUTC, @RequestExpiryTimeUTC, @RowUpdatedUTC);";

        public const string DeviceTypeFamilyQuery = @"Select FamilyName,  DT.TypeName, DT.DeviceTypeID From md_device_DeviceTypeFamily DTF 
                                                            Join md_device_DeviceType DT on DTF.DeviceTypeFamilyID = DT.fk_DeviceTypeFamilyID
                                                            Join md_device_Device D on DT.DeviceTypeID = D.fk_DeviceTypeID
                                                            Where D.DeviceUID = {0} Limit 1;";




		#endregion

		#region Subscription Validation

		public const string FETCH_ALL_SERVICETYPE_PARAMETER = @"SELECT 
																	DP.DeviceParameterID,
																	STP.fk_ServiceTypeID AS ServiceTypeID,
																	STP.IncludeInd,
																	STP.InsertUTC,
																	STP.UpdateUTC,
																	DP.ParameterName AS DeviceParameterName,
																	DPG.GroupName AS DeviceParamGroupName,
																	STE.Name AS ServiceTypeName,
																	STFE.FamilyName AS ServiceTypeFamilyName
																FROM
																	md_ServiceTypeParameter AS STP
																		INNER JOIN
																	md_device_DeviceParameter AS DP ON DP.DeviceParameterID = STP.fk_DeviceParameterID
																		INNER JOIN
																	md_device_DeviceParamGroupParameter AS DPGP ON DPGP.fk_DeviceParameterID = DP.DeviceParameterID
																		INNER JOIN
																	md_device_DeviceParamGroup AS DPG ON DPG.DeviceParamGroupID = DPGP.fk_DeviceParamGroupId
																		INNER JOIN
																	md_subscription_ServiceType AS STE ON STE.ServiceTypeID = STP.fk_ServiceTypeID
																		INNER JOIN
																	md_subscription_ServiceTypeFamily AS STFE ON STFE.ServiceTypeFamilyID = STE.fk_ServiceTypeFamilyID";


		public const string FETCH_SUBSCRIPTION_SERVICETYPE = @"SELECT 
																SSP.fk_ServiceTypeID AS ServiceTypeID,
																HEX(SSP.AssetSubscriptionUID) AS ServiceUIDString,
																HEX(SSP.fk_AssetUID) AS AssetUIDString,
																HEX(SSP.fk_CustomerUID) AS CustomerUIDString,
																HEX(SSP.fk_DeviceUID) AS DeviceUIDString,
																SSP.fk_SubscriptionSourceID AS SubscriptionSourceID,
																SSTP.Name AS PlanName,
																SSP.InsertUTC AS ActionUTC,
																SSP.UpdateUTC AS RowUpdatedUTC,
																SSP.StartDate,
																SSP.EndDate
															FROM
																md_subscription_AssetSubscription as SSP inner join md_subscription_ServiceType as SSTP on SSP.fk_ServiceTypeID = SSTP.ServiceTypeID
															WHERE 
																fk_AssetUID IN ({0}) and EndDate >= @endDate";


		#endregion



    }
}
