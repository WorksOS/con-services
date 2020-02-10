using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Config
{
  public class SubscriptionServiceConfig
  {
    #region Variables
    public static string TestEnv;

    public static string BaseWebAPIUri;
    public static string WebAPIUri;
    public static string WebAPIVersion;
    public static string WebAPIConsumerKey;
    public static string WebAPIConsumerSecret;
    public static Uri DiscoveryServiceURI;

    public static string UserName = "pub-vssadmin@trimble.com";
    public static string PassWord = "VisionLink@2015";

    public static string accessToken;

    public static string AssetSubscriptionServiceEndpoint;
    public static string CustomerSubscriptionServiceEndpoint;
    public static string ProjectSubscriptionServiceEndpoint;
    public static string WebAPIAssetSubscription;
    public static string WebAPICustomerSubscription;
    public static string WebAPIProjectSubscription;
    public static string SubscriptionServiceTopic;
    public static string SubscriptionServiceKafkaUri;

    //MySQL Settings
    public static string MySqlDBServer;
    public static string MySqlDBUsername;
    public static string MySqlDBPassword;
    public static string MySqlDBName;
    public static string MySqlConnection;

    public static string KafkaGroupName;
    public static string KafkaWaitTime;

    #endregion

    public static void SetupEnvironment()
    {
      string Protocol = "https://";
      TestEnv = ConfigurationManager.AppSettings["TestEnv"];

      string DiscoveryService = ConfigurationManager.AppSettings["DiscoveryService"];
      KafkaGroupName = ConfigurationManager.AppSettings["KafkaGroupName"];
      KafkaWaitTime = ConfigurationManager.AppSettings["KafkaWaitTime"];

      switch (TestEnv)
      {
        case "DEV":
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["DevSubscriptionWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["DevWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["DevWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["DevWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-Dev");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;

        case "LOCAL":
          BaseWebAPIUri = ConfigurationManager.AppSettings["LocalBaseWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["LocalWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["LocalWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["LocalWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["LocalWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["LocalWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["LocalWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-Dev");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBName"];

          break;

        case "IQA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["IQABaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["IQASubscriptionWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["IQAWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["IQAWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["IQAWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["IQAWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["IQAWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["IQAWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-IQA");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBName"];

          break;

        case "PERF":
          BaseWebAPIUri = ConfigurationManager.AppSettings["PERFBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["PERFSubscriptionWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["PERFWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["PERFWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["PERFWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["PERFWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["PERFWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["PERFWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-Perf");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBName"];

          break;

        case "ALPHA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["ALPHABaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["ALPHASubscriptionWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["ALPHAWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["ALPHAWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["ALPHAWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["ALPHAWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["ALPHAWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["ALPHAWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-Alpha");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["ALPHAMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["ALPHAMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["ALPHAMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["ALPHAMySqlDBName"];

          break;

        default: //Default is Dev Environment
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["DevSubscriptionWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIAssetSubscription = ConfigurationManager.AppSettings["DevWebAPIAssetSubscription"];
          WebAPICustomerSubscription = ConfigurationManager.AppSettings["DevWebAPICustomerSubscription"];
          WebAPIProjectSubscription = ConfigurationManager.AppSettings["DevWebAPIProjectSubscription"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          SubscriptionServiceTopic = String.Concat(ConfigurationManager.AppSettings["SubscriptionServiceTopic"], "-Dev");

          AssetSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIAssetSubscription;
          CustomerSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPICustomerSubscription;
          ProjectSubscriptionServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + WebAPIProjectSubscription;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;
      }
      accessToken = GetValidUserAccessToken();
      SubscriptionServiceKafkaUri = GetKafkaEndPointURL(SubscriptionServiceTopic);
      MySqlConnection = "server=" + MySqlDBServer + ";user id=" + MySqlDBUsername + ";password=" + MySqlDBPassword + ";database=" + MySqlDBName;
    }

    #region Kafka ServerEndpoint
    private static string GetKafkaEndPointURL(string kafkatopicName)
    {
      try
      {
        string response = RestClientUtil.DoHttpRequest(DiscoveryServiceURI.ToString(), HeaderSettings.GetMethod, accessToken,
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
      string userTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, UserName, PassWord);
      accessToken = TokenService.GetAccessToken(userTokenEndpoint, SubscriptionServiceConfig.WebAPIConsumerKey, SubscriptionServiceConfig.WebAPIConsumerSecret);

      return accessToken;
    }
    #endregion
  }
}



