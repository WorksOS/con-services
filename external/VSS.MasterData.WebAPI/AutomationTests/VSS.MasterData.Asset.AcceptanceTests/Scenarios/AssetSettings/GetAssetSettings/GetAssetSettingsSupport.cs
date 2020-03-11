
using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.GetAssetSettings
{
    public class GetAssetSettingsSupport
    {
        #region Variables
        private static Log4Net Log = new Log4Net(typeof(GetAssetSettingsSupport));
        GetAssetSettingsRequestModel getAssetSettingsRequest = new GetAssetSettingsRequestModel();
        GetAssetSettingsResponseModel getAssetSettingsResponse = new GetAssetSettingsResponseModel();
        public Guid CustomerUID = Guid.Parse(ConfigurationManager.AppSettings["CustomerUID"]);
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public Guid UserUID = Guid.Parse(ConfigurationManager.AppSettings["UserUID"]);
        public Guid DeviceUID = Guid.Parse(ConfigurationManager.AppSettings["DeviceUID-Dev"]);
        CreateAssetEvent CreateAsset = new CreateAssetEvent();
        public string CustomerUid = ConfigurationManager.AppSettings["X-VisionLink-CustomerUid"];
        public string AccessToken = string.Empty;
        public static Dictionary<string, string> CustomHeaders = new Dictionary<string, string>();
        public static string ResponseJSON = string.Empty;
        CreateAssetEvent Asset = new CreateAssetEvent();
        public static string MySqlConnectionString;
        AssociateAssetDevice associateAssetDevice = new AssociateAssetDevice();
        AssociateAssetCustomer associateAssetCustomer = new AssociateAssetCustomer();
        string queryString = null;
        string endPoint = null;
        #endregion

        #region Constructor
        public GetAssetSettingsSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
            Log = myLog;
        }

        #endregion

        #region Implementation

        public void SetDefaultValidValues()
        {

            getAssetSettingsRequest.CustomerUid = CustomerUID;
            getAssetSettingsRequest.PageNumber = 1;
            getAssetSettingsRequest.PageSize = 10;
            getAssetSettingsRequest.SortColumn = "assetid";
            getAssetSettingsRequest.UserUid = UserUID;
        }
        public void SetFilterNameFilterValue(string filterName, string filterValue)
        {
            queryString = "?PageNumber=" + Convert.ToString(getAssetSettingsRequest.PageNumber) + "&PageSize=" + Convert.ToString(getAssetSettingsRequest.PageSize) + "&filtername=" + filterName + "&filtervalue=" + filterValue;
            endPoint = AssetServiceConfig.AssetSettingsEndPoint + queryString;
        }
        public void GetAssetDetails(int pageNumber = 1)
        {
            GetToken(false);
            if (pageNumber == 1)
                CustomHeaders.Add(CustomerUid, CustomerUID.ToString());

            try
            {
                LogResult.Report(Log, "log_ForInfo", "GetAssetDetails-AssetSettings: Starts");
                if (queryString == null && endPoint == null)
                {
                    queryString = "?PageNumber=" + Convert.ToString(pageNumber) + "&PageSize=" + Convert.ToString(getAssetSettingsRequest.PageSize) + "&sortcolumn=" + Convert.ToString(getAssetSettingsRequest.SortColumn);
                    endPoint = AssetServiceConfig.AssetSettingsEndPoint + queryString;
                }

                LogResult.Report(Log, "log_ForInfo", "Get URL: " + endPoint);
                ResponseJSON = RestClientUtil.DoHttpRequest(endPoint, HeaderSettings.GetMethod, AccessToken, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                LogResult.Report(Log, "log_ForInfo", "GetAssetDetails-AssetSettings: Ends ");

            }
            catch (Exception e)
            {

            }

        }

        #region  Creating Assets And Mapping to Customer
        public void SetDefaultValidValuesToAssets()
        {
            CreateAsset = new CreateAssetEvent
            {
                AssetName = "ASSET" + Convert.ToString(DateTime.Now),
                LegacyAssetID = 18000265498,
                SerialNumber = "SN" + Convert.ToString(DateTime.Now),
                MakeCode = "MK001" + Convert.ToString(DateTime.Now),
                Model = "MODEL001",
                AssetType = "SKID STEER LOADERS",
                IconKey = 109,
                EquipmentVIN = "3AS6411",
                ModelYear = 2017,
                AssetUID = Guid.NewGuid(),
                OwningCustomerUID = CustomerUID,
                ActionUTC = DateTime.Now
            };
        }
        public void CreateAssets(int no_of_Assets = 1)
        {
            try
            {

                SetDefaultValidValuesToAssets();
                GetToken(true);
                var requestString = JsonConvert.SerializeObject(CreateAsset);
                LogResult.Report(Log, "log_ForInfo", "Create Asset: Starts" + requestString);
                var createAssetResponse = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, AccessToken,
                  HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                LogResult.Report(Log, "log_ForInfo", "Create Asset: Ends ");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating asset" + CreateAsset.AssetUID);
                throw new Exception("Exception Occured while creating asset\n" + CreateAsset.AssetUID);
            }
        }
        public void AssociateAssetDevice()
        {
            associateAssetDevice = new AssociateAssetDevice
            {
                DeviceUID = DeviceUID,
                AssetUID = CreateAsset.AssetUID,
                ReceivedUTC = DateTime.Now,
                ActionUTC = DateTime.Now
            };
            var requestString = JsonConvert.SerializeObject(associateAssetDevice);
            try
            {
                GetToken(true);
                var response = RestClientUtil.DoHttpRequest("https://api-stg.trimble.com/t/trimble.com/vss-dev-deviceservice/1.0/AssociateDeviceAsset", HeaderSettings.PostMethod, AccessToken,
                          HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
            }

            catch (Exception e)
            {

                LogResult.Report(Log, "log_ForError", "Exception Occured while associating asset to device" + "AssetUID-" + CreateAsset.AssetUID + "DeviceUID-" + associateAssetDevice.DeviceUID);
                throw new Exception("Exception Occured while associating asset to device" + "AssetUID - " + CreateAsset.AssetUID + "DeviceUID - " + associateAssetDevice.DeviceUID);
            }
        }
        public void AssociateAssetCustomer()
        {
            associateAssetCustomer = new AssociateAssetCustomer
            {
                CustomerUID = CustomerUID,
                AssetUID = CreateAsset.AssetUID,
                RelationType = "Customer",
                ActionUTC = DateTime.Now,
                ReceivedUTC = DateTime.Now
            };
            var requestString = JsonConvert.SerializeObject(associateAssetCustomer);
            try
            {
                GetToken(true);
                var response = RestClientUtil.DoHttpRequest("https://api-stg.trimble.com/t/trimble.com/vss-dev-customerservice/1.0/AssociateCustomerAsset", HeaderSettings.PostMethod, AccessToken,
                         HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while associating asset to device" + "AssetUID-" + CreateAsset.AssetUID + "Customer-" + associateAssetCustomer.CustomerUID);
                throw new Exception("Exception Occured while associating asset to device" + "AssetUID-" + CreateAsset.AssetUID + "Customer-" + associateAssetCustomer.CustomerUID);
            }

        }
        #endregion

        public void DeleteAsset()
        {
            try
            {
                LogResult.Report(Log, "log_ForInfo", "Delete Asset: Starts" + CreateAsset.AssetUID.ToString());
                GetToken(true);
                var endPoint = AssetServiceConfig.AssetServiceEndpoint + "?" + AssetServiceConfig.AssetUID + "=" + CreateAsset.AssetUID + "&ActionUTC=" + DateTime.Today;
                var DeleteAsset = RestClientUtil.DoHttpRequest(endPoint, HeaderSettings.DeleteMethod, AccessToken, null, HeaderSettings.JsonMediaType, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                LogResult.Report(Log, "log_ForInfo", "Delete Asset: Ends" + CreateAsset.AssetUID.ToString());
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while deleting asset" + CreateAsset.AssetUID);
                throw new Exception("Exception Occured while deleting asset\n" + CreateAsset.AssetUID);

            }
        }
        public void VerifyValidResponse()
        {
            try
            {
                getAssetSettingsResponse = JsonConvert.DeserializeObject<GetAssetSettingsResponseModel>(ResponseJSON);
                var result = getAssetSettingsResponse.assetSettings.Find(x => x.assetUid == CreateAsset.AssetUID);
                Assert.AreEqual(result.assetUid, CreateAsset.AssetUID);
            }
            catch (Exception e)

            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while  deserializing  getAssetSettingsResponse " + e);
                throw new Exception("Exception Occured while deserializing  getAssetSettingsResponse \n");
            }
        }
        public void VerifyInvalidResponse()
        {
            try
            {
                getAssetSettingsResponse = JsonConvert.DeserializeObject<GetAssetSettingsResponseModel>(ResponseJSON);
                var result = getAssetSettingsResponse.assetSettings.Find(x => x.assetUid == CreateAsset.AssetUID);
                Assert.AreEqual(null, result);
            }
            catch (Exception e)

            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while  deserializing  getAssetSettingsResponse " + e);
                throw new Exception("Exception Occured while deserializing  getAssetSettingsResponse \n");
            }

        }
        public void SetSortColumnAndType(string sortColumn, string sortType)
        {
            if (sortType == "ascending")
            {
                queryString = "?PageNumber=" + Convert.ToString(getAssetSettingsRequest.PageNumber) + "&PageSize=" + Convert.ToString(getAssetSettingsRequest.PageSize) + "&sortcolumn=" + sortColumn;
                endPoint = AssetServiceConfig.AssetSettingsEndPoint + queryString;
            }
            else
            {
                queryString = "?PageNumber=" + Convert.ToString(getAssetSettingsRequest.PageNumber) + "&PageSize=" + Convert.ToString(getAssetSettingsRequest.PageSize) + "&sortcolumn=-" + sortColumn;
                endPoint = AssetServiceConfig.AssetSettingsEndPoint + queryString;

            }
        }

        public void VerifyBasedOnSortColumn(string sortColumn)
        {

        }

        #endregion

        #region UtilityMethods

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

        #region DBValidation

        public List<string> GetMySQLResults(string queryString, string connectionString = "")
        {
            MySqlDataReader dataReader = null;
            List<string> dbResult = new List<string>();
            using (MySqlConnection mySqlConnection = new MySqlConnection(string.IsNullOrEmpty(connectionString) ? MySqlConnectionString : connectionString))
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
