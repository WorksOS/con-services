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
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerUserService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerUserService
{
  class CustomerUserServiceSupport
  {
    private static Log4Net Log = new Log4Net(typeof(CustomerUserServiceSupport));

    public AssociateCustomerUserEvent AssociateCustomerUserModel = new AssociateCustomerUserEvent();
    public DissociateCustomerUserEvent DissociateCustomerUserModel = new DissociateCustomerUserEvent();
    public InvalidAssociateCustomerUserEvent InvalidAssociateCustomerUserModel = new InvalidAssociateCustomerUserEvent();
    public InvalidDissociateCustomerUserEvent InvalidDissociateCustomerUserModel = new InvalidDissociateCustomerUserEvent();

    public string ResponseString = string.Empty;
    public ConsumerWrapper _consumerWrapper;
    public static string UserName;
    public static string PassWord;
    public CheckForCustomerUserAssociateHandler _checkForCustomerUserAssociateHandler;
    public CheckForCustomerUserDissociateHandler _checkForCustomerUserDissociateHandler;

    #region Constructors

    public CustomerUserServiceSupport(Log4Net myLog)
    {
      MasterDataConfig.SetupEnvironment();
      Log = myLog;
    }
    #endregion

    #region Post Methods

    public void PostValidCustomerUserAssociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(AssociateCustomerUserModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/AssociateCustomerUser", HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostValidCustomerUserDissociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(DissociateCustomerUserModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/DissociateCustomerUser", HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostInValidCustomerUserAssociateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidAssociateCustomerUserModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/AssociateCustomerUser", HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostInValidCustomerUserDissociateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidDissociateCustomerUserModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/DissociateCustomerUser", HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }
    #endregion

    #region ErrorResponse Verification
    public void VerifyErrorResponse(string ErrorMessage)
    {
      try
      {
        CustomerUserServiceErrorResponseModel error = JsonConvert.DeserializeObject<CustomerUserServiceErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.RelationType != null)
            Assert.AreEqual(resourceError, error.ModelState.RelationType[0].ToString());
          else if (error.ModelState.UserUID != null)
            Assert.AreEqual(resourceError, error.ModelState.UserUID[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
          else if (error.ModelState.CustomerUID != null)
            Assert.AreEqual(resourceError, error.ModelState.CustomerUID[0].ToString());
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

    #region Response Verification

    public void VerifyCustomerUserAssociateServiceResponse(AssociateCustomerUserModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.AssociateCustomerUserEvent.CustomerUID != null)
          Assert.AreEqual(AssociateCustomerUserModel.CustomerUID, kafkaresponse.AssociateCustomerUserEvent.CustomerUID);
        if (kafkaresponse.AssociateCustomerUserEvent.UserUID != null)
          Assert.AreEqual(AssociateCustomerUserModel.UserUID, kafkaresponse.AssociateCustomerUserEvent.UserUID);
        if (kafkaresponse.AssociateCustomerUserEvent.ActionUTC != null)
          Assert.AreEqual(AssociateCustomerUserModel.ActionUTC, kafkaresponse.AssociateCustomerUserEvent.ActionUTC);
      }
    }

    public void VerifyCustomerUserDissociateServiceResponse(DissociateCustomerUserModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.DissociateCustomerUserEvent.CustomerUID != null)
          Assert.AreEqual(DissociateCustomerUserModel.CustomerUID, kafkaresponse.DissociateCustomerUserEvent.CustomerUID);
        if (kafkaresponse.DissociateCustomerUserEvent.UserUID != null)
          Assert.AreEqual(DissociateCustomerUserModel.UserUID, kafkaresponse.DissociateCustomerUserEvent.UserUID);
        if (kafkaresponse.DissociateCustomerUserEvent.ActionUTC != null)
          Assert.AreEqual(DissociateCustomerUserModel.ActionUTC, kafkaresponse.DissociateCustomerUserEvent.ActionUTC);
      }
    }

    #endregion

    public void SetupCustomerUserAssociateKafkaConsumer(Guid CustomerUidToLookFor, Guid AssetUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerUserAssociateHandler = new CheckForCustomerUserAssociateHandler(CustomerUidToLookFor, AssetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerUserAssociateHandler);
    }

    public void SetupCustomerUserDissociateKafkaConsumer(Guid CustomerUidToLookFor, Guid AssetUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerUserDissociateHandler = new CheckForCustomerUserDissociateHandler(CustomerUidToLookFor, AssetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerUserDissociateHandler);
    }

    private void SubscribeAndConsumeFromKafka(CheckForCustomerUserHandler CustomerHandler)
    {
      var eventAggregator = new EventAggregator();
      eventAggregator.Subscribe(CustomerHandler);
      _consumerWrapper = new ConsumerWrapper(eventAggregator,
          new KafkaConsumerParams("CustomerAssetServiceAcceptanceTest", MasterDataConfig.CustomerServiceKafkaUri,
              MasterDataConfig.CustomerServiceTopic));
      //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
      Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
      Thread.Sleep(new TimeSpan(0, 0, 10));
    }
  }
}
