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
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerService
{
  class CustomerServiceSupport
  {
    public CreateCustomerEvent CreateCustomerModel = new CreateCustomerEvent();
    public UpdateCustomerEvent UpdateCustomerModel = new UpdateCustomerEvent();
    public DeleteCustomerEvent DeleteCustomerModel = new DeleteCustomerEvent();
    public InvalidCreateCustomerEvent InvalidCreateCustomerModel = new InvalidCreateCustomerEvent();
    public InvalidUpdateCustomerEvent InvalidUpdateCustomerModel = new InvalidUpdateCustomerEvent();
    public InvalidDeleteCustomerEvent InvalidDeleteCustomerModel = new InvalidDeleteCustomerEvent();
    private static Log4Net Log = new Log4Net(typeof(CustomerServiceSupport));

    public string ResponseString = string.Empty;
    public ConsumerWrapper _consumerWrapper;
    public static string UserName;
    public static string PassWord;
    public CheckForCustomerCreateHandler _checkForCustomerCreateHandler;
    public CheckForCustomerUpdateHandler _checkForCustomerUpdateHandler;
    public CheckForCustomerDeleteHandler _checkForCustomerDeleteHandler;

    #region Constructors

    public CustomerServiceSupport(Log4Net myLog)
    {
      MasterDataConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateCustomerModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostValidUpdateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(UpdateCustomerModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostValidDeleteRequestToService(Guid customerUid, DateTime actionUtc)
    {

      string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
      string customerUID = customerUid.ToString();
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUID);
        RestClientUtil.DoHttpRequest(string.Format("{0}?CustomerUID={1}&ActionUTC={2}", MasterDataConfig.CustomerServiceEndpoint, customerUID, actionUtcString),
          HeaderSettings.DeleteMethod, UserName, PassWord, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");

      }

    }


    public void PostInValidCreateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidCreateCustomerModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostInValidUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidUpdateCustomerModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostInValidDeleteRequestToService(string customerUid, string actionUtc, string contentType, HttpStatusCode actualResponse)
    {
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUid);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(string.Format("{0}?CustomerUID={1}&ActionUTC={2}", MasterDataConfig.CustomerServiceEndpoint, customerUid, actionUtc),
          HeaderSettings.DeleteMethod, UserName, PassWord, contentType, null, actualResponse);
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
        CustomerServiceErrorResponseModel error = JsonConvert.DeserializeObject<CustomerServiceErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.CustomerName != null)
            Assert.AreEqual(resourceError, error.ModelState.CustomerName[0].ToString());
          else if (error.ModelState.CustomerType != null)
            Assert.AreEqual(resourceError, error.ModelState.CustomerType[0].ToString());
          else if (error.ModelState.BSSID != null)
            Assert.AreEqual(resourceError, error.ModelState.BSSID[0].ToString());
          else if (error.ModelState.DealerNetwork != null)
            Assert.AreEqual(resourceError, error.ModelState.DealerNetwork[0].ToString());
          else if (error.ModelState.NetworkDealerCode != null)
            Assert.AreEqual(resourceError, error.ModelState.NetworkDealerCode[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
          else if (error.ModelState.NetworkCustomerCode != null)
            Assert.AreEqual(resourceError, error.ModelState.NetworkCustomerCode[0].ToString());
          else if (error.ModelState.DealerAccountCode != null)
            Assert.AreEqual(resourceError, error.ModelState.DealerAccountCode[0].ToString());
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

    public void SetupCreateCustomerKafkaConsumer(Guid CustomerUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerCreateHandler = new CheckForCustomerCreateHandler(CustomerUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerCreateHandler);
    }

    public void SetupUpdateCustomerKafkaConsumer(Guid CustomerUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerUpdateHandler = new CheckForCustomerUpdateHandler(CustomerUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerUpdateHandler);
    }

    public void SetupDeleteCustomerKafkaConsumer(Guid CustomerUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerDeleteHandler = new CheckForCustomerDeleteHandler(CustomerUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerDeleteHandler);
    }

    private void SubscribeAndConsumeFromKafka(CheckForCustomerHandler CustomerHandler)
    {
      var eventAggregator = new EventAggregator();
      eventAggregator.Subscribe(CustomerHandler);
      _consumerWrapper = new ConsumerWrapper(eventAggregator,
          new KafkaConsumerParams("CustomerServiceAcceptanceTest", MasterDataConfig.CustomerServiceKafkaUri,
              MasterDataConfig.CustomerServiceTopic));
      //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
      Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
      Thread.Sleep(new TimeSpan(0, 0, 10));
    }

    #region Response Verification

    public void VerifyCustomerServiceCreateResponse(CreateCustomerModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.CreateCustomerEvent.CustomerName != null)
          Assert.AreEqual(CreateCustomerModel.CustomerName, kafkaresponse.CreateCustomerEvent.CustomerName);
        if (kafkaresponse.CreateCustomerEvent.CustomerType != null)
          Assert.AreEqual(CreateCustomerModel.CustomerType, kafkaresponse.CreateCustomerEvent.CustomerType);
        if (kafkaresponse.CreateCustomerEvent.BSSID != null)
          Assert.AreEqual(CreateCustomerModel.BSSID, kafkaresponse.CreateCustomerEvent.BSSID);
        if (kafkaresponse.CreateCustomerEvent.DealerNetwork != null)
          Assert.AreEqual(CreateCustomerModel.DealerNetwork, kafkaresponse.CreateCustomerEvent.DealerNetwork);
        if (kafkaresponse.CreateCustomerEvent.NetworkDealerCode != null)
          Assert.AreEqual(CreateCustomerModel.NetworkDealerCode, kafkaresponse.CreateCustomerEvent.NetworkDealerCode);
        if (kafkaresponse.CreateCustomerEvent.NetworkCustomerCode != null)
          Assert.AreEqual(CreateCustomerModel.NetworkCustomerCode, kafkaresponse.CreateCustomerEvent.NetworkCustomerCode);
        if (kafkaresponse.CreateCustomerEvent.DealerAccountCode != null)
          Assert.AreEqual(CreateCustomerModel.DealerAccountCode, kafkaresponse.CreateCustomerEvent.DealerAccountCode);
        if (kafkaresponse.CreateCustomerEvent.CustomerUID != null)
          Assert.AreEqual(CreateCustomerModel.CustomerUID, kafkaresponse.CreateCustomerEvent.CustomerUID);
        if (kafkaresponse.CreateCustomerEvent.ActionUTC != null)
          Assert.AreEqual(CreateCustomerModel.ActionUTC, kafkaresponse.CreateCustomerEvent.ActionUTC);
      }
    }

    public void VerifyCustomerServiceUpdateResponse(UpdateCustomerModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.UpdateCustomerEvent.CustomerName != null)
          Assert.AreEqual(UpdateCustomerModel.CustomerName, kafkaresponse.UpdateCustomerEvent.CustomerName);
        if (kafkaresponse.UpdateCustomerEvent.DealerNetwork != null)
          Assert.AreEqual(UpdateCustomerModel.DealerNetwork, kafkaresponse.UpdateCustomerEvent.DealerNetwork);
        if (kafkaresponse.UpdateCustomerEvent.NetworkDealerCode != null)
          Assert.AreEqual(UpdateCustomerModel.NetworkDealerCode, kafkaresponse.UpdateCustomerEvent.NetworkDealerCode);
        if (kafkaresponse.UpdateCustomerEvent.NetworkCustomerCode != null)
          Assert.AreEqual(UpdateCustomerModel.NetworkCustomerCode, kafkaresponse.UpdateCustomerEvent.NetworkCustomerCode);
        if (kafkaresponse.UpdateCustomerEvent.DealerAccountCode != null)
          Assert.AreEqual(UpdateCustomerModel.DealerAccountCode, kafkaresponse.UpdateCustomerEvent.DealerAccountCode);
        if (kafkaresponse.UpdateCustomerEvent.CustomerUID != null)
          Assert.AreEqual(UpdateCustomerModel.CustomerUID, kafkaresponse.UpdateCustomerEvent.CustomerUID);
        if (kafkaresponse.UpdateCustomerEvent.ActionUTC != null)
          Assert.AreEqual(UpdateCustomerModel.ActionUTC, kafkaresponse.UpdateCustomerEvent.ActionUTC);
      }
    }

    public void VerifyCustomerServiceDeleteResponse(DeleteCustomerModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.DeleteCustomerEvent.CustomerUID != null)
          Assert.AreEqual(DeleteCustomerModel.CustomerUID, kafkaresponse.DeleteCustomerEvent.CustomerUID);
        if (kafkaresponse.DeleteCustomerEvent.ActionUTC != null)
          Assert.AreEqual(DeleteCustomerModel.ActionUTC, kafkaresponse.DeleteCustomerEvent.ActionUTC);
      }
    }
    #endregion
  }


}
