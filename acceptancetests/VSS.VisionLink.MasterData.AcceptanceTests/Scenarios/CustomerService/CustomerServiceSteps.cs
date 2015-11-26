using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerService
{
  [Binding]
  public class CustomerServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerServiceSteps));
    private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);

    [Given(@"CustomerService Is Ready To Verify '(.*)'")]
    public void GivenCustomerServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerServiceCreate Request Is Setup With Default Values")]
    public void GivenCustomerServiceCreateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.CreateCustomerModel = GetDefaultValidCustomerServiceCreateRequest();
    }

    [When(@"I Post Valid CustomerServiceCreate Request")]
    public void WhenIPostValidCustomerServiceCreateRequest()
    {
      customerServiceSupport.SetupCreateCustomerKafkaConsumer(customerServiceSupport.CreateCustomerModel.CustomerUID, customerServiceSupport.CreateCustomerModel.ActionUTC);

      customerServiceSupport.PostValidCreateRequestToService();
    }

    [Then(@"The Processed CustomerServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      CreateCustomerModel kafkaresponse = customerServiceSupport._checkForCustomerCreateHandler.CustomerEvent;
      Assert.IsTrue(customerServiceSupport._checkForCustomerCreateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerServiceSupport.VerifyCustomerServiceCreateResponse(kafkaresponse);
    }

    [Given(@"CustomerServiceUpdate Request Is Setup With Default Values")]
    public void GivenCustomerServiceUpdateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.UpdateCustomerModel = GetDefaultValidCustomerServiceUpdateRequest();
    }

    [When(@"I Set CustomerServiceUpdate DealerNetwork To '(.*)'")]
    public void WhenISetCustomerServiceUpdateDealerNetworkTo(string dealerNetwork)
    {
      customerServiceSupport.UpdateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
    }

    [When(@"I Set Invalid CustomerServiceUpdate CustomerName To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateCustomerNameTo(string customerName)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
    }

    [When(@"I Set Invalid CustomerServiceUpdate DealerNetwork To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateDealerNetworkTo(string dealerNetwork)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
    }

    [When(@"I Set Invalid CustomerServiceUpdate NetworkDealerCode To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateNetworkDealerCodeTo(string networkDealercode)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealercode);
    }

    [When(@"I Set Invalid CustomerServiceUpdate NetworkCustomerCode To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateNetworkCustomerCodeTo(string networkCustomercode)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomercode);
    }

    [When(@"I Set Invalid CustomerServiceUpdate DealerAccountCode To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateDealerAccountCodeTo(string dealerAccountcode)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountcode);
    }

    [When(@"I Post Valid CustomerServiceUpdate Request")]
    public void WhenIPostValidCustomerServiceUpdateRequest()
    {
        customerServiceSupport.SetupUpdateCustomerKafkaConsumer(customerServiceSupport.UpdateCustomerModel.CustomerUID, customerServiceSupport.UpdateCustomerModel.ActionUTC);

      customerServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The Processed CustomerServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      UpdateCustomerModel kafkaresponse = customerServiceSupport._checkForCustomerUpdateHandler.CustomerEvent;
      Assert.IsTrue(customerServiceSupport._checkForCustomerUpdateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerServiceSupport.VerifyCustomerServiceUpdateResponse(kafkaresponse);
    }

    [Given(@"CustomerServiceDelete Request Is Setup With Default Values")]
    public void GivenCustomerServiceDeleteRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.DeleteCustomerModel = GetDefaultValidCustomerServiceDeleteRequest();
    }

    [When(@"I Post Valid CustomerServiceDelete Request")]
    public void WhenIPostValidCustomerServiceDeleteRequest()
    {
        customerServiceSupport.SetupDeleteCustomerKafkaConsumer(customerServiceSupport.DeleteCustomerModel.CustomerUID, customerServiceSupport.DeleteCustomerModel.ActionUTC);

        customerServiceSupport.PostValidDeleteRequestToService(customerServiceSupport.DeleteCustomerModel.CustomerUID, customerServiceSupport.DeleteCustomerModel.ActionUTC);
    }

    [Then(@"The Processed CustomerServiceDelete Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerServiceDeleteMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      DeleteCustomerModel kafkaresponse = customerServiceSupport._checkForCustomerDeleteHandler.CustomerEvent;
      Assert.IsTrue(customerServiceSupport._checkForCustomerDeleteHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerServiceSupport.VerifyCustomerServiceDeleteResponse(kafkaresponse);
    }

    [When(@"I Set CustomerServiceCreate NetworkDealerCode To '(.*)'")]
    public void WhenISetCustomerServiceCreateNetworkDealerCodeTo(string networkDealerCode)
    {
      customerServiceSupport.CreateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealerCode);
    }

    [When(@"I Set CustomerServiceCreate NetworkCustomerCode To '(.*)'")]
    public void WhenISetCustomerServiceCreateNetworkCustomerCodeTo(string networkCustomerCode)
    {
      customerServiceSupport.CreateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomerCode);
    }

    [When(@"I Set CustomerServiceCreate CustomerType To '(.*)'")]
    public void WhenISetCustomerServiceCreateCustomerTypeTo(CustomerType customerType)
    {
      customerServiceSupport.CreateCustomerModel.CustomerType = customerType;
    }

    [When(@"I Set CustomerServiceCreate DealerAccountCode To '(.*)'")]
    public void WhenISetCustomerServiceCreateDealerAccountCodeTo(string dealerAccountCode)
    {
      customerServiceSupport.CreateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountCode);
    }

    [When(@"I Set CustomerServiceUpdate CustomerName To '(.*)'")]
    public void WhenISetCustomerServiceUpdateCustomerNameTo(string customerName)
    {
      customerServiceSupport.UpdateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
    }

    [When(@"I Set CustomerServiceUpdate NetworkDealerCode To '(.*)'")]
    public void WhenISetCustomerServiceUpdateNetworkDealerCodeTo(string networkDealerCode)
    {
      customerServiceSupport.UpdateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealerCode);
    }

    [When(@"I Set CustomerServiceUpdate NetworkCustomerCode To '(.*)'")]
    public void WhenISetCustomerServiceUpdateNetworkCustomerCodeTo(string networkCustomerCode)
    {
      customerServiceSupport.UpdateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomerCode);
    }

    [When(@"I Set CustomerServiceUpdate DealerAccountCode To '(.*)'")]
    public void WhenISetCustomerServiceUpdateDealerAccountCodeTo(string dealerAccountCode)
    {
      customerServiceSupport.UpdateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountCode);
    }

    [Given(@"CustomerServiceCreate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      customerServiceSupport.InvalidCreateCustomerModel = GetDefaultInValidCustomerServiceCreateRequest();
    }

    [When(@"I Set Invalid CustomerServiceCreate CustomerName To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateCustomerNameTo(string customerName)
    {
      customerServiceSupport.InvalidCreateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
    }

    [When(@"I Post Invalid CustomerServiceCreate Request")]
    public void WhenIPostInvalidCustomerServiceCreateRequest()
    {
      string contentType = "application/json";
      customerServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"CustomerServiceCreate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerServiceCreateResponseWithShouldBeReturned(string errorMessage)
    {
      customerServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid CustomerServiceCreate CustomerType To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateCustomerTypeTo(string customerType)
    {
      customerServiceSupport.InvalidCreateCustomerModel.CustomerType = InputGenerator.GetValue(customerType);
    }

    [When(@"I Set Invalid CustomerServiceCreate BSSID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateBSSIDTo(string bssId)
    {
      customerServiceSupport.InvalidCreateCustomerModel.BSSID = InputGenerator.GetValue(bssId);
    }

    [When(@"I Set Invalid CustomerServiceCreate DealerNetwork To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateDealerNetworkTo(string dealerNetwork)
    {
      customerServiceSupport.InvalidCreateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
    }

    [When(@"I Set Invalid CustomerServiceCreate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateCustomerUIDTo(string customerUid)
    {
      customerServiceSupport.InvalidCreateCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid CustomerServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerServiceCreateActionUTCTo(string actionUtc)
    {
      customerServiceSupport.InvalidCreateCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Given(@"CustomerServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      customerServiceSupport.InvalidUpdateCustomerModel = GetDefaultInValidCustomerServiceUpdateRequest();
    }

    [When(@"I Set Invalid CustomerServiceUpdate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateCustomerUIDTo(string customerUid)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerServiceUpdate Request")]
    public void WhenIPostInvalidCustomerServiceUpdateRequest()
    {
      string contentType = "application/json";
      customerServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"CustomerServiceUpdate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerServiceUpdateResponseWithShouldBeReturned(string errorMessage)
    {
      customerServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid CustomerServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerServiceUpdateActionUTCTo(string actionUtc)
    {
      customerServiceSupport.InvalidUpdateCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Given(@"CustomerServiceDelete Request Is Setup With Invalid Default Values")]
    public void GivenCustomerServiceDeleteRequestIsSetupWithInvalidDefaultValues()
    {
      customerServiceSupport.InvalidDeleteCustomerModel = GetDefaultInValidCustomerServiceDeleteRequest();
    }

    [When(@"I Set Invalid CustomerServiceDelete CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDeleteCustomerUIDTo(string customerUid)
    {
      customerServiceSupport.InvalidDeleteCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerServiceDelete Request")]
    public void WhenIPostInvalidCustomerServiceDeleteRequest()
    {
        string contentType = "application/json";
        customerServiceSupport.PostInValidDeleteRequestToService(customerServiceSupport.InvalidDeleteCustomerModel.CustomerUID,
        customerServiceSupport.InvalidDeleteCustomerModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"CustomerServiceDelete Response With '(.*)' Should Be Returned")]
    public void ThenCustomerServiceDeleteResponseWithShouldBeReturned(string errorMessage)
    {
      customerServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid CustomerServiceDelete ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDeleteActionUTCTo(string actionUtc)
    {
      customerServiceSupport.InvalidDeleteCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    public static CreateCustomerEvent GetDefaultValidCustomerServiceCreateRequest()
    {
      CreateCustomerEvent defaultValidCustomerServiceCreateModel = new CreateCustomerEvent();
      defaultValidCustomerServiceCreateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceCreateModel.CustomerType = 0;
      defaultValidCustomerServiceCreateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceCreateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceCreateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceCreateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceCreateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"); 
      defaultValidCustomerServiceCreateModel.CustomerUID = Guid.NewGuid();
      defaultValidCustomerServiceCreateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidCustomerServiceCreateModel;
    }

    public static UpdateCustomerEvent GetDefaultValidCustomerServiceUpdateRequest()
    {
      UpdateCustomerEvent defaultValidCustomerServiceUpdateModel = new UpdateCustomerEvent();
      defaultValidCustomerServiceUpdateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceUpdateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceUpdateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceUpdateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceUpdateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidCustomerServiceUpdateModel.CustomerUID = Guid.NewGuid();
      defaultValidCustomerServiceUpdateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidCustomerServiceUpdateModel;
    }

    public static DeleteCustomerEvent GetDefaultValidCustomerServiceDeleteRequest()
    {
      DeleteCustomerEvent defaultValidCustomerServiceDeleteModel = new DeleteCustomerEvent();
      defaultValidCustomerServiceDeleteModel.CustomerUID = Guid.NewGuid();
      defaultValidCustomerServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      return defaultValidCustomerServiceDeleteModel;
    }

    public static InvalidCreateCustomerEvent GetDefaultInValidCustomerServiceCreateRequest()
    {
      InvalidCreateCustomerEvent defaultInValidCustomerServiceCreateModel = new InvalidCreateCustomerEvent();
      defaultInValidCustomerServiceCreateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.CustomerType = "Dealer";
      defaultInValidCustomerServiceCreateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidCustomerServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidCustomerServiceCreateModel;
    }

    public static InvalidUpdateCustomerEvent GetDefaultInValidCustomerServiceUpdateRequest()
    {
      InvalidUpdateCustomerEvent defaultInValidCustomerServiceUpdateModel = new InvalidUpdateCustomerEvent();
      defaultInValidCustomerServiceUpdateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceUpdateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceUpdateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceUpdateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceUpdateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidCustomerServiceUpdateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidCustomerServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidCustomerServiceUpdateModel;
    }

    public static InvalidDeleteCustomerEvent GetDefaultInValidCustomerServiceDeleteRequest()
    {
      InvalidDeleteCustomerEvent defaultInValidCustomerServiceDeleteModel = new InvalidDeleteCustomerEvent();
      defaultInValidCustomerServiceDeleteModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidCustomerServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString(); 
      return defaultInValidCustomerServiceDeleteModel;
    }
  }
}
