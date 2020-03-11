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
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.AddAssetSettings
{
    public class AddAssetSettingsSupport
    {
        #region Variables
        public static string MySqlConnectionString = string.Empty;
        private static Log4Net Log = new Log4Net(typeof(AddAssetSettingsSupport));
        public static CreateAssetSettings createAssetSettingsRequest = null;
        public string AccessToken = string.Empty;
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public Guid AssetUId = Guid.Parse(ConfigurationManager.AppSettings["AssetUID1"]);
        public static string ResponseJSON = string.Empty;
        public static CreateAssetSettingsResponse createAssetSettingsResponse = new CreateAssetSettingsResponse();
        public static RetrieveAssetSettingsResponse RetrieveAssetSettingsResponse = new RetrieveAssetSettingsResponse();

        #endregion

        #region Constructor

        public AddAssetSettingsSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            MySqlConnectionString = AssetServiceConfig.MySqlConnection;
            Log = myLog;
        }

        #endregion

        #region Implementation

        public void SetDefaultValidValues()
        {
            createAssetSettingsRequest = new CreateAssetSettings
            {
                assetTargetSettings = new List<AssetTargetSetting>
                {
                    new AssetTargetSetting
                    {
                        runtime=new Runtime
                        {   monday=5,
                            tuesday =5,
                            wednesday =5,
                            thursday =5,
                            friday =5,
                            saturday =5,
                            sunday =5,
                        },
                        idle=new Idle {
                            monday=5,
                            tuesday =5,
                            wednesday =5,
                            thursday =5,
                            friday =5,
                            saturday =5,
                            sunday =5,

                        },
                        startDate=DateTime.Now,
                        endDate=DateTime.Now.AddDays(10),
                        assetUid=AssetUId

                    }
                }
            };
        }

        public void VerifyAssetSettings()
        {

        }

        public void RetrieveAssetSettings()
        {
            string requestString = string.Empty;
            string asset = string.Empty;
            var count = 1;

            foreach (var assetTarget in createAssetSettingsRequest.assetTargetSettings)
            {
                Convert.ToString(createAssetSettingsRequest.assetTargetSettings.Select(a => a.assetUid));

                asset = "\"" + asset + "\",";
                count = count + 1;
            }
            requestString = "[" + asset + "]";
            if (count == 1)
                requestString.Trim(',');

            GetToken();
            LogResult.Report(Log, "log_ForInfo", "Retrieve Asset Settings: Starts");
            var retrieveAssetSettingsUrl = @"https://api-stg.trimble.com/t/trimble.com/vss-dev-assetservice/1.0/assetsettings/targets/retrieve?" + Convert.ToString(createAssetSettingsRequest.assetTargetSettings.Select(a => a.startDate)) +
                "&" + Convert.ToString(createAssetSettingsRequest.assetTargetSettings.Select(a => a.endDate));
            var RetrieveAssetResponse = RestClientUtil.DoHttpRequest(retrieveAssetSettingsUrl, HeaderSettings.PostMethod, AccessToken,
                                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
            RetrieveAssetSettingsResponse = JsonConvert.DeserializeObject<RetrieveAssetSettingsResponse>(RetrieveAssetResponse);
            LogResult.Report(Log, "log_ForInfo", "Retrieve Asset Settings: Ends");
        }

        public void CreateValidAssetSettings()
        {
            var requestString = JsonConvert.SerializeObject(createAssetSettingsRequest);
            try
            {
                GetToken();
                LogResult.Report(Log, "log_ForInfo", "Create Asset Settings: Starts");
                var createAssetResponse = RestClientUtil.DoHttpRequest("https://api-stg.trimble.com/t/trimble.com/vss-dev-assetservice/1.0/assetsettings/targets/Save", HeaderSettings.PutMethod, AccessToken,
                                       HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType, null);
                createAssetSettingsResponse = JsonConvert.DeserializeObject<CreateAssetSettingsResponse>(createAssetResponse);
                LogResult.Report(Log, "log_ForInfo", "Create Asset Settings: Ends");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Exception Occured while creating asset settings" + createAssetSettingsRequest.assetTargetSettings.Select(a => a.assetUid));
                throw new Exception("Exception Occured while creating asset settings \n");
            }
        }
        
        #endregion

        #region Utility Methods

        public void GetToken()
        {
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

        #endregion

    }
}