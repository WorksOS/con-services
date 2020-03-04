using CommonModel.DeviceSettings;
using Dapper;
using DbModel.DeviceConfig;
using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace DeviceConfigRepository
{
	public class MessageConstructorDeviceService : IMessageConstructorDeviceService
    {
        //private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _deviceConnectionString;
        private const string Select_DeviceTypeFamily_Query = "select DeviceTypeId, TypeName, FamilyName from md_device_DeviceType  dt inner join md_device_DeviceTypeFamily dtf on dt.fk_DeviceTypeFamilyID = dtf.DeviceTypeFamilyID";
        private const string Select_DeviceTypeID_Query = "select DeviceTypeId,TypeName from md_device_DeviceType";
        private const string Select_DeviceData_Query = @"select hex(Deviceuid) as DeviceUid, 
                                                                SerialNumber, 
                                                                dt.TypeName As DeviceType
                                                                from md_asset_AssetDevice ad 
                                                                inner join md_device_Device d on ad.fk_deviceuid = d.deviceuid 
                                                                inner join md_device_DeviceType dt on d.fk_DeviceTypeID = dt.DeviceTypeID
                                                                where fk_assetuid = {0}";
        private const string Select_DeviceFeature_Support_Query = @"Select 
                                                                    DF.Name As `DeviceFeatureName`, 
                                                                    Case When DTF.DeviceTypeFeatureID is Not Null then 1 else 0 end as `isSupported`
                                                                    From md_asset_AssetDevice AD  
                                                                    Inner Join md_device_Device d On AD.fk_DeviceUID = d.DeviceUID and AD.fk_AssetUID = {0} and AD.fk_DeviceUID = {1}
                                                                    Inner Join md_device_DeviceType DT on d.fk_DeviceTypeID = DT.DeviceTypeID
                                                                    Inner Join md_device_DeviceTypeFeature DTF On DTF.fk_DeviceTypeID = DT.DeviceTypeID
                                                                    Inner Join md_device_DeviceFeature DF On DF.DeviceFeatureID = DTF.fk_DeviceFeatureID;";
        private const string Insert_DeviceCnfigMessage_Query = "INSERT INTO `md_device_DeviceConfigMessage`(`DeviceConfigMessageUID`,`fk_DeviceUID`,`EventUTC`,`MessageContent`,`fk_StatusID`,`LastMessageUTC`,`fk_DeviceTypeID`, `fk_UserUID`)";
        private const string Insert_DeviceAckMessage_Query = @"INSERT INTO 
                                                                `md_device_DevicePingACKMessage`(`DevicePingACKMessageUID`,`fk_DevicePingLogUID`,
                                                                `fk_DeviceUID`,`fk_AssetUID`, `fk_AcknowledgeStatusID`,`RowUpdatedUTC`)
                                                                Values (@DevicePingACKMessageID, @DevicePingLogID, @DeviceID, @AssetID, @AckStatusID, @RowUpdatedUTC)";
        private const string Select_DeviceDataList_Query = "select hex(fk_AssetUid) as AssetUid, hex(fk_Deviceuid) as DeviceUid, SerialNumber from md_asset_AssetDevice ad inner join md_device_Device d on ad.fk_deviceuid = d.deviceuid inner join md_device_DeviceType dt on dt.DeviceTypeId = d.fk_DeviceTypeId where fk_assetuid IN ({0}) and TypeName = '{1}'";
        private readonly IDictionary<string, long> _deviceType = new Dictionary<string, long>();

        public MessageConstructorDeviceService(IOptions<Configurations> configurations)
        {
            _deviceConnectionString = configurations.Value.ConnectionString.MasterData;
            _deviceType = GetDeviceId();
        }

        public IDictionary<string, DeviceTypeFamily> GetDeviceTypeFamily()
        {
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                var deviceTypeFamilyList = mySqlConnection.Query<DeviceTypeFamily>(Select_DeviceTypeFamily_Query);
                if (deviceTypeFamilyList != null)
                    return deviceTypeFamilyList.ToDictionary(parameter => parameter.TypeName, parameter => parameter);
            }
            return new Dictionary<string, DeviceTypeFamily>();
        }

        public IDictionary<string, long> GetDeviceId()
        {
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                var deviceTypeFamilyList = mySqlConnection.Query<DeviceType>(Select_DeviceTypeID_Query);
                if (deviceTypeFamilyList != null)
                    return deviceTypeFamilyList.ToDictionary(parameter => parameter.TypeName, parameter => parameter.DeviceTypeID);
            }
            return new Dictionary<string, long>();
        }

        public bool PersistDeviceConfig(IEnumerable<DeviceConfigMsg> deviceConfigMessages)
        {
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                mySqlConnection.Open();
                int count = 0;
                foreach (var deviceConfigMessage in deviceConfigMessages)
                {
                    count = mySqlConnection.Execute(string.Format(Insert_DeviceCnfigMessage_Query + " values ({0}, {1}, @EventUTC, @MessageContent, @fk_StatusID, @LastMessageUTC, @fk_DeviceTypeID, {2});",
                     deviceConfigMessage.MessageUid.ToString("N").WrapWithUnhex(),
                     deviceConfigMessage.DeviceUid.ToString("N").WrapWithUnhex(),
                     deviceConfigMessage.UserUid.ToString("N").WrapWithUnhex()),
                   new
                   {
                       EventUTC = deviceConfigMessage.EventUtc,
                       MessageContent = deviceConfigMessage.MessageContent,
                       fk_StatusID = 0,
                       LastMessageUTC = DateTime.UtcNow,
                       fk_DeviceTypeID = _deviceType[deviceConfigMessage.DeviceType]
                   });
                   count++;
                }
                //Log.Info("Device Config Message Persisted with count : " + count);
                return count > 0;
            }
        }

        public DeviceData GetDeviceData(string assetUid)
        {
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                var deviceData =
                   mySqlConnection.Query<DeviceData>(String.Format(Select_DeviceData_Query, new Guid(assetUid).ToString("N").WrapWithUnhex())).FirstOrDefault();
                return deviceData;
            }
        }

        public IDictionary<string, string> GetDeviceSupportedFeatures(string assetUID, string deviceUID)
        {
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                var deviceData =
                   mySqlConnection.Query<DeviceFeatures>(String.Format(Select_DeviceFeature_Support_Query, 
                                                                    new Guid(assetUID).ToString("N").WrapWithUnhex(), 
                                                                    new Guid(deviceUID).ToString("N").WrapWithUnhex()));
                return deviceData.ToDictionary(data => data.DeviceFeatureName, data => Convert.ToString(data.isSupported));
            }
        }

        public bool PersistDeviceACKMessage(IEnumerable<DeviceACKMessage> deviceACKMessage)
        {
            int count = 0;
            using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
            {
                mySqlConnection.Open();
                count += mySqlConnection.Execute(Insert_DeviceAckMessage_Query, deviceACKMessage);
            }
            return count > 0;
        }
		public IEnumerable<DeviceData> GetDeviceData(List<string> assetUids, string deviceType)
		{
			using (var mySqlConnection = new MySqlConnection(_deviceConnectionString))
			{
				var deviceDatas =
				   mySqlConnection.Query<DeviceData>(String.Format(Select_DeviceDataList_Query, string.Join(",", assetUids.Select(assetUid => new Guid(assetUid).ToString("N").WrapWithUnhex())), deviceType));
				return deviceDatas;
			}
		}
	}
}
