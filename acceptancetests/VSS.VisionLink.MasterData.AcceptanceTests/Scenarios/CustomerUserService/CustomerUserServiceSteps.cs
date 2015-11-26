using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerUserService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerUserService
{
  [Binding]
  public class CustomerUserServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerUserServiceSteps));
    private static CustomerUserServiceSupport customerUserServiceSupport = new CustomerUserServiceSupport(Log);

    [Given(@"CustomerUserService Is Ready To Verify '(.*)'")]
    public void GivenCustomerUserServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerUserServiceAssociate Request Is Setup With Default Values")]
    public void GivenCustomerUserServiceAssociateRequestIsSetupWithDefaultValues()
    {
      customerUserServiceSupport.AssociateCustomerUserModel = GetDefaultValidAssociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceDissociate Request Is Setup With Default Values")]
    public void GivenCustomerUserServiceDissociateRequestIsSetupWithDefaultValues()
    {
      customerUserServiceSupport.DissociateCustomerUserModel = GetDefaultValidDissociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceAssociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerUserServiceAssociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel = GetDefaultInValidAssociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceDissociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerUserServiceDissociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel = GetDefaultInValidDissociateCustomerUserServiceRequest();
    }

    [When(@"I Post Valid CustomerUserServiceAssociate Request")]
    public void WhenIPostValidCustomerUserServiceAssociateRequest()
    {
      customerUserServiceSupport.SetupCustomerUserAssociateKafkaConsumer(customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID,
        customerUserServiceSupport.AssociateCustomerUserModel.UserUID, customerUserServiceSupport.AssociateCustomerUserModel.ActionUTC);

      customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
    }

    [When(@"I Post Valid CustomerUserServiceDissociate Request")]
    public void WhenIPostValidCustomerUserServiceDissociateRequest()
    {
      customerUserServiceSupport.SetupCustomerUserDissociateKafkaConsumer(customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID,
        customerUserServiceSupport.DissociateCustomerUserModel.UserUID, customerUserServiceSupport.DissociateCustomerUserModel.ActionUTC);

      customerUserServiceSupport.PostValidCustomerUserDissociateRequestToService();
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateCustomerUIDTo(string customerUid)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerUserServiceAssociate Request")]
    public void WhenIPostInvalidCustomerUserServiceAssociateRequest()
    {
      string contentType = "application/json";
      customerUserServiceSupport.PostInValidCustomerUserAssociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate UserUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateUserUIDTo(string userUid)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateActionUTCTo(string actionUtc)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateCustomerUIDTo(string customerUid)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerUserServiceDissociate Request")]
    public void WhenIPostInvalidCustomerUserServiceDissociateRequest()
    {
      string contentType = "application/json";
      customerUserServiceSupport.PostInValidCustomerUserDissociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate UserUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateUserUIDTo(string userUid)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateActionUTCTo(string actionUtc)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"The Processed CustomerUserServiceAssociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerUserServiceAssociateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerUserServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      AssociateCustomerUserModel kafkaresponse = customerUserServiceSupport._checkForCustomerUserAssociateHandler.CustomerEvent;
      Assert.IsTrue(customerUserServiceSupport._checkForCustomerUserAssociateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerUserServiceSupport.VerifyCustomerUserAssociateServiceResponse(kafkaresponse);
    }

    [Then(@"The Processed CustomerUserServiceDissociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerUserServiceDissociateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerUserServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      DissociateCustomerUserModel kafkaresponse = customerUserServiceSupport._checkForCustomerUserDissociateHandler.CustomerEvent;
      Assert.IsTrue(customerUserServiceSupport._checkForCustomerUserDissociateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerUserServiceSupport.VerifyCustomerUserDissociateServiceResponse(kafkaresponse);
    }

    [Then(@"CustomerUserServiceAssociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerUserServiceAssociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerUserServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"CustomerUserServiceDissociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerUserServiceDissociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerUserServiceSupport.VerifyErrorResponse(errorMessage);
    }

    public static AssociateCustomerUserEvent GetDefaultValidAssociateCustomerUserServiceRequest()
    {
      AssociateCustomerUserEvent defaultValidAssociateCustomerAssetServiceModel = new AssociateCustomerUserEvent();
      defaultValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.UserUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      return defaultValidAssociateCustomerAssetServiceModel;
    }

    public static DissociateCustomerUserEvent GetDefaultValidDissociateCustomerUserServiceRequest()
    {
      DissociateCustomerUserEvent defaultValidDissociateCustomerAssetServiceModel = new DissociateCustomerUserEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid();
      defaultValidDissociateCustomerAssetServiceModel.UserUID = Guid.NewGuid();
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      return defaultValidDissociateCustomerAssetServiceModel;
    }

    public static InvalidAssociateCustomerUserEvent GetDefaultInValidAssociateCustomerUserServiceRequest()
    {
      InvalidAssociateCustomerUserEvent defaultInValidAssociateCustomerAssetServiceModel = new InvalidAssociateCustomerUserEvent();
      defaultInValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidAssociateCustomerAssetServiceModel.UserUID = Guid.NewGuid().ToString(); ;
      defaultInValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidAssociateCustomerAssetServiceModel;
    }

    public static InvalidDissociateCustomerUserEvent GetDefaultInValidDissociateCustomerUserServiceRequest()
    {
      InvalidDissociateCustomerUserEvent defaultValidDissociateCustomerAssetServiceModel = new InvalidDissociateCustomerUserEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString(); ;
      defaultValidDissociateCustomerAssetServiceModel.UserUID = Guid.NewGuid().ToString(); ;
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString(); ;
      return defaultValidDissociateCustomerAssetServiceModel;
    }
  }
}
