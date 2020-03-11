using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.FuelBurntRate;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.Fuel_Burnt_Rate
{


    public class FuelBurntRateSupport
    {
        public FuelBurntRateSupport(Log4Net myLog)
        {
            Log = myLog;
            AssetServiceConfig.SetupEnvironment();
        }

        public static string MySqlConnectionString = AssetServiceConfig.MySqlConnection;
        private static Log4Net Log = new Log4Net(typeof(FuelBurntRateSupport));
        private List<string> assetUIDS = new List<string>();
        FuelBurntRateRequest fuelRequest = new FuelBurntRateRequest();
        FuelBurntRateResponse fuelResponse = new FuelBurntRateResponse();
        FuelBurntRateResponse getFuelResponse = new FuelBurntRateResponse();
        public Dictionary<string, string> CustomHeaders = new Dictionary<string, string>();



        public void SetDefaultValidValues(string workBurntRate, string idleBurntRate)
        {
            fuelRequest = new FuelBurntRateRequest { assetUIds = assetUIDS, workTargetValue = Convert.ToDouble(workBurntRate), idleTargetValue =Convert.ToDouble(idleBurntRate )};
        }

        public void FetchAssets()
        {
            string query = string.Format(AssetServiceMySqlQueries.Select_Assets, AssetServiceConfig.CustomerUID);

            MySqlDataReader reader = MySqlUtil.ExecuteMySQLQuery(MySqlConnectionString, query);
            while (reader.Read())
            {
                assetUIDS.Add(reader["HEX(AssetUID)"].ToString());
            }
            MySqlUtil.CloseConnection();
            List<Guid> assets = new List<Guid>();
            assets = assetUIDS.ConvertAll(Guid.Parse);
            assetUIDS.Clear();
            assetUIDS = assets.Select(a => Convert.ToString(a)).ToList();
        }
        public void PostFuelBurntRate(bool isvalid=true)
        {
            var requestString = JsonConvert.SerializeObject(fuelRequest);
            try
            {
                LogResult.Report(Log, "log_ForInfo", "Create Fuel Burnt rate: Starts");
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                string response;
                SetCustomerUIDHeader(AssetServiceConfig.CustomerUID);
                if (isvalid)
                {
                     response = RestClientUtil.DoHttpRequest(AssetServiceConfig.FuelBurnRateAPI, HeaderSettings.PutMethod, accessToken, HeaderSettings.JsonMediaType, requestString,
                    HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                }
                else
                {
                     response = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.FuelBurnRateAPI, HeaderSettings.PutMethod, accessToken, HeaderSettings.JsonMediaType, requestString,
                HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                }
                fuelResponse = JsonConvert.DeserializeObject<FuelBurntRateResponse>(response);
                LogResult.Report(Log, "log_ForInfo", "Create Fuel Burnt rate: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating fuel burnt rate " + e);
                throw new Exception("Exception Occured while creating  creating fuel burnt rate \n");
            }
        }

       
        public void GetFuelBurntRate()
        {
            try
            {
                var requestString = JsonConvert.SerializeObject(assetUIDS);
                LogResult.Report(Log, "log_ForInfo", "Get Fuel Burnt rate: Starts");
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                var response = RestClientUtil.DoHttpRequest(AssetServiceConfig.FuelBurnRateAPI, HeaderSettings.PostMethod, accessToken, HeaderSettings.JsonMediaType, requestString,
                  HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, CustomHeaders);
                getFuelResponse = JsonConvert.DeserializeObject<FuelBurntRateResponse>(response);
                LogResult.Report(Log, "log_ForInfo", "Get Fuel Burnt rate: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while getting fuel burnt rate " + e);
                throw new Exception("Exception Occured while getting fuel burnt rate \n");
            }

        }

        public void SetCustomerUIDHeader(string customerUID = "")
        {
            CustomHeaders = new Dictionary<string, string>();
            CustomHeaders.Add(AssetServiceConfig.CustomerUidHeader, customerUID);
        }

        public void VerifyValidValues()
        {
            try
            {
                for (int i = 0; i < fuelResponse.assetFuelBurnRateSettings.Count; i++)
                {
                    Assert.AreEqual(fuelResponse.assetFuelBurnRateSettings.ElementAt(i).assetUid, getFuelResponse.assetFuelBurnRateSettings.ElementAt(i).assetUid);
                    Assert.AreEqual(fuelResponse.assetFuelBurnRateSettings.ElementAt(i).idleTargetValue, getFuelResponse.assetFuelBurnRateSettings.ElementAt(i).idleTargetValue);
                    Assert.AreEqual(fuelResponse.assetFuelBurnRateSettings.ElementAt(i).workTargetValue, getFuelResponse.assetFuelBurnRateSettings.ElementAt(i).workTargetValue);
                    Assert.AreEqual(fuelResponse.assetFuelBurnRateSettings.ElementAt(i).startDate, getFuelResponse.assetFuelBurnRateSettings.ElementAt(i).startDate);
                }

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while assertion on fuel burnt rate " + e);
                throw new Exception("Exception Occured while assertion on fuel burnt rate  \n");

            }
        }

   public void VerifyInvalidValues()
        {


        }
        

    }
}
