using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Library;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Config
{
  public class CustomerServiceConfig
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

    public static string CustomerServiceEndpoint;
    public static string CustomerServiceTopic;
    public static string CustomerServiceKafkaUri;

    //MySQL Settings
    public static string MySqlDBServer;
    public static string MySqlDBUsername;
    public static string MySqlDBPassword;
    public static string MySqlDBName;
    public static string MySqlConnection;

    public static string KafkaGroupName;
    public static string KafkaWaitTime;

    public static string AccountHierarchyBaseURI;
    public static string AccountHierarchyByUserUIDEndPoint;
    public static string AccountHierarchyByCustomerUIDEndPoint;
    public static string AccountHierarchyWebAPIVersion;
    public static string AccountHierarchyService;

    #endregion

    #region EnvironmentSetup
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
          WebAPIUri = ConfigurationManager.AppSettings["DevCustomerWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-Dev");

          CustomerServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

            MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];


          break;

        case "LOCAL":
          BaseWebAPIUri = ConfigurationManager.AppSettings["LocalBaseWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["LocalWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["LocalWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["LocalWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-Dev");

          CustomerServiceEndpoint = "http://" + BaseWebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["LocalMySqlDBName"];

          break;

        case "IQA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["IQABaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["IQACustomerWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["IQAWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["IQAWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["IQAWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-IQA");

          CustomerServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["IQAMySqlDBName"];

          break;

        case "PERF":
          BaseWebAPIUri = ConfigurationManager.AppSettings["PERFBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["PERFCustomerWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["PERFWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["PERFWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["PERFWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-Perf");

          CustomerServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["PERFMySqlDBName"];

          break;

        case "ALPHA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["AlphaBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["AlphaCustomerWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["AlphaWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["AlphaWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["AlphaWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-Alpha");

          CustomerServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI +"/"+ WebAPIVersion +"/"+ AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["AlphaMySqlDBName"];

          break;

        default: //Default is Dev Environment
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          WebAPIUri = ConfigurationManager.AppSettings["DevCustomerWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          CustomerServiceTopic = String.Concat(ConfigurationManager.AppSettings["CustomerServiceTopic"], "-Dev");

          CustomerServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion;
          DiscoveryServiceURI = new Uri(Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" + WebAPIVersion + "/" + DiscoveryService);

          AccountHierarchyBaseURI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri;
          AccountHierarchyService = ConfigurationManager.AppSettings["AccountHierarchyService"];
          AccountHierarchyByUserUIDEndPoint = AccountHierarchyBaseURI + WebAPIUri + WebAPIVersion + AccountHierarchyService + "?topLevelsOnly=true";
          AccountHierarchyByCustomerUIDEndPoint = AccountHierarchyBaseURI + "/" + WebAPIVersion + "/" + AccountHierarchyService + "?targetcustomeruid =";

          MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;
      }

      accessToken = GetValidUserAccessToken();
      CustomerServiceKafkaUri = GetKafkaEndPointURL(CustomerServiceTopic);
      MySqlConnection = "server=" + MySqlDBServer + ";user id=" + MySqlDBUsername + ";password=" + MySqlDBPassword + ";database="+MySqlDBName;

    }
    #endregion

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
      accessToken = TokenService.GetAccessToken(userTokenEndpoint, CustomerServiceConfig.WebAPIConsumerKey, CustomerServiceConfig.WebAPIConsumerSecret);

      return accessToken;
    }
    #endregion

  }
}



