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
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.VolumePerCycle;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.VolumePerCycle
{
    public class VolumePerCycleSupport
    {
        #region Variables

        public CreateAssetEvent createAssetEvent;
        public ResponseModel responseModel;
        public POSTVolumePerCycleRequest pOSTVolumePerCycleRequest;
        public POSTVolumePerCycleResponse pOSTVolumePerCycleResponse;
        public AssociateAssetCustomer associateAssetCustomer;
        public DeviceAssetAssociationModel deviceAssetAssociationModel;
        public Guid CustomerUID = Guid.Parse(ConfigurationManager.AppSettings["CustomerUID"]);
        public Guid DeviceUID = Guid.Parse(ConfigurationManager.AppSettings["DeviceUID-Dev"]);
        public Log4Net Log = new Log4Net(typeof(VolumePerCycleSupport));
        public string MySqlConnectionString;
        public string ResponseString = String.Empty;
        public static string AccessToken = string.Empty;
        public List<string> AssetUIDs = new List<string>();
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public Dictionary<string, string> CustomHeaders { get; private set; }

        #endregion

        #region UtilityMethods

        public List<string> CreateAsset(int noOfAssets = 1)
        {
            for (int i = 0; i < noOfAssets; i++)
            {
                SetDefaultValues();
                AssetUIDs.Add(PostRequest().AssetUID);
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
                OwningCustomerUID = CustomerUID,
                AssetUID = Guid.NewGuid(),
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
        }

        public string GetRequestString()
        {
            return JsonConvert.SerializeObject(createAssetEvent);
        }

        public string GetAssociateAssetCustomerRequestString()
        {
            return JsonConvert.SerializeObject(associateAssetCustomer);
        }

        public int RandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(0000000, 2147483647);
            return randomNumber;
        }

        public void GetToken()
        {
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


        public void SetVolumePerCycleAPIDefaultValues(List<string> assetUIDs)
        {
            pOSTVolumePerCycleRequest = new POSTVolumePerCycleRequest
            {
                assetUIDs = assetUIDs
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

        public void AssociateAssetCustomer(List<string> assetUIDs)
        {
            List<Guid> Asset = new List<Guid>();
            Asset = assetUIDs.Select(a=>Guid.Parse(a)).ToList();
            for (int i = 0; i < assetUIDs.Count; i++)
            {
                Guid assetuid = Asset.ElementAt(i);
                associateAssetCustomer = new AssociateAssetCustomer
                {
                    CustomerUID = CustomerUID,
                    AssetUID = assetuid,
                    RelationType = "Customer",
                    ActionUTC = DateTime.Now,
                    ReceivedUTC = DateTime.Now
                };
            }
        }

        #endregion

        #region PostMethod

        public ResponseModel PostRequest()
        {
            responseModel = null;
            CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken();
            try
            {
                string createAssetRequestString = GetRequestString();
                //CustomHeaders.Add(CustomerUid, CustomerUID);
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
                responseModel = JsonConvert.DeserializeObject<ResponseModel>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            return responseModel;
        }

        public POSTVolumePerCycleResponse PostVolumePerCycleRequest()
        {
            pOSTVolumePerCycleResponse = null;
            //CustomHeaders = new Dictionary<string, string>();
            // if (string.IsNullOrEmpty(AccessToken))
            GetToken();
            try
            {
                string VolumePerCycleRequestString = GetRequestString();
                //CustomHeaders.Add(CustomerUid, CustomerUID);
                LogResult.Report(Log, "log_ForInfo", "Request string: " + VolumePerCycleRequestString);
                LogResult.Report(Log, "log_ForInfo", "Post to URL: " + AssetServiceConfig.VolumePerCycleAPI);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.MileageTargerAPI, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, VolumePerCycleRequestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "Response Recieved: " + ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data From AlertsManager Service", e);
                throw new Exception(e + " Got Error While Getting Data From AlertsManager Service");
            }
            try
            {
                pOSTVolumePerCycleResponse = JsonConvert.DeserializeObject<POSTVolumePerCycleResponse>(ResponseString);
                LogResult.Report(Log, "log_ForInfo", "Deserialized the response");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception(e + " Got Error While DeSerializing JSON Object");
            }
            return pOSTVolumePerCycleResponse;
        }

        public void PostAssociateAssetCustomer()
        {
            try
            {
                string AssociateAssetCustomerRequest = GetAssociateAssetCustomerRequestString();
                string AssociateAssetCustomer = "https://api-stg.trimble.com/t/trimble.com/vss-dev-customerservice/1.0/AssociateCustomerAsset";
                var Responsestring = RestClientUtil.DoHttpRequest(AssociateAssetCustomer, HeaderSettings.PostMethod, AccessToken, HeaderSettings.JsonMediaType, AssociateAssetCustomerRequest, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
            }
            catch(Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while associating asset to device" + "AssetUID-" + createAssetEvent.AssetUID + "Customer-" + associateAssetCustomer.CustomerUID);
                throw new Exception("Exception Occured while associating asset to device" + "AssetUID-" + createAssetEvent.AssetUID + "Customer-" + associateAssetCustomer.CustomerUID);
            }
            
        }

        #endregion

        #region DBMethods

        public bool ValidateDB(List<string> assetUIDs)
        {
            try
            {
                string query = string.Format(AssetServiceConfig.VolumePerCycleAPI, assetUIDs);
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
