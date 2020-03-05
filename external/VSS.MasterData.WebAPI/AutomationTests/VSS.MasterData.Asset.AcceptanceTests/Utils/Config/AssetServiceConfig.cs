using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Config
{
  public class AssetServiceConfig
  {
    #region Variables
    public static string TestEnv;

    public static string BaseWebAPIUri;
    public static string WebAPIUri;
    public static string WebAPIVersion;
    public static string WebAPIConsumerKey;
    public static string WebAPIConsumerSecret;
    public static Uri DiscoveryServiceURI;


    public static string UserName;
    public static string PassWord;

    public static string accessToken;

    //AssetDevice Search Endpoint
    public static string AssetDeviceListAPI;
    public static string SearchString;
    public static string PageNo;
    public static string PageSize;
    public static string DeviceServiceEndpoint;
    public static string DeviceAssetAssociationEndpoint;
    public static string DeviceAssetDissociationEndpoint;
    public static string AssetSubscriptionEndpoint;
    public static string AssetDetailAPI;
    public static string AssetUID;
    public static string DeviceUID;
    public static string AssetDetailEndpoint;
    public static string MileageTargerAPI;
    public static string FuelBurnRateAPI;
    public static string VolumePerCycleAPI;
    public static string RetrieveVolumePerCycleAPI;

    public static string AssetServiceEndpoint;
    public static string AssetSearchEndpoint;
    public static string AssetServiceTopic;
    public static string AssetServiceKafkaUri;
    public static string WebAPIAsset;
    public static string WebAPIDevice;
    public static string AssetWebAPIUri;
    public static string DeviceWebAPIUri;
    public static string SubscriptionWebAPIUri;
    public static string WebAPIDeviceAssetAssociation;
    public static string WebAPIDeviceAssetDissociation;
    public static string AssetSettingsEndPoint;
    public static string CreateProductivityDetailsEndpoint;
    public static string RetrieveProductivityDetailsEndpoint;
    public static string GetDeviceType;
    public static string DeviceType;

    public static string MakeCodeConsumer;
    public static string MakeCodeEndpoint;

    //MySQL Settings
    public static string MySqlDBServer;
    public static string MySqlDBUsername;
    public static string MySqlDBPassword;
    public static string MySqlDBName;
    public static string MySqlConnection;

    //MySQL Settings
    public static string SFMySqlDBServer;
    public static string SFMySqlDBUsername;
    public static string SFMySqlDBPassword;
    public static string SFMySqlDBName;
    public static string SFMySqlConnection;

    public static string KafkaGroupName;
    public static string KafkaWaitTime;

    public static string CustomerUID;
    public static string CustomerUidHeader;

    #endregion

    #region EnvironmentSetup
    public static void SetupEnvironment()
    {
      string Protocol = "https://";
      TestEnv = ConfigurationManager.AppSettings["TestEnv"];

      string DiscoveryService = ConfigurationManager.AppSettings["DiscoveryService"];
      KafkaGroupName = ConfigurationManager.AppSettings["KafkaGroupName"];
      KafkaWaitTime = ConfigurationManager.AppSettings["KafkaWaitTime"];

      //AssetDevice Search API
      AssetDeviceListAPI = ConfigurationManager.AppSettings["AssetDeviceListAPI"];
      SearchString = ConfigurationManager.AppSettings["SearchString"];
      PageNo = ConfigurationManager.AppSettings["PageNo"];
      PageSize = ConfigurationManager.AppSettings["PageSize"];
      WebAPIDevice = ConfigurationManager.AppSettings["WebAPIDevice"];
      WebAPIDeviceAssetAssociation = ConfigurationManager.AppSettings["DeviceAssetAssociation"];
      WebAPIDeviceAssetDissociation = ConfigurationManager.AppSettings["DeviceAssetDissociation"];
      AssetDetailAPI = ConfigurationManager.AppSettings["AssetDetailAPI"];
      AssetUID = ConfigurationManager.AppSettings["AssetUID"];
      DeviceUID = ConfigurationManager.AppSettings["DeviceUID"];
      DeviceType = ConfigurationManager.AppSettings["DeviceType"];
      CustomerUidHeader = ConfigurationManager.AppSettings["X-VisionLink-CustomerUid"];

      switch (TestEnv)
      {
        case "DEV":
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          AssetWebAPIUri = ConfigurationManager.AppSettings["DevAssetWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["DevWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["DevDeviceWebAPIUri"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["DevSubscriptionWebAPIUri"];

          AssetSettingsEndPoint = ConfigurationManager.AppSettings["AssetSettingsEndPoint"];
          CreateProductivityDetailsEndpoint = AssetSettingsEndPoint + "productivitytargets" + "/" + "Save";
          RetrieveProductivityDetailsEndpoint = AssetSettingsEndPoint + "productivitytargets" + "/" + "retrieve";
          AssetSettingsEndPoint = AssetSettingsEndPoint + "assetsettings";
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-Dev");
          MakeCodeEndpoint= ConfigurationManager.AppSettings["DevMakeCodeEndpoint"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-Dev");
          AssetServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);
          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          DeviceAssetDissociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetDissociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;
          GetDeviceType = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DeviceType;

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];


          SFMySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["SFDevMySqlDBServer"];
          SFMySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["SFDevMySqlDBUsername"];
          SFMySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["SFDevMySqlDBPassword"];
          SFMySqlDBName = System.Configuration.ConfigurationManager.AppSettings["SFDevMySqlDBName"];


          MileageTargerAPI = "https://api-stg.trimble.com/t/trimble.com/vss-dev-assetservice/1.0/assetsettings/mileage/save";
          FuelBurnRateAPI = "https://api-stg.trimble.com/t/trimble.com/vss-dev-assetservice/1.0/assetfuelburnratesettings";
          VolumePerCycleAPI = "https://api-stg.trimble.com/t/trimble.com/vss-dev-assetservice/v1/assetvolumepercyclesettings";
          break;

        case "LOCAL":
          BaseWebAPIUri = ConfigurationManager.AppSettings["LocalBaseWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["LocalWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["LocalWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["LocalWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["LocalWebAPIConsumerSecret"];
          WebAPIDevice = ConfigurationManager.AppSettings["LocalWebAPIDevice"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["LocalSubscriptionWebAPIUri"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-Dev");

          AssetServiceEndpoint = "http://" + BaseWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + WebAPIVersion + "/" + DiscoveryService);

          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-Dev");
          MakeCodeEndpoint = ConfigurationManager.AppSettings["DevMakeCodeEndpoint"];

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBName"];

          break;

        case "IQA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["IQABaseWebAPIUri"];
          AssetWebAPIUri = ConfigurationManager.AppSettings["IQAAssetWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["IQAWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["IQAWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["IQAWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["IQAWebAPIConsumerSecret"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["IQADeviceWebAPIUri"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["IQASubscriptionWebAPIUri"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-IQA");
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-IQA");
          MakeCodeEndpoint = ConfigurationManager.AppSettings["IQAMakeCodeEndpoint"];

          AssetServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          DeviceAssetDissociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetDissociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBName"];

          break;

        case "PERF":
          BaseWebAPIUri = ConfigurationManager.AppSettings["PERFBaseWebAPIUri"];
          AssetWebAPIUri = ConfigurationManager.AppSettings["PERFAssetWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["PERFWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["PERFWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["PERFWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["PERFWebAPIConsumerKey"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["PERFDeviceWebAPIUri"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["PERFSubscriptionWebAPIUri"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-Perf");
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-Perf");
          MakeCodeEndpoint = ConfigurationManager.AppSettings["PerfMakeCodeEndpoint"];

          AssetServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          DeviceAssetDissociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetDissociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBName"];

          break;

        case "ALPHA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["AlphaBaseWebAPIUri"];
          AssetWebAPIUri = ConfigurationManager.AppSettings["AlphaAssetWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["AlphaWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["AlphaWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["AlphaWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["AlphaWebAPIConsumerSecret"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["AlphaDeviceWebAPIUri"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["AlphaSubscriptionWebAPIUri"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-Alpha");
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-Alpha");
          MakeCodeEndpoint = ConfigurationManager.AppSettings["AlphaMakeCodeEndpoint"];

          AssetServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          DeviceAssetDissociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetDissociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;
          FuelBurnRateAPI = System.Configuration.ConfigurationManager.AppSettings["AlphaFuelBurntRate"];
          UserName = System.Configuration.ConfigurationManager.AppSettings["AlphaUserName"];
          PassWord = System.Configuration.ConfigurationManager.AppSettings["AlphaPassword"];
          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBName"];
          CustomerUID = System.Configuration.ConfigurationManager.AppSettings["AlphaCustomerUID"];
          break;

        default: //Default is Dev Environment
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          AssetWebAPIUri = ConfigurationManager.AppSettings["DevAssetWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIAsset = ConfigurationManager.AppSettings["DevWebAPIAsset"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
          WebAPIDevice = ConfigurationManager.AppSettings["DevWebAPIDevice"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["DevDeviceWebAPIUri"];
          SubscriptionWebAPIUri = ConfigurationManager.AppSettings["DevSubscriptionWebAPIUri"];

          AssetServiceTopic = String.Concat(ConfigurationManager.AppSettings["AssetServiceTopic"], "-Dev");
          MakeCodeConsumer = string.Concat(ConfigurationManager.AppSettings["MakeCodeConsumer"], "-Dev");
          MakeCodeEndpoint = ConfigurationManager.AppSettings["DevMakeCodeEndpoint"];

          AssetServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AssetSearchEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDeviceListAPI;
          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;
          DeviceAssetAssociationEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIDeviceAssetAssociation;
          AssetSubscriptionEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + SubscriptionWebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAsset;
          AssetDetailEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + AssetWebAPIUri + "/" + WebAPIVersion + "/" + AssetDetailAPI;

          //Protocol+BaseWebAPIUri+"/t/trimble.com/"+MasterDataAssetAPI+Version+MileageTargetAPI;

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;
      }

      accessToken = GetValidUserAccessToken();
      AssetServiceKafkaUri = GetKafkaEndPointURL(AssetServiceTopic);
      MySqlConnection = "server=" + MySqlDBServer + ";user id=" + MySqlDBUsername + ";password=" + MySqlDBPassword + ";database=" + MySqlDBName;
      SFMySqlConnection = "server=" + SFMySqlDBServer + ";user id=" + SFMySqlDBUsername + ";password=" + SFMySqlDBPassword + ";database=";

    }
    #endregion

    #region Kafka ServerEndpoint
    private static string GetKafkaEndPointURL(string kafkatopicName)
    {
      try
      {
        string response = RestClientUtil.DoHttpRequest(AssetServiceEndpoint.ToString(), HeaderSettings.GetMethod, accessToken,
          HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
        JObject jsonObj = JObject.Parse(response);
        var token = jsonObj.SelectToken("$.Topics[?(@.Name == '" + kafkatopicName + "')].URL");
        return token.ToString();
      }
      catch (Exception)
      {

      }
      return null;
    }
    #endregion

    #region Access Token
    public static string GetValidUserAccessToken()
    {
      string accessToken = string.Empty;
      TPaaSServicesConfig.SetupEnvironment();
     // string userTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, UserName, PassWord);
      accessToken = TokenService.GetAccessToken(TPaaSServicesConfig.TPaaSTokenEndpoint, AssetServiceConfig.WebAPIConsumerKey, AssetServiceConfig.WebAPIConsumerSecret);

      return accessToken;
    }
    #endregion

  }
}



