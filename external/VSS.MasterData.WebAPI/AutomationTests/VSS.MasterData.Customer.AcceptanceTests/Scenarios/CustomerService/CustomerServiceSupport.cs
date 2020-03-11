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
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService;
using VSS.Kafka.Factory.Model;
using VSS.Kafka.Factory.Interface;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService
{
  public class CustomerServiceSupport : IHandler
  {

    #region Variables

    private static Log4Net Log = new Log4Net(typeof(CustomerServiceSupport));

    public CreateCustomerEvent CreateCustomerModel = new CreateCustomerEvent();
    public UpdateCustomerEvent UpdateCustomerModel = new UpdateCustomerEvent();
    public DeleteCustomerEvent DeleteCustomerModel = new DeleteCustomerEvent();

    public InvalidCreateCustomerEvent InvalidCreateCustomerModel = new InvalidCreateCustomerEvent();
    public InvalidUpdateCustomerEvent InvalidUpdateCustomerModel = new InvalidUpdateCustomerEvent();
    public InvalidDeleteCustomerEvent InvalidDeleteCustomerModel = new InvalidDeleteCustomerEvent();

    public string ResponseString = string.Empty;

    public CreateCustomerModel customerServiceCreateResponse = null;
    public UpdateCustomerModel customerServiceUpdateResponse = null;
    public DeleteCustomerModel customerServiceDeleteResponse = null;

    #endregion

    #region Constructors

    public CustomerServiceSupport(Log4Net myLog)
    {
      CustomerServiceConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateCustomerModel);

      try
      {
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUID);
        ResponseString = RestClientUtil.DoHttpRequest(string.Format("{0}?CustomerUID={1}&ActionUTC={2}", CustomerServiceConfig.CustomerServiceEndpoint, customerUID, actionUtcString),
          HeaderSettings.DeleteMethod, accessToken,
           HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint, HeaderSettings.PostMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint, HeaderSettings.PutMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUid);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(string.Format("{0}?CustomerUID={1}&ActionUTC={2}", CustomerServiceConfig.CustomerServiceEndpoint, customerUid, actionUtc),
           HeaderSettings.DeleteMethod, accessToken,
          contentType, null, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string resourceError = CustomerServiceMessages.ResourceManager.GetString(ErrorMessage);
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

    public void VerifyCustomerServiceCreateResponse()
    {
      WaitForKafkaResponseAfterCreate();

      if (CreateCustomerModel.CustomerName != null)
        Assert.AreEqual(CreateCustomerModel.CustomerName, customerServiceCreateResponse.CreateCustomerEvent.CustomerName);
      if (CreateCustomerModel.CustomerType != null)
        Assert.AreEqual(CreateCustomerModel.CustomerType, customerServiceCreateResponse.CreateCustomerEvent.CustomerType);
      if (CreateCustomerModel.BSSID != null)
        Assert.AreEqual(CreateCustomerModel.BSSID, customerServiceCreateResponse.CreateCustomerEvent.BSSID);
      if (CreateCustomerModel.DealerNetwork != null)
        Assert.AreEqual(CreateCustomerModel.DealerNetwork, customerServiceCreateResponse.CreateCustomerEvent.DealerNetwork);
      if (CreateCustomerModel.NetworkDealerCode != null)
        Assert.AreEqual(CreateCustomerModel.NetworkDealerCode, customerServiceCreateResponse.CreateCustomerEvent.NetworkDealerCode);
      if (CreateCustomerModel.NetworkCustomerCode != null)
        Assert.AreEqual(CreateCustomerModel.NetworkCustomerCode, customerServiceCreateResponse.CreateCustomerEvent.NetworkCustomerCode);
      if (CreateCustomerModel.DealerAccountCode != null)
        Assert.AreEqual(CreateCustomerModel.DealerAccountCode, customerServiceCreateResponse.CreateCustomerEvent.DealerAccountCode);
      if (CreateCustomerModel.CustomerUID != null)
        Assert.AreEqual(CreateCustomerModel.CustomerUID, customerServiceCreateResponse.CreateCustomerEvent.CustomerUID);
      if (CreateCustomerModel.ActionUTC != null)
        Assert.AreEqual(CreateCustomerModel.ActionUTC.ToString("yyyyMMddHHmmss"), customerServiceCreateResponse.CreateCustomerEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      customerServiceCreateResponse = null; // Reassigning the response back to null
    }

    public void VerifyCustomerServiceUpdateResponse()
    {
      WaitForKafkaResponseAfterUpdate();

      if (UpdateCustomerModel.CustomerName != null)
        Assert.AreEqual(UpdateCustomerModel.CustomerName, customerServiceUpdateResponse.UpdateCustomerEvent.CustomerName);
      if (UpdateCustomerModel.DealerNetwork != null)
        Assert.AreEqual(UpdateCustomerModel.DealerNetwork, customerServiceUpdateResponse.UpdateCustomerEvent.DealerNetwork);
      if (UpdateCustomerModel.NetworkDealerCode != null)
        Assert.AreEqual(UpdateCustomerModel.NetworkDealerCode, customerServiceUpdateResponse.UpdateCustomerEvent.NetworkDealerCode);
      if (UpdateCustomerModel.NetworkCustomerCode != null)
        Assert.AreEqual(UpdateCustomerModel.NetworkCustomerCode, customerServiceUpdateResponse.UpdateCustomerEvent.NetworkCustomerCode);
      if (UpdateCustomerModel.DealerAccountCode != null)
        Assert.AreEqual(UpdateCustomerModel.DealerAccountCode, customerServiceUpdateResponse.UpdateCustomerEvent.DealerAccountCode);
      if (UpdateCustomerModel.CustomerUID != null)
        Assert.AreEqual(UpdateCustomerModel.CustomerUID, customerServiceUpdateResponse.UpdateCustomerEvent.CustomerUID);
      if (UpdateCustomerModel.ActionUTC != null)
        Assert.AreEqual(UpdateCustomerModel.ActionUTC.ToString("yyyyMMddHHmmss"), customerServiceUpdateResponse.UpdateCustomerEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      customerServiceUpdateResponse = null; // Reassigning the response back to null

    }

    public void VerifyCustomerServiceDeleteResponse()
    {
      WaitForKafkaResponseAfterDelete();

      if (DeleteCustomerModel.CustomerUID != null)
        Assert.AreEqual(DeleteCustomerModel.CustomerUID, customerServiceDeleteResponse.DeleteCustomerEvent.CustomerUID);
      if (DeleteCustomerModel.ActionUTC != null)
        Assert.AreEqual(DeleteCustomerModel.ActionUTC.ToString("yyyyMMddHHmmss"), customerServiceDeleteResponse.DeleteCustomerEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      customerServiceDeleteResponse = null; // Reassigning the response back to null

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

        if (CreateCustomerModel != null && CreateCustomerModel.ActionUTC != null)
        {
          if (CreateCustomerModel.ActionUTC.ToString() != null && message.Value.Contains(CreateCustomerModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(CreateCustomerModel.ReceivedUTC.ToString())
              && CreateCustomerModel.CustomerUID.ToString() != null && message.Value.Contains(CreateCustomerModel.CustomerUID.ToString()))
            customerServiceCreateResponse = JsonConvert.DeserializeObject<CreateCustomerModel>(message.Value);
          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (UpdateCustomerModel != null && UpdateCustomerModel.ActionUTC != null && UpdateCustomerModel.CustomerUID != Guid.Empty)
          {
            if (UpdateCustomerModel.ActionUTC.ToString() != null && message.Value.Contains(UpdateCustomerModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(UpdateCustomerModel.ReceivedUTC.ToString())
                && UpdateCustomerModel.CustomerUID.ToString() != null && message.Value.Contains(UpdateCustomerModel.CustomerUID.ToString()))
              customerServiceUpdateResponse = JsonConvert.DeserializeObject<UpdateCustomerModel>(message.Value);
            LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
          }

          if (DeleteCustomerModel != null && DeleteCustomerModel.ActionUTC != null && DeleteCustomerModel.CustomerUID != Guid.Empty)
          {
            if (DeleteCustomerModel.ActionUTC.ToString() != null && message.Value.Contains(DeleteCustomerModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(DeleteCustomerModel.ReceivedUTC.ToString())
                && DeleteCustomerModel.CustomerUID.ToString() != null && message.Value.Contains(DeleteCustomerModel.CustomerUID.ToString()))
              customerServiceDeleteResponse = JsonConvert.DeserializeObject<DeleteCustomerModel>(message.Value);
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

    private void WaitForKafkaResponseAfterCreate(bool isPositiveCase = true)
    {

      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (CreateCustomerModel.CustomerUID != Guid.Empty)
        {
          if (customerServiceCreateResponse != null)
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

    private void WaitForKafkaResponseAfterUpdate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (UpdateCustomerModel.CustomerUID != Guid.Empty)
        {
          if (customerServiceUpdateResponse != null)
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

    private void WaitForKafkaResponseAfterDelete(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (DeleteCustomerModel.CustomerUID != Guid.Empty)
        {
          if (customerServiceDeleteResponse != null)
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
