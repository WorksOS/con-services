using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config
{
  public class MasterDataConfig
  {
    public static string VSSTestEnv;

    public static string AssetServiceBaseUri;
    public static string GroupServiceBaseUri;
    public static string GeofenceServiceBaseUri;
    public static string CustomerServiceBaseUri;
    public static string SubscriptionServiceBaseUri;
    public static string AssetServiceEndpoint;
    public static string GroupServiceEndpoint;
    public static string GeofenceServiceEndpoint;
    public static string CustomerServiceEndpoint;
    public static string SubscriptionServiceEndpoint;
    public static string KafkaUri;
    public static string KafkaEndpoint;
    public static string AssetServiceTopic;
    public static string AssetServiceKey;
    public static string AssetServiceKafkaUri;
    public static string GeofenceServiceKafkaUri;
    public static string CustomerServiceKafkaUri;
    public static string GroupServiceKafkaUri;
    public static string SubscriptionServiceKafkaUri;
    public static string AssetUri;
    public static string GeofenceServiceKey;
    public static string GroupServiceKey;
    public static string CustomerServiceKey;
    public static string GeofenceServiceTopic;
    public static string SubscriptionServiceKey;
    public static string GroupUri;
    public static string CustomerUri;
    public static string GroupServiceTopic;
    public static string CustomerServiceTopic;
    public static string SubscriptionServiceTopic;

    public static void SetupEnvironment()
    {
      string Protocol = "http://";
      VSSTestEnv = ConfigurationManager.AppSettings["VSSTestEnv"];
      AssetServiceKey = ConfigurationManager.AppSettings["AssetServiceKey"];
      KafkaUri = ConfigurationManager.AppSettings["KafkaUri"];
      AssetUri = ConfigurationManager.AppSettings["AssetUri"];
      GeofenceServiceKey = ConfigurationManager.AppSettings["GeofenceServiceKey"];
      CustomerServiceKey = ConfigurationManager.AppSettings["CustomerServiceKey"];
      GroupServiceKey = ConfigurationManager.AppSettings["GroupServiceKey"];
      SubscriptionServiceKey = ConfigurationManager.AppSettings["SubscriptionServiceKey"];
      GroupUri = ConfigurationManager.AppSettings["GroupUri"];


      var disvoveryServiceURI = new Uri(ConfigurationManager.AppSettings["DiscoveryURI"]);
      AssetServiceKafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, AssetServiceKey);
      GroupServiceKafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, GroupServiceKey);
      GeofenceServiceKafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, GeofenceServiceKey);
      CustomerServiceKafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, CustomerServiceKey);
      SubscriptionServiceKafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, SubscriptionServiceKey);

      AssetServiceTopic = GetKafkaTopicName(disvoveryServiceURI, AssetServiceKey);
      GeofenceServiceTopic = GetKafkaTopicName(disvoveryServiceURI, GeofenceServiceKey);
      CustomerServiceTopic = GetKafkaTopicName(disvoveryServiceURI, CustomerServiceKey);
      GroupServiceTopic = GetKafkaTopicName(disvoveryServiceURI, GroupServiceKey);
      SubscriptionServiceTopic = GetKafkaTopicName(disvoveryServiceURI, SubscriptionServiceKey);

      switch (VSSTestEnv)
      {
        case "Dev":
          AssetServiceBaseUri = ConfigurationManager.AppSettings["DevAssetServiceBaseUri"];
          GroupServiceBaseUri = ConfigurationManager.AppSettings["DevGroupServiceBaseUri"];
          GeofenceServiceBaseUri = ConfigurationManager.AppSettings["DevGeofenceServiceBaseUri"];
          CustomerServiceBaseUri = ConfigurationManager.AppSettings["DevCustomerServiceBaseUri"];
          SubscriptionServiceBaseUri = ConfigurationManager.AppSettings["DevSubscriptionServiceBaseUri"];
          AssetServiceEndpoint = Protocol + AssetServiceBaseUri + "/" + "AssetService" + "/" + "v1";
          GroupServiceEndpoint = Protocol + GroupServiceBaseUri + "/" + "GroupService" + "/" + "v1";
          GeofenceServiceEndpoint = Protocol + GeofenceServiceBaseUri + "/" + "GeofenceService" + "/" + "v1";
          CustomerServiceEndpoint = Protocol + CustomerServiceBaseUri + "/" + "CustomerService" + "/" + "v1";
          SubscriptionServiceEndpoint = Protocol + SubscriptionServiceBaseUri + "/" + "SubscriptionService" + "/" + "v1";
          break;

        case "LOCAL":
          AssetServiceBaseUri = ConfigurationManager.AppSettings["LocalAssetServiceBaseUri"];
          GroupServiceBaseUri = ConfigurationManager.AppSettings["LocalGroupServiceBaseUri"];
          GeofenceServiceBaseUri = ConfigurationManager.AppSettings["LocalGeofenceServiceBaseUri"];
          CustomerServiceBaseUri = ConfigurationManager.AppSettings["LocalCustomerServiceBaseUri"];
          SubscriptionServiceBaseUri = ConfigurationManager.AppSettings["LocalSubscriptionServiceBaseUri"];
          AssetServiceEndpoint = Protocol + AssetServiceBaseUri  + "/" + "v1";
          GroupServiceEndpoint = Protocol + GroupServiceBaseUri  + "/" + "v1";
          GeofenceServiceEndpoint = Protocol + GeofenceServiceBaseUri + "/"  + "v1";
          CustomerServiceEndpoint = Protocol + CustomerServiceBaseUri + "/"  + "v1";
          SubscriptionServiceEndpoint = Protocol + SubscriptionServiceBaseUri + "/" + "v1";
          break;

        default: //Default is Dev Environment
          AssetServiceBaseUri = ConfigurationManager.AppSettings["DevAssetServiceBaseUri"];
          GroupServiceBaseUri = ConfigurationManager.AppSettings["DevGroupServiceBaseUri"];
          GeofenceServiceBaseUri = ConfigurationManager.AppSettings["DevGeofenceServiceBaseUri"];
          CustomerServiceBaseUri = ConfigurationManager.AppSettings["DevCustomerServiceBaseUri"];
          SubscriptionServiceBaseUri = ConfigurationManager.AppSettings["DevSubscriptionServiceBaseUri"];
          AssetServiceEndpoint = Protocol + AssetServiceBaseUri + "/" + "AssetService" + "/" + "v1";
          GroupServiceEndpoint = Protocol + GroupServiceBaseUri + "/" + "GroupService" + "/" + "v1";
          GeofenceServiceEndpoint = Protocol + GeofenceServiceBaseUri + "/" + "GeofenceService" + "/" + "v1";
          CustomerServiceEndpoint = Protocol + CustomerServiceBaseUri + "/" + "CustomerService" + "/" + "v1";
          SubscriptionServiceEndpoint = Protocol + SubscriptionServiceBaseUri + "/" + "SubscriptionService" + "/" + "v1";
          break;
      }
      KafkaEndpoint = Protocol + KafkaUri;

    }

    private static string GetKafkaEndPointURL(Uri _url, string defaultKey)
    {
      try
      {
        string jsonStr;
        using (var wc = new WebClient())
        {
          jsonStr = wc.DownloadString(_url);
        }
        JObject jsonObj = JObject.Parse(jsonStr);
        var token = jsonObj.SelectToken("$.Topics[?(@.DefaultKey == '" + defaultKey + "')].URL");

        return token.ToString();
      }
      catch (Exception)
      {

      }
      return null;
    }

    private static string GetKafkaTopicName(Uri _url, string defaultKey)
    {
      try
      {
        string jsonStr;
        using (var wc = new WebClient())
        {
          jsonStr = wc.DownloadString(_url);
        }
        JObject jsonObj = JObject.Parse(jsonStr);

        var token = jsonObj.SelectToken("$.Topics[?(@.DefaultKey == '" + defaultKey + "')].Name");
        return token.ToString();
      }
      catch (Exception)
      {

      }
      return null;
    }

  }
}



