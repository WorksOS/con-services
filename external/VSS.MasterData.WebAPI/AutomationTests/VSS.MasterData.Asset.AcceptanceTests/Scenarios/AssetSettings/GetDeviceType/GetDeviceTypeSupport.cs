using System;
using AutomationCore.Shared.Library;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using System.Configuration;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Library;
using AutomationCore.API.Framework.Common;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings;
using System.Net;
using System.Collections.Generic;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.GetDeviceType
{
    public class GetDeviceTypeSupport
    {
        #region Variables
        private static Log4Net Log = new Log4Net(typeof(GetDeviceTypeSupport));
        public static string MySqlConnectionString;
        public static CreateDeviceModel defaultValidDeviceServiceCreateModel = new CreateDeviceModel();
        public string AccessToken = string.Empty;
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public string AssetUID = ConfigurationManager.AppSettings["AssetUID1"];
        public Guid AssetUidGuid;
        AssociateAssetDevice associateAssetDevice = new AssociateAssetDevice();
        public string CustomerUid = ConfigurationManager.AppSettings["X-VisionLink-CustomerUid"];
        public Guid CustomerUID = Guid.Parse(ConfigurationManager.AppSettings["CustomerUID"]);
        public static Dictionary<string, string> CustomHeaders = new Dictionary<string, string>();
        public DeviceTypeResponse deviceTypeResponse = new DeviceTypeResponse();
        #endregion

        #region Constructor
        public GetDeviceTypeSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
            Log = myLog;
        }

        #endregion

        #region Implementation

        public void SetDefaultValuesToDevice()
        {
            defaultValidDeviceServiceCreateModel.DeviceUID = Guid.NewGuid();
            defaultValidDeviceServiceCreateModel.DeviceSerialNumber = "AutoTest-CreateDevice" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidDeviceServiceCreateModel.DeviceType = "PLE631-" + DateTime.UtcNow.ToString("yyyy/MM/dd");
            defaultValidDeviceServiceCreateModel.DeviceState = "Provisioned"; //"Subscribed";
            defaultValidDeviceServiceCreateModel.DeregisteredUTC = null;
            defaultValidDeviceServiceCreateModel.ModuleType = null;
            defaultValidDeviceServiceCreateModel.MainboardSoftwareVersion = null;
            defaultValidDeviceServiceCreateModel.RadioFirmwarePartNumber = null;
            defaultValidDeviceServiceCreateModel.GatewayFirmwarePartNumber = null;
            defaultValidDeviceServiceCreateModel.DataLinkType = null;
            defaultValidDeviceServiceCreateModel.ActionUTC = DateTime.UtcNow;
            defaultValidDeviceServiceCreateModel.ReceivedUTC = null;

        }
        public void CreateDevice()
        {
            try
            {
                GetToken(true);
                var requestString = JsonConvert.SerializeObject(defaultValidDeviceServiceCreateModel);
                LogResult.Report(Log, "log_ForInfo", "Create Device: Starts" + requestString);
                var createDeviceRespoonse = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceServiceEndpoint, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                LogResult.Report(Log, "log_ForInfo", "Create Device: Ends ");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating Device" + defaultValidDeviceServiceCreateModel.DeviceUID);
                throw new Exception("Exception Occured while creating device\n" + defaultValidDeviceServiceCreateModel.DeviceUID);
            }
        }

        public void AssociateAssetDevice()
        {
            Guid.TryParse(AssetUID, out AssetUidGuid);
            associateAssetDevice = new AssociateAssetDevice
            {
                DeviceUID = defaultValidDeviceServiceCreateModel.DeviceUID,
                AssetUID = AssetUidGuid,
                ReceivedUTC = DateTime.Now,
                ActionUTC = DateTime.Now
            };
            var requestString = JsonConvert.SerializeObject(associateAssetDevice);
            try
            {
                GetToken(true);
                var response = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceAssetAssociationEndpoint, HeaderSettings.PostMethod, AccessToken,
                          HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                LogResult.Report(Log, "log_ForInfo", "Device Asset Association ends-Success ");
            }

            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while associating asset to device" + "AssetUID-" + AssetUidGuid + "DeviceUID-" + defaultValidDeviceServiceCreateModel.DeviceUID);
                throw new Exception("Exception Occured while associating asset to device" + "AssetUID - " + AssetUidGuid + "DeviceUID - " + defaultValidDeviceServiceCreateModel.DeviceUID);
            }
        }

        public void GetDeviceTypeDetails()
        {
            try
            {
                LogResult.Report(Log, "log_ForInfo", "GetDeviceTypeDetails: Starts for Customer" + CustomerUID);
                if (CustomHeaders == null)
                    CustomHeaders.Add(CustomerUid, CustomerUID.ToString());
                GetToken(false);
                var ResponseJSON = RestClientUtil.DoHttpRequest(AssetServiceConfig.GetDeviceType, HeaderSettings.GetMethod, AccessToken, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                deviceTypeResponse = JsonConvert.DeserializeObject<DeviceTypeResponse>(ResponseJSON);
                LogResult.Report(Log, "log_ForInfo", "GetDeviceTypeDetails: Ends for Customer" + CustomerUID);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while getting device type details" + "CustomerUID-" + CustomerUID);
                throw new Exception("Exception Occured while getting device type details" + "CustomerUID - " + CustomerUID + "DeviceUID - ");
            }
        }

        public void VerifyGetDeviceTypeDetails()
        {
            var result = deviceTypeResponse.DeviceTypes.Find(x => (x.Id.Equals(defaultValidDeviceServiceCreateModel.DeviceType)));

            Assert.AreEqual(defaultValidDeviceServiceCreateModel.DeviceType, result.Id);
            Assert.AreEqual(defaultValidDeviceServiceCreateModel.DeviceType, result.Name);

        }
        public void VerifyGetDeviceTypeDetailsCount()
        {
            var result = deviceTypeResponse.DeviceTypes.FindAll(x => (x.Id.Equals(defaultValidDeviceServiceCreateModel.DeviceType)));
            Assert.AreEqual(1, result.Count);
        }

        public void ChangeCustomerUID()
        {
            CustomerUID = Guid.Parse(ConfigurationManager.AppSettings["CustomerUID2"]);
            CustomHeaders.Add(CustomerUid, CustomerUID.ToString());
        }

        public void VerifyDeviceTypeDifferentCustomer()
        {
            var result = deviceTypeResponse.DeviceTypes.FindAll(x => (x.Id.Equals(defaultValidDeviceServiceCreateModel.DeviceType)));
            Assert.AreEqual(0, result.Count);
        }


        public void GetDeviceTypeCustomerNull()
        {
            LogResult.Report(Log, "log_ForInfo", "GetDeviceTypeDetails: Starts for Customer" + CustomerUID);
            GetToken(false);
            CustomHeaders.Add(CustomerUid, "");
            var ResponseJSON = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.GetDeviceType, HeaderSettings.GetMethod, AccessToken, HeaderSettings.JsonMediaType, null, HttpStatusCode.BadRequest, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
            deviceTypeResponse = JsonConvert.DeserializeObject<DeviceTypeResponse>(ResponseJSON);
            LogResult.Report(Log, "log_ForInfo", "GetDeviceTypeDetails: Ends for Customer" + CustomerUID);
        }

        public void VerifyErrorResponse()
        {
            Assert.AreEqual("4000411", deviceTypeResponse.errors.First().ErrorCode);
            Assert.AreEqual("CustomerUid is null", deviceTypeResponse.errors.First().Message);
        }
        #endregion

        #region UtilityMethods
        public void GetToken(bool isCreateDevice = false)
        {
            try
            {
                if (isCreateDevice)
                {
                    AccessToken = TokenService.GetAccessToken("https://identity-stg.trimble.com/i/oauth2/token?grant_type=client_credentials&scope=device_1", "jMQxwhtHohyry_V9Jr0HYMLIrhka", "oZUrkfYuKAmCW19QjUY4hm3dNksa");
                    if (!string.IsNullOrEmpty(AccessToken))
                    {
                        LogResult.Report(Log, "log_ForInfo", "AccessToken: " + AccessToken);
                    }
                    else
                    {
                        LogResult.Report(Log, "log_ForError", "AccessToken is empty or null");
                        throw new Exception("AccessToken is empty or null\n");
                    }
                }
                else
                {
                    AccessToken = TokenService.GetAccessToken("https://identity-stg.trimble.com/token?grant_type=password&username=alagammai_annamalai%2b10035%40trimble.com&password=Sanjayalagi12%40", ConsumerKey, ConsumerSecret);
                    if (!string.IsNullOrEmpty(AccessToken))
                    {
                        LogResult.Report(Log, "log_ForInfo", "AccessToken: " + AccessToken);
                    }
                    else
                    {
                        LogResult.Report(Log, "log_ForError", "AccessToken is empty or null");
                        throw new Exception("AccessToken is empty or null\n");
                    }
                }
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while Getting Access Token" + e);
                throw new Exception("Exception Occured while Getting Access Token\n");
            }
        }
        #endregion
    }
}
