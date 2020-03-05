using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.MileageTargetAPI;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.MileageTargetAPI
{
    public class MileageTargetAPISupport
    {
        #region variable

        public CreateAssetEvent createAssetEvent;
        public ResponseModel responseModel;
        public static Dictionary<string, string> CustomHeaders = new Dictionary<string, string>();
        public Guid DeviceUID = Guid.Parse(ConfigurationManager.AppSettings["DeviceUID-Dev"]);
        public static string CustomerUid = System.Configuration.ConfigurationManager.AppSettings["X-VisionLink-CustomerUid"];
        public static string CustomerUID = System.Configuration.ConfigurationManager.AppSettings["CustomerUID"];
        public DeviceAssetAssociationModel deviceAssetAssociationModel;
        public static string AccessToken = string.Empty;
        public string ResponseString = String.Empty;
        public string MySqlConnectionString;
        public Log4Net Log = new Log4Net(typeof(MileageTargetAPISupport));
        public MileageTargetAPIRequest mileageTargetAPIRequest;
        public MileageTargetAPIResponse mileageTargetAPIResponse;
        public AssociateAssetDevice associateAssetDevice;
        public AssociateAssetCustomer associateAssetCustomer;
        public static List<string> AssetUIDs = new List<string>();
        public List<Guid> DeviceAssetUIDs = new List<Guid>();
        public string AssetMasterDataConsumerKey = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
        public string AssetMasterDataConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];

        #endregion

        #region UtilityMethods

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
                OwningCustomerUID = Guid.Parse(CustomerUID),
                AssetUID = Guid.NewGuid(),
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
        }

        public List<string> CreateAsset(int noOfAssets= 1)
        {
            for (int i = 0; i < noOfAssets; i++)
            {
                SetDefaultValues();
                AssetUIDs.Add(PostRequest());
            }
            return AssetUIDs;
        }

        public void SetDefaultValuesForAssociateAssetDevice(List<string> assetUIDs)
        {
            DeviceAssetUIDs = AssetUIDs.Select(x=>Guid.Parse(x)).ToList();
            associateAssetDevice = new AssociateAssetDevice
            {
                DeviceUID = DeviceUID,
               // AssetUID = DeviceAssetUIDs,
                ReceivedUTC = DateTime.Now,
                ActionUTC = DateTime.Now
            };
        }

        public void AssociateAssetDevice(List<string> assetUIDs)
        {
            List<Guid> Asset = new List<Guid>();
            Asset = assetUIDs.Select(a => Guid.Parse(a)).ToList();
            for (int i = 0; i <= assetUIDs.Count(); i++)
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

        public void SetMileageTargetAPIDefaultValues(List<string> assetUIDs)
        {
            mileageTargetAPIRequest = new MileageTargetAPIRequest
            {
                assetUIDs = assetUIDs
            };
        }

        public int RandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(0000000, 2147483647);
            return randomNumber;
        }

        public void SetAssetUIDs(List<string> ListAssetUIDs)
        {
            mileageTargetAPIRequest.assetUIDs = ListAssetUIDs;
        }

        public string GetAssociateAssetDeviceRequestString()
        {
            return JsonConvert.SerializeObject(associateAssetDevice);
        }

        public string GetRequestString()
        {
            return JsonConvert.SerializeObject(createAssetEvent);
        }

        public void AssociateAssetCustomer(List<string> assetUIDs)
        {
            List<Guid> Asset = new List<Guid>();
            Asset = assetUIDs.Select(a => Guid.Parse(a)).ToList();
            for (int i = 0; i < assetUIDs.Count; i++)
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

        public void GetToken()
        {
            try
            {
                AccessToken = TokenService.GetAccessToken(null, AssetMasterDataConsumerKey, AssetMasterDataConsumerSecret);
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
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while Getting Access Token" + e);
                throw new Exception("Exception Occured while Getting Access Token\n");
            }
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

        #endregion

        #region PostMethod

        public string PostRequest()
        {
            responseModel = null;
            CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken(true);
            try
            {
                string createAssetRequestString = GetRequestString();
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
          /*  try
            {
                responseModel = JsonConvert.DeserializeObject<ResponseModel>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }*/
            return ResponseString;
        }

        public void PostAssociateAssetDevice()
        {
            responseModel = null;
            //CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken();
            try
            {
                string AssociateAssetDeviceRequestString = GetAssociateAssetDeviceRequestString();
                //CustomHeaders.Add(CustomerUid, CustomerUID);
                LogResult.Report(Log, "log_ForInfo", "Request string: " + AssociateAssetDeviceRequestString);
                LogResult.Report(Log, "log_ForInfo", "Post to URL: " + AssetServiceConfig.DeviceAssetAssociationEndpoint);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceAssetAssociationEndpoint, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, AssociateAssetDeviceRequestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "Response Recieved: " + ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data From AlertsManager Service", e);
                throw new Exception(e + " Got Error While Getting Data From AlertsManager Service");
            }
            try
            {
                //responseModel = JsonConvert.DeserializeObject<ResponseModel>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                //LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            //return responseModel;
        }
                
        public MileageTargetAPIResponse PostMileageTargetRequest()
        {
            mileageTargetAPIRequest = null;
            //CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken();
            try
            {
                string MileageTargetRequestString = GetRequestString();
                //CustomHeaders.Add(CustomerUid, CustomerUID);
                LogResult.Report(Log, "log_ForInfo", "Request string: " + MileageTargetRequestString);
                LogResult.Report(Log, "log_ForInfo", "Post to URL: " + AssetServiceConfig.FuelBurnRateAPI);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.FuelBurnRateAPI, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, MileageTargetRequestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "Response Recieved: " + ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data From AlertsManager Service", e);
                throw new Exception(e + " Got Error While Getting Data From AlertsManager Service");
            }
            try
            {
                mileageTargetAPIResponse = JsonConvert.DeserializeObject<MileageTargetAPIResponse>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            return mileageTargetAPIResponse;
        }

        #endregion

        #region DBMethod

        public bool ValidateDB(List<string> assetUIDs)
        {
            try
            {
                string query = string.Format(AssetServiceConfig.MileageTargerAPI, assetUIDs);
                List<string> queryResults = GetSQLResults(query);
                if (queryResults.Count != 0)
                {
                    LogResult.Report(Log, "log_ForInfo", "AlertConfigID: " + queryResults);
                }
                else
                {
                    LogResult.Report(Log, "log_ForError", "No Rows Returned From DB");
                }
                return true;
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
                throw new InvalidDataException("Error Occurred while executing db query");
            }
        }

        public List<string> GetSQLResults(string queryString)
        {
            MySqlDataReader dataReader = null;
            List<string> dbResult = new List<string>();
            using (MySqlConnection mySqlConnection = new MySqlConnection(MySqlConnectionString))
            {
                try
                {
                    //Open connection 
                    mySqlConnection.Open();
                    //Execute the SQL query
                    MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
                    //Read the results into a SqlDataReader and store in string variable for later reference
                    dataReader = mySqlCommand.ExecuteReader();
                    while (dataReader != null && dataReader.Read())
                    {
                        if (dataReader.HasRows)
                        {
                            for (int i = 0; i < dataReader.VisibleFieldCount; i++)
                            {
                                dbResult.Add(dataReader[i].ToString());
                            }
                        }
                        //dataReader.ToString();
                    }
                }
                catch (Exception e)
                {
                    LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
                    throw new InvalidDataException("Error Occurred while executing db query");
                }
            };
            return dbResult;
        }

        #endregion
    }
}
