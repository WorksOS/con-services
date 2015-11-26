using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.KafkaWrapper;
using VSS.KafkaWrapper.Models;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.VisionLink.MasterData.AcceptanceTests.Resources;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.SubscriptionService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.SubscriptionService
{
  class SubscriptionServiceSupport
  {
    public CreateSubscriptionEvent CreateSubscriptionModel = new CreateSubscriptionEvent();
    public UpdateSubscriptionEvent UpdateSubscriptionModel = new UpdateSubscriptionEvent();
    public InvalidCreateSubscriptionEvent InvalidCreateSubscriptionModel = new InvalidCreateSubscriptionEvent();
    public InvalidUpdateSubscriptionEvent InvalidUpdateSubscriptionModel = new InvalidUpdateSubscriptionEvent();
    private static Log4Net Log = new Log4Net(typeof(SubscriptionServiceSupport));

    public string ResponseString = string.Empty;
    public ConsumerWrapper _consumerWrapper;
    public static string UserName;
    public static string PassWord;
    public CheckForSubscriptionCreateHandler _checkForSubscriptionCreateHandler;
    public CheckForSubscriptionUpdateHandler _checkForSubscriptionUpdateHandler;

    #region Constructors

    public SubscriptionServiceSupport(Log4Net myLog)
    {
      MasterDataConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateSubscriptionModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
        throw new Exception(e + " Got Error While Posting Data To Subscription Service");
      }
    }

    public void PostValidUpdateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(UpdateSubscriptionModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
        throw new Exception(e + " Got Error While Posting Data To Subscription Service");
      }
    }

    public void PostInValidCreateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidCreateSubscriptionModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
        throw new Exception(e + " Got Error While Posting Data To Subscription Service");
      }
    }

    public void PostInValidUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidUpdateSubscriptionModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
        throw new Exception(e + " Got Error While Posting Data To Subscription Service");
      }
    }
    #endregion

    #region Response Verification

    public void VerifySubscriptionServiceCreateResponse(CreateSubscriptionModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.CreateSubscriptionEvent.SubscriptionUID != null)
          Assert.AreEqual(CreateSubscriptionModel.SubscriptionUID, kafkaresponse.CreateSubscriptionEvent.SubscriptionUID);
        if (kafkaresponse.CreateSubscriptionEvent.CustomerUID != null)
          Assert.AreEqual(CreateSubscriptionModel.CustomerUID, kafkaresponse.CreateSubscriptionEvent.CustomerUID);
        if (kafkaresponse.CreateSubscriptionEvent.AssetUID != null)
          Assert.AreEqual(CreateSubscriptionModel.AssetUID, kafkaresponse.CreateSubscriptionEvent.AssetUID);
        if (kafkaresponse.CreateSubscriptionEvent.SubscriptionTypeID != null)
          Assert.AreEqual(CreateSubscriptionModel.SubscriptionTypeID, kafkaresponse.CreateSubscriptionEvent.SubscriptionTypeID);
        if (kafkaresponse.CreateSubscriptionEvent.StartDate != null)
          Assert.AreEqual(CreateSubscriptionModel.StartDate, kafkaresponse.CreateSubscriptionEvent.StartDate);
        if (kafkaresponse.CreateSubscriptionEvent.EndDate != null)
          Assert.AreEqual(CreateSubscriptionModel.EndDate, kafkaresponse.CreateSubscriptionEvent.EndDate);
        if (kafkaresponse.CreateSubscriptionEvent.ActionUTC != null)
          Assert.AreEqual(CreateSubscriptionModel.ActionUTC, kafkaresponse.CreateSubscriptionEvent.ActionUTC);
      }
    }

    public void VerifySubscriptionServiceUpdateResponse(UpdateSubscriptionModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.UpdateSubscriptionEvent.SubscriptionUID != null)
          Assert.AreEqual(UpdateSubscriptionModel.SubscriptionUID, kafkaresponse.UpdateSubscriptionEvent.SubscriptionUID);
        if (kafkaresponse.UpdateSubscriptionEvent.CustomerUID != null)
          Assert.AreEqual(UpdateSubscriptionModel.CustomerUID, kafkaresponse.UpdateSubscriptionEvent.CustomerUID);
        if (kafkaresponse.UpdateSubscriptionEvent.AssetUID != null)
          Assert.AreEqual(UpdateSubscriptionModel.AssetUID, kafkaresponse.UpdateSubscriptionEvent.AssetUID);
        if (kafkaresponse.UpdateSubscriptionEvent.SubscriptionTypeID != null)
          Assert.AreEqual(UpdateSubscriptionModel.SubscriptionTypeID, kafkaresponse.UpdateSubscriptionEvent.SubscriptionTypeID);
        if (kafkaresponse.UpdateSubscriptionEvent.StartDate != null)
          Assert.AreEqual(UpdateSubscriptionModel.StartDate, kafkaresponse.UpdateSubscriptionEvent.StartDate);
        if (kafkaresponse.UpdateSubscriptionEvent.EndDate != null)
          Assert.AreEqual(UpdateSubscriptionModel.EndDate, kafkaresponse.UpdateSubscriptionEvent.EndDate);
        if (kafkaresponse.UpdateSubscriptionEvent.ActionUTC != null)
          Assert.AreEqual(UpdateSubscriptionModel.ActionUTC, kafkaresponse.UpdateSubscriptionEvent.ActionUTC);
      }
    }
    #endregion

    #region ErrorResponse Verification
    public void VerifyErrorResponse(string ErrorMessage)
    {
      try
      {
        SubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<SubscriptionServiceErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.SubscriptionUID != null)
            Assert.AreEqual(resourceError, error.ModelState.SubscriptionUID[0].ToString());
          else if (error.ModelState.CustomerUID != null)
            Assert.AreEqual(resourceError, error.ModelState.CustomerUID[0].ToString());
          else if (error.ModelState.AssetUID != null)
            Assert.AreEqual(resourceError, error.ModelState.AssetUID[0].ToString());
          else if (error.ModelState.SubscriptionTypeID != null)
            Assert.AreEqual(resourceError, error.ModelState.SubscriptionTypeID[0].ToString());
          else if (error.ModelState.StartDate != null)
            Assert.AreEqual(resourceError, error.ModelState.StartDate[0].ToString());
          else if (error.ModelState.EndDate != null)
            Assert.AreEqual(resourceError, error.ModelState.EndDate[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
          else
            Assert.AreEqual(MasterDataMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
        }
        else
          Assert.AreEqual(MasterDataMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
        throw new Exception("Got Error While DeSerializing JSON Object");
      }
    }
    #endregion

    public void SetupCreateSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, Guid CustomerUidToLookFor, DateTime actionUtc)
    {
      _checkForSubscriptionCreateHandler = new CheckForSubscriptionCreateHandler(SubscriptionUidToLookFor, CustomerUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForSubscriptionCreateHandler);
    }

    public void SetupUpdateSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, Guid CustomerUidToLookFor, DateTime actionUtc)
    {
      _checkForSubscriptionUpdateHandler = new CheckForSubscriptionUpdateHandler(SubscriptionUidToLookFor, CustomerUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForSubscriptionUpdateHandler);
    }

    private void SubscribeAndConsumeFromKafka(CheckForSubscriptionHandler SubscriptionHandler)
    {
      var eventAggregator = new EventAggregator();
      eventAggregator.Subscribe(SubscriptionHandler);
      _consumerWrapper = new ConsumerWrapper(eventAggregator,
          new KafkaConsumerParams("SubscriptionServiceAcceptanceTest", MasterDataConfig.SubscriptionServiceKafkaUri,
              MasterDataConfig.SubscriptionServiceTopic));
      //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
      Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
      Thread.Sleep(new TimeSpan(0, 0, 10));
    }
  }
}
