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
using VSS.MasterData.Customer.AcceptanceTests.Resources;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerUserService;
using VSS.Kafka.Factory.Interface;
using VSS.Kafka.Factory.Model;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerUserService
{
  public class CustomerUserServiceSupport : IHandler
  {

    #region Variables

    private static Log4Net Log = new Log4Net(typeof(CustomerUserServiceSupport));

    public AssociateCustomerUserEvent AssociateCustomerUserModel = new AssociateCustomerUserEvent();
    public DissociateCustomerUserEvent DissociateCustomerUserModel = new DissociateCustomerUserEvent();

    public InvalidAssociateCustomerUserEvent InvalidAssociateCustomerUserModel = new InvalidAssociateCustomerUserEvent();
    public InvalidDissociateCustomerUserEvent InvalidDissociateCustomerUserModel = new InvalidDissociateCustomerUserEvent();

    public string ResponseString = string.Empty;

    public AssociateCustomerUserModel associateCustomerUserResponse = null;
    public DissociateCustomerUserModel dissociateCustomerUserResponse = null;

    #endregion

    #region Constructors

    public CustomerUserServiceSupport(Log4Net myLog)
    {
      CustomerServiceConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Post Methods

    public void PostValidCustomerUserAssociateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(AssociateCustomerUserModel);

      try
      {
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/AssociateCustomerUser", HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/DissociateCustomerUser", HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/AssociateCustomerUser", HeaderSettings.PostMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/DissociateCustomerUser", HeaderSettings.PostMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string resourceError = CustomerServiceMessages.ResourceManager.GetString(ErrorMessage);
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

    public void VerifyCustomerUserAssociateServiceResponse()
    {
      WaitForKafkaResponseAfterAssociate();

      if (AssociateCustomerUserModel.CustomerUID != null)
        Assert.AreEqual(AssociateCustomerUserModel.CustomerUID, associateCustomerUserResponse.AssociateCustomerUserEvent.CustomerUID);
      if (AssociateCustomerUserModel.UserUID != null)
        Assert.AreEqual(AssociateCustomerUserModel.UserUID, associateCustomerUserResponse.AssociateCustomerUserEvent.UserUID);
      if (AssociateCustomerUserModel.ActionUTC != null)
        Assert.AreEqual(AssociateCustomerUserModel.ActionUTC.ToString("yyyyMMddHHmmss"), associateCustomerUserResponse.AssociateCustomerUserEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      associateCustomerUserResponse = null;//reassigning the response back to null

    }

    public void VerifyCustomerUserDissociateServiceResponse()
    {
      WaitForKafkaResponseAfterDissociate();

      if (DissociateCustomerUserModel.CustomerUID != null)
        Assert.AreEqual(DissociateCustomerUserModel.CustomerUID, dissociateCustomerUserResponse.DissociateCustomerUserEvent.CustomerUID);
      if (DissociateCustomerUserModel.UserUID != null)
        Assert.AreEqual(DissociateCustomerUserModel.UserUID, dissociateCustomerUserResponse.DissociateCustomerUserEvent.UserUID);
      if (DissociateCustomerUserModel.ActionUTC != null)
        Assert.AreEqual(DissociateCustomerUserModel.ActionUTC.ToString("yyyyMMddHHmmss"), dissociateCustomerUserResponse.DissociateCustomerUserEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      dissociateCustomerUserResponse = null;//reassigning the response back to null
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

        if (AssociateCustomerUserModel != null && AssociateCustomerUserModel.ActionUTC != null)
        {
          if (AssociateCustomerUserModel.ActionUTC.ToString() != null && message.Value.Contains(AssociateCustomerUserModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(AssociateCustomerUserModel.ReceivedUTC.ToString())
              && AssociateCustomerUserModel.CustomerUID.ToString() != null && message.Value.Contains(AssociateCustomerUserModel.CustomerUID.ToString()))
            associateCustomerUserResponse = JsonConvert.DeserializeObject<AssociateCustomerUserModel>(message.Value);
          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (DissociateCustomerUserModel != null && DissociateCustomerUserModel.ActionUTC != null && DissociateCustomerUserModel.CustomerUID != Guid.Empty)
          {
            if (DissociateCustomerUserModel.ActionUTC.ToString() != null && message.Value.Contains(DissociateCustomerUserModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(DissociateCustomerUserModel.ReceivedUTC.ToString())
                && DissociateCustomerUserModel.CustomerUID.ToString() != null && message.Value.Contains(DissociateCustomerUserModel.CustomerUID.ToString()))
              dissociateCustomerUserResponse = JsonConvert.DeserializeObject<DissociateCustomerUserModel>(message.Value);
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
        if (AssociateCustomerUserModel.CustomerUID != Guid.Empty)
        {
          if (associateCustomerUserResponse != null)
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
        if (DissociateCustomerUserModel.CustomerUID != Guid.Empty)
        {
          if (dissociateCustomerUserResponse!= null)
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
