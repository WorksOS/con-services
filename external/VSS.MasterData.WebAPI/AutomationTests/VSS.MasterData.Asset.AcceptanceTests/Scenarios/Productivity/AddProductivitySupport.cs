using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationCore.Shared.Library;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.ProductivityTargets;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Library;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Common;
using System.Net;
using VSP.MasterData.Asset.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Asset.AcceptanceTests.Resources;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.Productivity
{
    public class AddProductivitySupport
    {
        #region Variables
        private Log4Net myLog;
        public static string MySqlConnectionString;
        private static Log4Net Log = new Log4Net(typeof(AddProductivitySupport));
        public ProductivityTargetsRequestModel CreateproductivityTargetsRequest = new ProductivityTargetsRequestModel();
        public string AccessToken = string.Empty;
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public static Guid AssetUId = Guid.Parse(ConfigurationManager.AppSettings["AssetUID1"]);
        public static string ResponseJSON = string.Empty;
        public ProductivityTargetsResponseModel CreateProductitvityResponse = new ProductivityTargetsResponseModel();
        public RetrieveProductivityDetails retrieveProductivityDetails = new RetrieveProductivityDetails();

        #endregion

        #region constructor
        public AddProductivitySupport(Log4Net Log)
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
            Log = myLog;
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Set Default values for create productivity details
        /// </summary>
        public void SetDefaultValidValuesForProductivity()
        {
            CreateproductivityTargetsRequest = new ProductivityTargetsRequestModel
            {
                assettargets = new List<Targets>()
                {
                    new Targets
                    {
                        targetcycles=new Targetcycles
                        {  Sunday="8",
                            Monday="7",
                            Tuesday="6",
                            Wednesday="5",
                            Thursday="3",
                            Friday="2",
                            Saturday="7"
                        },
                        targetpayload=new Targetpayload
                        {  Sunday="8",
                            Monday="7",
                            Tuesday="6",
                            Wednesday="5",
                            Thursday="3",
                            Friday="2",
                            Saturday="7"
                        },
                        targetvolumes=new Targetvolumes
                        {  Sunday="8",
                            Monday="7",
                            Tuesday="6",
                            Wednesday="5",
                            Thursday="3",
                            Friday="2",
                            Saturday="7"
                        },
                        startdate=DateTime.Now,
                        enddate=DateTime.Now.AddDays(10),
                        assetuid=AssetUId

                    } }
            };
        }
        /// <summary>
        /// Validating Error Code
        /// </summary>
        public void ErrorCodeValidation(int errorCode)
        {
            string resourceError = ErrorCodes.ResourceManager.GetString(errorCode.ToString());
            Assert.AreEqual(errorCode, CreateProductitvityResponse.error.errorCode);
            Assert.AreEqual(resourceError, CreateProductitvityResponse.error.message);
        }

        /// <summary>
        /// Verifies the Productivity Details
        /// </summary>
        public void VerifyProductivityDetails()
        {

            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Sunday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Sunday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Monday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Monday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Tuesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Tuesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Wednesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Wednesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Thursday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Thursday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Friday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Friday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetcycles.Saturday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetcycles.Saturday).FirstOrDefault());

            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Saturday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Saturday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Monday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Monday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Tuesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Tuesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Wednesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Wednesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Thursday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Thursday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Friday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Friday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetvolumes.Saturday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetvolumes.Saturday).FirstOrDefault());


            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Sunday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Sunday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Monday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Monday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Tuesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Tuesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Wednesday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Wednesday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Thursday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Thursday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Friday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Friday).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.targetpayload.Saturday).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.targetpayload.Saturday).FirstOrDefault());

            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.startdate).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.startdate).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.enddate).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.enddate).FirstOrDefault());
            Assert.AreEqual(CreateproductivityTargetsRequest.assettargets.Select(a => a.assetuid).FirstOrDefault(), retrieveProductivityDetails.productivitytargets.Select(a => a.assetuid).FirstOrDefault());
        }

        /// <summary>
        ///  Creates productivity details for an asset/assets
        /// </summary>
        public void ValidPutRequest()
        {
            var requestString = JsonConvert.SerializeObject(CreateproductivityTargetsRequest);
            try
            {
                GetToken();
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Starts");
                var ProductivityResponse = RestClientUtil.DoHttpRequest(AssetServiceConfig.CreateProductivityDetailsEndpoint, HeaderSettings.PutMethod, AccessToken,
                                       HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                CreateProductitvityResponse = JsonConvert.DeserializeObject<ProductivityTargetsResponseModel>(ProductivityResponse);
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating asset settings" + CreateproductivityTargetsRequest.assettargets.Select(a => a.assetuid));
                throw new Exception("Exception Occured while creating asset productivity \n");
            }

        }
 public void InvalidPutRequest()
        {

            var requestString = JsonConvert.SerializeObject(CreateproductivityTargetsRequest);
            try
            {
                GetToken();
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Starts");
                var ProductivityResponse = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.CreateProductivityDetailsEndpoint, HeaderSettings.PutMethod, AccessToken,
                                       HeaderSettings.JsonMediaType, requestString, HttpStatusCode.BadRequest, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                CreateProductitvityResponse = JsonConvert.DeserializeObject<ProductivityTargetsResponseModel>(ProductivityResponse);
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating asset settings" + CreateproductivityTargetsRequest.assettargets.Select(a => a.assetuid));
                throw new Exception("Exception Occured while creating asset productivity \n");
            }

        }

        public void ModifyDefaultProductivityTargetValues(string assetTargetName, string assetTargetValue)
        {
            switch (assetTargetName)
            {
                case "targetcycles":
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Saturday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Sunday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Monday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Tuesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Wednesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Thursday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetcycles.Friday = assetTargetValue;


                    break;

                case "targetvolumes":
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Saturday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Sunday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Monday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Tuesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Wednesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Thursday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetvolumes.Friday = assetTargetValue;
                    break;

                case "targetpayload":
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Saturday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Sunday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Monday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Tuesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Wednesday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Thursday = assetTargetValue;
                    CreateproductivityTargetsRequest.assettargets.FirstOrDefault().targetpayload.Friday = assetTargetValue;
                    break;

            }
        }

        public void RetrieveProductivityDetails()
        {
            string requestString = string.Empty;

            requestString = JsonConvert.SerializeObject(CreateProductitvityResponse);
            requestString = requestString.Replace("\"assetUID\":", "");
            requestString = requestString.Replace("{", "");
            requestString = requestString.Replace("}", "");


            try
            {

                GetToken();
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Starts");
                var ProductivityResponse = RestClientUtil.DoHttpRequest(AssetServiceConfig.RetrieveProductivityDetailsEndpoint + "?startdate=" + CreateproductivityTargetsRequest.assettargets.Select(a => a.startdate).FirstOrDefault() + "&enddate=" + CreateproductivityTargetsRequest.assettargets.Select(a => a.enddate).FirstOrDefault(), HeaderSettings.PostMethod, AccessToken,
                                       HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                retrieveProductivityDetails = JsonConvert.DeserializeObject<RetrieveProductivityDetails>(ProductivityResponse);
                LogResult.Report(Log, "log_ForInfo", "Create Asset Productivity: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating asset settings" + CreateproductivityTargetsRequest.assettargets.Select(a => a.assetuid));
                throw new Exception("Exception Occured while creating asset productivity \n");
            }

        }



        #endregion

        #region UtilityMethods

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
        #endregion

        #region DBValidation
        /// <summary>
        /// Deletes AssetWeeklyConfig for a particular AssetUID
        /// </summary>
        public void UpdateDB()
        {
            string query = string.Format(AssetSettingsMySqlQueries.DeleteAssetWeeklyConfig, AssetUId.ToStringWithoutHyphens());
            var dbResult = GetMySQLResults(query);
        }


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
