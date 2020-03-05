using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.FuelBurnRate;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.FuelBurnRate
{
      public class FuelBurnRateSupport
    {
        #region variables 
        public CreateAssetEvent createAssetEvent;
        public ResponseModel responseModel;
        public FuelBurnRateRequest fuelBurnRateRequest;
        public FuelBurnRateResponse fuelBurnRateResponse;
        public List<string> AssetUIDs = new List<string>();
        public AssociateAssetDevice associateAssetDevice;
        public AssociateAssetCustomer associateAssetCustomer;
        public static string AccessToken = string.Empty;
        public string ResponseString = String.Empty;
        public Log4Net Log = new Log4Net(typeof(FuelBurnRateSupport));
        public static string CustomerUID = System.Configuration.ConfigurationManager.AppSettings["CustomerUID"];
        public Guid DeviceUID = Guid.Parse(ConfigurationManager.AppSettings["DeviceUID-Dev"]);
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public DeviceAssetAssociationModel deviceAssetAssociationModel;
        public string MySqlConnectionString;
        public static Dictionary<string, string> CustomHeaders = new Dictionary<string, string>();
        public string AssetMasterDataConsumerKey = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
        public static string CustomerUid = System.Configuration.ConfigurationManager.AppSettings["X-VisionLink-CustomerUid"];
        public string AssetMasterDataConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

        #endregion

        #region Constructor

        public FuelBurnRateSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
            Log = myLog;
        }
        
        public FuelBurnRateSupport()
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
        }

        #endregion

        #region UtilityMethod

        public List<string> CreateAsset(int noOfAssets = 1)
        {
            for (int i = 0; i < noOfAssets; i++)
            {
                //SetVolumePerCycleAPIDefaultValues();
                SetDefaultValues();
                AssetUIDs.Add(PostRequest());
            }
            return AssetUIDs;
        }

        public void SetDefaultValues()
        {
            createAssetEvent = new CreateAssetEvent
            {
                AssetName = "Asset" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                LegacyAssetID = RandomNumber(),
                SerialNumber = "SerialNumber" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                MakeCode = "MakeCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                Model = "Model" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                AssetType = "AssetType" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                IconKey = RandomNumber(),
                EquipmentVIN = "EquipmentVIN" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                ModelYear = 2017,
                OwningCustomerUID = new Guid(CustomerUID),
                AssetUID = Guid.NewGuid(),
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow

            };
        }

        public string PostRequest()
        {
            responseModel = null;
            //CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken(true);
            try
            {
                string createAssetRequestString = GetCreateAssetRequestString();
                CustomHeaders.Add(CustomerUid, CustomerUID);
                LogResult.Report(Log, "log_ForInfo", "Request string: " + createAssetRequestString);
                LogResult.Report(Log, "log_ForInfo", "Post to URL: " + AssetServiceConfig.AssetServiceEndpoint);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, createAssetRequestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "Response Recieved: " + ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data From AlertsManager Service", e);
                throw new Exception(e + " Got Error While Getting Data From AlertsManager Service");
            }
            try
            {
                // responseModel = JsonConvert.DeserializeObject<ResponseModel>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            return ResponseString;
        }

        public string GetCreateAssetRequestString()
        {
            return JsonConvert.SerializeObject(createAssetEvent);
        }

        public void AssociateAssetDevice(List<string> AssetUIDs)
        {
            try
            {
                List<Guid> Asset = new List<Guid>();
                Asset = AssetUIDs.Select(a => Guid.Parse(a)).ToList();
                //foreach(string assetuid in AssetUIDs)
                for (int i = 0; i <= AssetUIDs.Count(); i++)
                {
                    Guid assetuid = Asset.ElementAt(i);
                    deviceAssetAssociationModel = new DeviceAssetAssociationModel
                    {
                        DeviceUID = DeviceUID,
                        AssetUID = assetuid,
                        ActionUTC = DateTime.Now,
                        ReceivedUTC = DateTime.Now
                    };
                }

            }
            catch(Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
            }

        }

        public void AssociateAssetCustomer(List<string> AssetUIDs = null)
        {
            try
            {
                List<Guid> Asset = new List<Guid>();
                Asset = AssetUIDs.Select(a => Guid.Parse(a)).ToList();
                for (int i = 0; i < AssetUIDs.Count; i++)
                {
                    Guid assetuid = Asset.ElementAt(i);
                    associateAssetCustomer = new AssociateAssetCustomer
                    {
                        CustomerUID = Guid.Parse(CustomerUID),
                        AssetUID = assetuid,
                        RelationType = "Customer",
                        ActionUTC = DateTime.Now,
                        ReceivedUTC = DateTime.Now
                    };
                }

            }
            catch(Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
            }
            
        }

        public int RandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(0000000, 2147483647);
            return randomNumber;
        }

        public void VerifyAssetSettings()
        {

        }

        public void GetToken(bool isCreateAsset = false)
        {
            try
            {
                if (isCreateAsset)
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
                    AccessToken = TokenService.GetAccessToken("https://identity-stg.trimble.com/token?grant_type=password&username=alagammai_annamalai%2b10035%40trimble.com&password=Sanjayalagi1%40", ConsumerKey, ConsumerSecret);
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

        public void SetFuelBurnAPIDefaultValues(List<string> assetUIDs)
        {
            //fuelBurnRateRequest.assetUIds = assetUIDs;
            //fuelBurnRateRequest.IdleTargetValue = 0;
            //fuelBurnRateRequest.workTargetValue = 0;

            fuelBurnRateRequest = new FuelBurnRateRequest
            {
                assetUIds = assetUIDs,
                IdleTargetValue = 0,
                workTargetValue = 0
            };
                               
        }

        public string GetFuelBurnRateRequest()
        {
            return JsonConvert.SerializeObject(fuelBurnRateRequest);
        }

        #endregion

        #region PostMethod

        public FuelBurnRateRequest PostMileageTargetRequest()
        {
            //fuelBurnRateRequest = null;
            //CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken();
            try
            {
                string FuelBurnRequestString = GetFuelBurnRateRequest();
                //CustomHeaders.Add(CustomerUid, CustomerUID);
                LogResult.Report(Log, "log_ForInfo", "Request string: " + FuelBurnRequestString);
                LogResult.Report(Log, "log_ForInfo", "Post to URL: " + AssetServiceConfig.FuelBurnRateAPI);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.FuelBurnRateAPI, HeaderSettings.PutMethod, AccessToken, HeaderSettings.JsonMediaType, FuelBurnRequestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "Response Recieved: " + ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data From AlertsManager Service", e);
                throw new Exception(e + " Got Error While Getting Data From AlertsManager Service");
            }
            try
            {
                fuelBurnRateResponse = JsonConvert.DeserializeObject<FuelBurnRateResponse>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            return fuelBurnRateRequest;
        }
               
        #endregion

    }
}
