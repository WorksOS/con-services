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
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerRelationshipService;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerRelationship
{
  public class CustomerRelationshipServiceSupport : IHandler
  {

    #region Variables

    private static Log4Net Log = new Log4Net(typeof(CustomerRelationshipServiceSupport));

    public CreateCustomerRelationshipEvent CreateCustomerRelationshipModel = new CreateCustomerRelationshipEvent();
    public DeleteCustomerRelationshipEvent DeleteCustomerRelationshipModel = new DeleteCustomerRelationshipEvent();

    public InvalidCreateCustomerRelationshipEvent InvalidCreateCustomerRelationshipModel = new InvalidCreateCustomerRelationshipEvent();
    public InvalidDeleteCustomerRelationshipEvent InvalidDeleteCustomerRelationshipModel = new InvalidDeleteCustomerRelationshipEvent();

    public string ResponseString = string.Empty;

    public CreateCustomerRelationshipModel createCustomerRelationshipResponse = new CreateCustomerRelationshipModel();
    public DeleteCustomerRelationshipModel deleteCustomerRelationshipResponse = new DeleteCustomerRelationshipModel();

    #endregion

    #region Constructors

    public CustomerRelationshipServiceSupport(Log4Net myLog)
    {
      CustomerServiceConfig.SetupEnvironment();
      Log = myLog;
    }
    #endregion

    #region PostMethods
    public void PostValidCreateCustomerRelationshipRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateCustomerRelationshipModel);

      try
      {
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/customerrelationship", HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");
      }
    }

    public void PostValidDeleteCustomerRelationshipRequestToService(Guid parentCustomeruid, Guid childCustomerUid, DateTime actionUtc)
    {

      string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
      string parentCustomerUID = parentCustomeruid.ToString();
      string childCustomerUID = childCustomerUid.ToString();
      try
      {
        string accessToken = CustomerServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + parentCustomerUID);
        ResponseString = RestClientUtil.DoHttpRequest(string.Format("{0}/customerrelationship?parentcustomeruid={1}&childcustomeruid={2}&actionutc={3}",
          CustomerServiceConfig.CustomerServiceEndpoint, parentCustomerUID, childCustomerUID, actionUtcString),
          HeaderSettings.DeleteMethod, accessToken,
           HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Customer Service", e);
        throw new Exception(e + " Got Error While Posting Data To Customer Service");

      }
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

        if (CreateCustomerRelationshipModel != null && CreateCustomerRelationshipModel.ActionUTC != null)
        {
          if (CreateCustomerRelationshipModel.ActionUTC.ToString() != null && message.Value.Contains(CreateCustomerRelationshipModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss"))
            && message.Value.Contains(CreateCustomerRelationshipModel.ReceivedUTC.ToString())
              && CreateCustomerRelationshipModel.ParentCustomerUID.ToString() != null && message.Value.Contains(CreateCustomerRelationshipModel.ParentCustomerUID.ToString())
            && CreateCustomerRelationshipModel.ChildCustomerUID.ToString() != null && message.Value.Contains(CreateCustomerRelationshipModel.ChildCustomerUID.ToString()))

            createCustomerRelationshipResponse = JsonConvert.DeserializeObject<CreateCustomerRelationshipModel>(message.Value);

          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (DeleteCustomerRelationshipModel != null && DeleteCustomerRelationshipModel.ActionUTC != null && DeleteCustomerRelationshipModel.ParentCustomerUID != Guid.Empty)
          {
            if (DeleteCustomerRelationshipModel.ActionUTC.ToString() != null && message.Value.Contains(DeleteCustomerRelationshipModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss")) && message.Value.Contains(DeleteCustomerRelationshipModel.ReceivedUTC.ToString())
                && DeleteCustomerRelationshipModel.ParentCustomerUID.ToString() != null && message.Value.Contains(DeleteCustomerRelationshipModel.ParentCustomerUID.ToString())
              && DeleteCustomerRelationshipModel.ParentCustomerUID.ToString() != null && message.Value.Contains(DeleteCustomerRelationshipModel.ParentCustomerUID.ToString()))


              deleteCustomerRelationshipResponse = JsonConvert.DeserializeObject<DeleteCustomerRelationshipModel>(message.Value);

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
        if (CreateCustomerRelationshipModel.ParentCustomerUID != Guid.Empty)
        {
          if (createCustomerRelationshipResponse.CreateCustomerRelationshipEvent != null)
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
        if (DeleteCustomerRelationshipModel.ParentCustomerUID != Guid.Empty)
        {
          if (deleteCustomerRelationshipResponse.DeleteCustomerRelationshipEvent != null)
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

    #region Response Verification

    public void VerifyCustomerRelationshipServiceCreateResponse()
    {
      WaitForKafkaResponseAfterCreate();


      Assert.AreEqual(CreateCustomerRelationshipModel.ParentCustomerUID, createCustomerRelationshipResponse.CreateCustomerRelationshipEvent.ParentCustomerUID);

      Assert.AreEqual(CreateCustomerRelationshipModel.ChildCustomerUID, createCustomerRelationshipResponse.CreateCustomerRelationshipEvent.ChildCustomerUID);

      Assert.AreEqual(CreateCustomerRelationshipModel.ActionUTC.ToString("yyyyMMddHHmmss"), createCustomerRelationshipResponse.CreateCustomerRelationshipEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      createCustomerRelationshipResponse = null; // Reassigning the response back to null
    }


    public void VerifyCustomerRelationshipServiceDeleteResponse()
    {
      WaitForKafkaResponseAfterDelete();

      Assert.AreEqual(DeleteCustomerRelationshipModel.ParentCustomerUID, deleteCustomerRelationshipResponse.DeleteCustomerRelationshipEvent.ParentCustomerUID);

      Assert.AreEqual(DeleteCustomerRelationshipModel.ChildCustomerUID, deleteCustomerRelationshipResponse.DeleteCustomerRelationshipEvent.ChildCustomerUID);

      Assert.AreEqual(DeleteCustomerRelationshipModel.ActionUTC.ToString("yyyyMMddHHmmss"), deleteCustomerRelationshipResponse.DeleteCustomerRelationshipEvent.ActionUTC.ToString("yyyyMMddHHmmss"));

      deleteCustomerRelationshipResponse = null; // Reassigning the response back to null

    }
    #endregion

    #region ErrorResponse Verification
    public void VerifyErrorResponse(string ErrorMessage)
    {
      try
      {
        CustomerRelationshipErrorResponseModel error = JsonConvert.DeserializeObject<CustomerRelationshipErrorResponseModel>(ResponseString);
        string resourceError = CustomerServiceMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.createCustomerRelationshipEvent != null)
            Assert.AreEqual(resourceError, error.ModelState.createCustomerRelationshipEvent[0].ToString());
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

  }
}

