using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Models;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using AutomationCore.Shared.Library;
using VSS.MasterData.Asset.AcceptanceTests.Utils.DBQueries;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.MakeCode;
using AutomationCore.API.Framework.Library;
using AutomationCore.API.Framework.Common;
using System.Net;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.MakeCode
{
  [Binding]
  class MakeCodeEndpointSteps
  {
    public static Log4Net Log = new Log4Net(typeof(MakeCodeEndpointSteps));

    public static Make MakeCodeModel = new Make();
    public static CreateMakeCOde CreateMakeEvent = new CreateMakeCOde();

    public string Topic;
    public string Payload;

    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    Random rand = new Random();

    public string ResponseString = string.Empty;
    public static string responseString;
    public static List<MakeCodeResponse> MakeCodeAPIResponse = new List<MakeCodeResponse>();
    public MakeCodeEndpointSteps()
    {

      AssetServiceConfig.SetupEnvironment();
    }

    public void SetResponseString()
    {
      responseString= "[{\"code\":\"1HA\",\"name\":\"HAMMEL\"},{\"code\":\"A07\",\"name\":\"AUSTIN WESTERN\"}]";
    }

    [Given(@"'(.*)' Is Ready to Verify '(.*)'")]
    public void GivenIsReadyToVerify(string p0, string p1)
    {
      
    }

    public string GenerateMakeCode(int size)
    {
      char[] chars = new char[size];
      for (int i = 0; i < size; i++)
      {
        chars[i] = Alphabet[rand.Next(Alphabet.Length)];
      }
      return new string(chars);
    }

    public void SetupCreateMakeCodeRequest()
    {
      
      MakeCodeModel.MakeCode = GenerateMakeCode(3);
      MakeCodeModel.MakeDesc = "Make";
      MakeCodeModel.MakeUID = Guid.NewGuid();
      MakeCodeModel.ActionUTC= DateTime.UtcNow;
      MakeCodeModel.ReceivedUTC= DateTime.UtcNow;
      CreateMakeEvent.CreateMakeEvent = MakeCodeModel;
    }


    [Given(@"I Perform Create New MakeCode")]
    public void GivenIPerformCreateNewMakeCode()
    {
      Topic = AssetServiceConfig.MakeCodeConsumer;
      SetupCreateMakeCodeRequest();
      Payload = JsonConvert.SerializeObject(CreateMakeEvent, Formatting.Indented, new StringEnumConverter());
      ProduceKafkaMessage(Topic);

    }

    public void ProduceKafkaMessage(string topic)
    {
      Console.WriteLine("Topic to publish : " + topic);
      KafkaServicesConfig.InitializeKafkaProducer(topic);

      try
      {
        if (MakeCodeModel == null)
        {
          LogResult.Report(Log, "log_ForInfo", topic + ": MakeCode Model for payload is null");
          throw new Exception("MakeCode Model is null");
        }
        else
        {
          if (MakeCodeModel.MakeUID == null)
          {
            LogResult.Report(Log, "log_ForInfo", topic + ": MakeCodeUID for payload is null");
            throw new Exception("MakeCodeUid is null");
          }
          else
          {
            Console.WriteLine(topic + ": Asset Details OK, proceed publish.");
            LogResult.Report(Log, "log_ForInfo", topic + ": Asset Details OK, proceed publish.");
          }
        }

        //else
        //  Console.WriteLine("Asset detail not null"+AssetDetail);        
        //else
        //  Console.WriteLine("Asset detail uid not null",AssetDetail.AssetUid);

        KafkaServicesConfig.ProduceMessage(Payload, MakeCodeModel?.MakeUID.ToString());
      }
      catch (Exception e)
      {
        throw new Exception("Unable To publish in kafka", e);
      }
    }

    [When(@"I Hit MakeCode Endpoint")]
    public void WhenIHitMakeCodeEndpoint()
    {

      try
      {
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();

        //   public static string DoHttpRequest(string resourceUri, string httpMethod,
        //string mediaType, string payloadData, HttpStatusCode httpResponseCode)
        ResponseString = RestClientUtil.DoHttpRequestWithNoBody(AssetServiceConfig.MakeCodeEndpoint, HeaderSettings.GetMethod,accessToken,
                          HeaderSettings.JsonMediaType, HttpStatusCode.OK);

         //DoHttpRequestWithNoBody(string resourceUri, string httpMethod, string authKey, string mediaType, HttpStatusCode httpResponseCode, string authKeyType = null,
         //                                               string contentType = null, Dictionary<string, string> customHeaders = null)
        MakeCodeAPIResponse= JsonConvert.DeserializeObject<List<MakeCodeResponse>>(ResponseString);
      }
      catch (Exception e)
      {
        throw new Exception("Unable To Perform Get MakeCode", e);
      }

      }


    [Then(@"The MakeCode Endpoint Should Return Valid Response")]
    public void ThenTheMakeCodeEndpointShouldReturnValidResponse()
    {
      string query;

      //SetResponseString();
      //var obj = JsonConvert.DeserializeObject<List<MakeCodeResponse>>(responseString);
      query = MakeCodeDBQueries.makeCodeDBQuery;

      MakeCodeEndpointSupport.FetchMakeCodeMsgDetailsFromDB(query);

    }



  }
}
