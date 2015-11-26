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
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerAssetService
{
  class CustomerAssetServiceSupport
  {
    private static Log4Net Log = new Log4Net(typeof(CustomerAssetServiceSupport));

    public AssociateCustomerAssetEvent AssociateCustomerAssetModel = new AssociateCustomerAssetEvent();
    public DissociateCustomerAssetEvent DissociateCustomerAssetModel = new DissociateCustomerAssetEvent();
    public InvalidAssociateCustomerAssetEvent InvalidAssociateCustomerAssetModel = new InvalidAssociateCustomerAssetEvent();
    public InvalidDissociateCustomerAssetEvent InvalidDissociateCustomerAssetModel = new InvalidDissociateCustomerAssetEvent();

    public string ResponseString = string.Empty;
    public ConsumerWrapper _consumerWrapper;
    public static string UserName;
    public static string PassWord;
    public CheckForCustomerAssetAssociateHandler _checkForCustomerAssetAssociateHandler;
    public CheckForCustomerAssetDissociateHandler _checkForCustomerAssetDissociateHandler;

    #region Constructors

    public CustomerAssetServiceSupport(Log4Net myLog)
    {
      MasterDataConfig.SetupEnvironment();
      Log = myLog;
    }
    #endregion

    #region Post Methods

    public void PostValidCustomerAssetAssociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(AssociateCustomerAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/AssociateCustomerAsset", HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To customer Service");
      }
    }

    public void PostValidCustomerAssetDissociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(DissociateCustomerAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/DissociateCustomerAsset", HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To customer Service");
      }
    }

    public void PostInValidCustomerAssetAssociateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidAssociateCustomerAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/AssociateCustomerAsset", HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To customer Service");
      }
    }

    public void PostInValidCustomerAssetDissociateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidDissociateCustomerAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.CustomerServiceEndpoint + "/DissociateCustomerAsset", HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To customer Service");
      }
    }
    #endregion

    #region ErrorResponse Verification
    public void VerifyErrorResponse(string ErrorMessage)
    {
      try
      {
        CustomerAssetServiceErrorResponseModel error = JsonConvert.DeserializeObject<CustomerAssetServiceErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.RelationType != null)
            Assert.AreEqual(resourceError, error.ModelState.RelationType[0].ToString());
          else if (error.ModelState.AssetUID != null)
            Assert.AreEqual(resourceError, error.ModelState.AssetUID[0].ToString());
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

    public void VerifyCustomerAssetAssociateServiceResponse(AssociateCustomerAssetModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.AssociateCustomerAssetEvent.CustomerUID != null)
          Assert.AreEqual(AssociateCustomerAssetModel.CustomerUID, kafkaresponse.AssociateCustomerAssetEvent.CustomerUID);
        if (kafkaresponse.AssociateCustomerAssetEvent.AssetUID != null)
          Assert.AreEqual(AssociateCustomerAssetModel.AssetUID, kafkaresponse.AssociateCustomerAssetEvent.AssetUID);
        if (kafkaresponse.AssociateCustomerAssetEvent.RelationType != null)
          Assert.AreEqual(AssociateCustomerAssetModel.RelationType, kafkaresponse.AssociateCustomerAssetEvent.RelationType);
        if (kafkaresponse.AssociateCustomerAssetEvent.ActionUTC != null)
          Assert.AreEqual(AssociateCustomerAssetModel.ActionUTC, kafkaresponse.AssociateCustomerAssetEvent.ActionUTC);
      }
    }

    public void VerifyCustomerAssetDissociateServiceResponse(DissociateCustomerAssetModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.DissociateCustomerAssetEvent.CustomerUID != null)
          Assert.AreEqual(DissociateCustomerAssetModel.CustomerUID, kafkaresponse.DissociateCustomerAssetEvent.CustomerUID);
        if (kafkaresponse.DissociateCustomerAssetEvent.AssetUID != null)
          Assert.AreEqual(DissociateCustomerAssetModel.AssetUID, kafkaresponse.DissociateCustomerAssetEvent.AssetUID);
        if (kafkaresponse.DissociateCustomerAssetEvent.ActionUTC != null)
          Assert.AreEqual(DissociateCustomerAssetModel.ActionUTC, kafkaresponse.DissociateCustomerAssetEvent.ActionUTC);
      }
    }

    #endregion

    public void SetupCustomerAssetAssociateKafkaConsumer(Guid CustomerUidToLookFor, Guid AssetUidToLookFor, DateTime actionUtc)
    {
      _checkForCustomerAssetAssociateHandler = new CheckForCustomerAssetAssociateHandler(CustomerUidToLookFor,AssetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerAssetAssociateHandler);
    }

    public void SetupCustomerAssetDissociateKafkaConsumer(Guid CustomerUidToLookFor, Guid AssetUidToLookFor,DateTime actionUtc)
    {
      _checkForCustomerAssetDissociateHandler = new CheckForCustomerAssetDissociateHandler(CustomerUidToLookFor, AssetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForCustomerAssetDissociateHandler);
    }

    private void SubscribeAndConsumeFromKafka(CheckForCustomerAssetHandler CustomerHandler)
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
