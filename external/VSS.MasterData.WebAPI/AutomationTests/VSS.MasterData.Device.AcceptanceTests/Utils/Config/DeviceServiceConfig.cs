using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.Config
{
  public class DeviceServiceConfig
  {

    #region Variables
    public static string TestEnv;

    public static string BaseWebAPIUri;
    public static string WebAPIUri;
    public static string WebAPIVersion;
    public static string WebAPIConsumerKey;
    public static string WebAPIConsumerSecret;

    public static string UserName = "pub-vssadmin@trimble.com";
    public static string PassWord = "VisionLink@2015";

    public static string accessToken;

    public static string DeviceServiceEndpoint;
    public static string DeviceServiceTopic;
    public static string DeviceServiceKafkaUri;
    public static string WebAPIDevice;
    public static string DeviceWebAPIUri;

    public static string DeviceDetailKafkaTopic;
    public static string DeviceFirmwareKafkaTopic;

    //MySQL Settings
    public static string MySqlDBServer;
    public static string MySqlDBUsername;
    public static string MySqlDBPassword;
    public static string MySqlDBName;
    public static string MySqlConnection;

    public static string KafkaGroupName;
    public static string KafkaWaitTime;



    #endregion

    #region EnvironmentSetup
    public static void SetupEnvironment()
    {
      string Protocol = "https://";
      TestEnv = ConfigurationManager.AppSettings["TestEnv"];

      KafkaGroupName = ConfigurationManager.AppSettings["KafkaGroupName"];
      KafkaWaitTime = ConfigurationManager.AppSettings["KafkaWaitTime"];


      switch (TestEnv)
      {
        case "DEV":
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["DevDeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["DevWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-Dev");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-Dev";

          DeviceServiceKafkaUri = ConfigurationManager.AppSettings["DevKafkaUri"];

          MySqlDBServer = ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;

        case "LOCAL":
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["DevDeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["DevWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-Dev");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-Dev";

          DeviceServiceKafkaUri = ConfigurationManager.AppSettings["DevKafkaUri"];

          MySqlDBServer = ConfigurationManager.AppSettings["LocalMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["LocalMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["LocalMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["LocalMySqlDBName"];

          break;

        case "IQA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["IQABaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["IQADeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["IQAWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["IQAWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["IQAWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["IQAWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-IQA");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-IQA";

          MySqlDBServer = ConfigurationManager.AppSettings["IQAMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["IQAMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["IQAMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["IQAMySqlDBName"];

          break;

        case "PERF":
          BaseWebAPIUri = ConfigurationManager.AppSettings["PERFBaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["PERFDeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["PERFWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["PERFWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["PERFWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["PERFWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-PERF");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-Perf";

          MySqlDBServer = ConfigurationManager.AppSettings["PERFMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["PERFMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["PERFMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["PERFMySqlDBName"];

          break;

        case "ALPHA":
          BaseWebAPIUri = ConfigurationManager.AppSettings["AlphaBaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["AlphaDeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["AlphaWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["AlphaWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["AlphaWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["AlphaWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-Alpha");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-Alpha";

          DeviceServiceKafkaUri = ConfigurationManager.AppSettings["AlphaKafkaUri"];

          MySqlDBServer = ConfigurationManager.AppSettings["AlphaMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["AlphaMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["AlphaMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["AlphaMySqlDBName"];

          break;

        default: //Default is Dev Environment
          BaseWebAPIUri = ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
          DeviceWebAPIUri = ConfigurationManager.AppSettings["DevDeviceWebAPIUri"];
          WebAPIVersion = ConfigurationManager.AppSettings["DevWebAPIVersion"];
          WebAPIDevice = ConfigurationManager.AppSettings["DevWebAPIDevice"];
          WebAPIConsumerKey = ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
          WebAPIConsumerSecret = ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];

          DeviceServiceTopic = String.Concat(ConfigurationManager.AppSettings["DeviceServiceTopic"], "-Dev");

          DeviceServiceEndpoint = Protocol + BaseWebAPIUri + "/t/trimble.com/" + DeviceWebAPIUri + "/" + WebAPIVersion;

          DeviceDetailKafkaTopic = ConfigurationManager.AppSettings["DeviceDetail_KafkaTopic"] + "-Dev";

          MySqlDBServer = ConfigurationManager.AppSettings["DevMySqlDBServer"];
          MySqlDBUsername = ConfigurationManager.AppSettings["DevMySqlDBUsername"];
          MySqlDBPassword = ConfigurationManager.AppSettings["DevMySqlDBPassword"];
          MySqlDBName = ConfigurationManager.AppSettings["DevMySqlDBName"];

          break;
      }

      accessToken = GetValidUserAccessToken();
      MySqlConnection = "server=" + MySqlDBServer + ";user id=" + MySqlDBUsername + ";password=" + MySqlDBPassword + ";database=" + MySqlDBName;
    }
    #endregion

    #region Access Token
    public static string GetValidUserAccessToken()
    {
      string accessToken = string.Empty;
      TPaaSServicesConfig.SetupEnvironment();
      string userTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, UserName, PassWord);
      accessToken = TokenService.GetAccessToken(userTokenEndpoint, WebAPIConsumerKey, WebAPIConsumerSecret);

      return accessToken;
    }
    #endregion
  }
}
