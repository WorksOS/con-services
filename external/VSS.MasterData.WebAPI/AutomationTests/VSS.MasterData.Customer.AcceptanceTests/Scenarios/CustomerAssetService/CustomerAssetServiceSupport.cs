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
using VSS.Kafka.Factory.Interface;
using VSS.Kafka.Factory.Model;
using VSS.MasterData.Customer.AcceptanceTests.Resources;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerAssetService
{
  public class CustomerAssetServiceSupport : IHandler
  {

    #region Variables

    private static Log4Net Log = new Log4Net(typeof(CustomerAssetServiceSupport));

    public AssociateCustomerAssetEvent AssociateCustomerAssetModel = new AssociateCustomerAssetEvent();
    public DissociateCustomerAssetEvent DissociateCustomerAssetModel = new DissociateCustomerAssetEvent();

    public InvalidAssociateCustomerAssetEvent InvalidAssociateCustomerAssetModel = new InvalidAssociateCustomerAssetEvent();
    public InvalidDissociateCustomerAssetEvent InvalidDissociateCustomerAssetModel = new InvalidDissociateCustomerAssetEvent();

    public string ResponseString = string.Empty;

    public AssociateCustomerAssetModel associateCustomerAssetResponse = new AssociateCustomerAssetModel();
    public DissociateCustomerAssetModel dissociateCustomerAssetResponse = new DissociateCustomerAssetModel();

    #endregion

    #region Constructors

    public CustomerAssetServiceSupport(Log4Net myLog)
    {
      CustomerServiceConfig.SetupEnvironment();
      Log = myLog;
    }
    #endregion

    #region Post Methods

    public void PostValidCustomerAssetAssociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(AssociateCustomerAssetModel);

      try
      {
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/AssociateCustomerAsset", HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/DissociateCustomerAsset", HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/AssociateCustomerAsset", HeaderSettings.PostMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/DissociateCustomerAsset", HeaderSettings.PostMethod, accessToken,
          contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string resourceError = CustomerServiceMessages.ResourceManager.GetString(ErrorMessage);
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
            Assert.AreEqual(CustomerServiceMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
        }
        else
          Assert.AreEqual(resourceError, error.Message);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
        throw new Exception("Got Error While DeSerializing JSON Object");
      }
    }
    #endregion

    #region Response Verification

    public void VerifyCustomerAssetAssociateServiceResponse()
    {
      WaitForKafkaResponseAfterAssociate();

      if (AssociateCustomerAssetModel.CustomerUID != null)
        Assert.AreEqual(AssociateCustomerAssetModel.CustomerUID, associateCustomerAssetResponse.AssociateCustomerAssetEvent.CustomerUID);
      if (AssociateCustomerAssetModel.AssetUID != null)
        Assert.AreEqual(AssociateCustomerAssetModel.AssetUID, associateCustomerAssetResponse.AssociateCustomerAssetEvent.AssetUID);
      if (AssociateCustomerAssetModel.RelationType != null)
        Assert.AreEqual(AssociateCustomerAssetModel.RelationType, associateCustomerAssetResponse.AssociateCustomerAssetEvent.RelationType);
      if (AssociateCustomerAssetModel.ActionUTC != null)
        Assert.AreEqual(AssociateCustomerAssetModel.ActionUTC.ToString("yyyyMMddHHmmss"), associateCustomerAssetResponse.AssociateCustomerAssetEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      associateCustomerAssetResponse = null;//reassigning the response back to null
    }

    public void VerifyCustomerAssetDissociateServiceResponse()
    {
      WaitForKafkaResponseAfterDissociate();

      if (DissociateCustomerAssetModel.CustomerUID != null)
        Assert.AreEqual(DissociateCustomerAssetModel.CustomerUID, dissociateCustomerAssetResponse.DissociateCustomerAssetEvent.CustomerUID);
      if (DissociateCustomerAssetModel.AssetUID != null)
        Assert.AreEqual(DissociateCustomerAssetModel.AssetUID, dissociateCustomerAssetResponse.DissociateCustomerAssetEvent.AssetUID);
      if (DissociateCustomerAssetModel.ActionUTC != null)
        Assert.AreEqual(DissociateCustomerAssetModel.ActionUTC.ToString("yyyyMMddHHmmss"), dissociateCustomerAssetResponse.DissociateCustomerAssetEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      dissociateCustomerAssetResponse = null;//reassigning the response back to null
    }

    #endregion

    #region Kafka Handler
    public bool BatchRead
    {
      get
      {
        return false;
      }
    }

    public bool ReadAsync
    {
      get
      {
        return false;
      }
    }

    public void Handle(PayloadMessage message)
    {
      try
      {
        if (message.Value == null || message.Value == "null")
        {
          LogResult.Report(Log, "log_ForInfo", "Kafka Message is Null");
          return;
        }

        if (AssociateCustomerAssetModel != null && AssociateCustomerAssetModel.ActionUTC != null)
        {
          if (AssociateCustomerAssetModel.ActionUTC.ToString() != null && message.Value.Contains(AssociateCustomerAssetModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(AssociateCustomerAssetModel.ReceivedUTC.ToString())
              && AssociateCustomerAssetModel.CustomerUID.ToString() != null && message.Value.Contains(AssociateCustomerAssetModel.CustomerUID.ToString()))
            associateCustomerAssetResponse = JsonConvert.DeserializeObject<AssociateCustomerAssetModel>(message.Value);
          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (DissociateCustomerAssetModel != null && DissociateCustomerAssetModel.ActionUTC != null && DissociateCustomerAssetModel.CustomerUID != Guid.Empty)
          {
            if (DissociateCustomerAssetModel.ActionUTC.ToString() != null && message.Value.Contains(DissociateCustomerAssetModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(DissociateCustomerAssetModel.ReceivedUTC.ToString())
                && DissociateCustomerAssetModel.CustomerUID.ToString() != null && message.Value.Contains(DissociateCustomerAssetModel.CustomerUID.ToString()))
              dissociateCustomerAssetResponse = JsonConvert.DeserializeObject<DissociateCustomerAssetModel>(message.Value);
            LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
          }
        }

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Handling Response", e);
        throw new Exception(e + "Got Error While Handling Response");
      }

    }

    public void Handle(List<PayloadMessage> messages)
    {

    }

    #endregion

    #region Helpers

    private void WaitForKafkaResponseAfterAssociate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (AssociateCustomerAssetModel.CustomerUID != Guid.Empty)
        {
          if (associateCustomerAssetResponse.AssociateCustomerAssetEvent != null)
            break;
        }
        Thread.Sleep(1000);
      }
      if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
        throw new Exception("Got Error While Waiting For Kafka Response");
      }
    }

    private void WaitForKafkaResponseAfterDissociate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (DissociateCustomerAssetModel.CustomerUID != Guid.Empty)
        {
          if (dissociateCustomerAssetResponse.DissociateCustomerAssetEvent != null)
            break;
        }
        Thread.Sleep(1000);
      }
      if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
        throw new Exception("Got Error While Waiting For Kafka Response");
      }
    }
    #endregion

  }
}
