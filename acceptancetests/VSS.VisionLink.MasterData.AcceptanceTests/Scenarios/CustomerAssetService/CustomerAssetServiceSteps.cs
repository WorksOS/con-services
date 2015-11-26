using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.CustomerAssetService
{
  [Binding]
  public class CustomerAssetServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerAssetServiceSteps));
    private static CustomerAssetServiceSupport customerAssetServiceSupport = new CustomerAssetServiceSupport(Log);

    [Given(@"CustomerAssetService Is Ready To Verify '(.*)'")]
    public void GivenCustomerAssetServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerAssetServiceAssociate Request Is Setup With Default Values")]
    public void GivenCustomerAssetServiceAssociateRequestIsSetupWithDefaultValues()
    {
      customerAssetServiceSupport.AssociateCustomerAssetModel = GetDefaultValidAssociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceDissociate Request Is Setup With Default Values")]
    public void GivenCustomerAssetServiceDissociateRequestIsSetupWithDefaultValues()
    {
      customerAssetServiceSupport.DissociateCustomerAssetModel = GetDefaultValidDissociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerAssetServiceAssociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel = GetDefaultInValidAssociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceDissociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerAssetServiceDissociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel = GetDefaultInValidDissociateCustomerAssetServiceRequest();
    }

    [When(@"I Post Valid CustomerAssetServiceAssociate Request")]
    public void WhenIPostValidCustomerAssetServiceAssociateRequest()
    {
      customerAssetServiceSupport.SetupCustomerAssetAssociateKafkaConsumer(customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID,
        customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID, customerAssetServiceSupport.AssociateCustomerAssetModel.ActionUTC);

      customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
    }

    [When(@"I Post Valid CustomerAssetServiceDissociate Request")]
    public void WhenIPostValidCustomerAssetServiceDissociateRequest()
    {
      customerAssetServiceSupport.SetupCustomerAssetDissociateKafkaConsumer(customerAssetServiceSupport.DissociateCustomerAssetModel.CustomerUID,
        customerAssetServiceSupport.DissociateCustomerAssetModel.AssetUID, customerAssetServiceSupport.DissociateCustomerAssetModel.ActionUTC);

      customerAssetServiceSupport.PostValidCustomerAssetDissociateRequestToService();
    }

    [When(@"I Set CustomerAssetServiceAssociate RelationType To '(.*)'")]
    public void WhenISetCustomerAssetServiceAssociateRelationTypeTo(RelationType relationType)
    {
      customerAssetServiceSupport.AssociateCustomerAssetModel.RelationType = relationType;
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate RelationType To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateRelationTypeTo(string relationType)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.RelationType = InputGenerator.GetValue(relationType);
    }

    [When(@"I Post Invalid CustomerAssetServiceAssociate Request")]
    public void WhenIPostInvalidCustomerAssetServiceAssociateRequest()
    {
      string contentType = "application/json";
      customerAssetServiceSupport.PostInValidCustomerAssetAssociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateCustomerUIDTo(string customerUid)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate AssetUID To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateAssetUIDTo(string assetUid)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateActionUTCTo(string actionUtc)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Post Invalid CustomerAssetServiceDissociate Request")]
    public void WhenIPostInvalidCustomerAssetServiceDissociateRequest()
    {
      string contentType = "application/json";
      customerAssetServiceSupport.PostInValidCustomerAssetDissociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerServiceDissociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateCustomerUIDTo(string customerUid)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid CustomerServiceDissociate AssetUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateAssetUIDTo(string assetUid)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid CustomerServiceDissociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateActionUTCTo(string actionUtc)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"The Processed CustomerAssetServiceAssociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerAssetServiceAssociateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerAssetServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      AssociateCustomerAssetModel kafkaresponse = customerAssetServiceSupport._checkForCustomerAssetAssociateHandler.CustomerEvent;
      Assert.IsTrue(customerAssetServiceSupport._checkForCustomerAssetAssociateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerAssetServiceSupport.VerifyCustomerAssetAssociateServiceResponse(kafkaresponse);
    }

    [Then(@"The Processed CustomerAssetServiceDissociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerAssetServiceDissociateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => customerAssetServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      DissociateCustomerAssetModel kafkaresponse = customerAssetServiceSupport._checkForCustomerAssetDissociateHandler.CustomerEvent;
      Assert.IsTrue(customerAssetServiceSupport._checkForCustomerAssetDissociateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      customerAssetServiceSupport.VerifyCustomerAssetDissociateServiceResponse(kafkaresponse);
    }

    [Then(@"CustomerAssetServiceAssociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerAssetServiceAssociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerAssetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"CustomerAssetServiceDissociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerAssetServiceDissociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerAssetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    public static  AssociateCustomerAssetEvent GetDefaultValidAssociateCustomerAssetServiceRequest()
    {
      AssociateCustomerAssetEvent defaultValidAssociateCustomerAssetServiceModel = new AssociateCustomerAssetEvent();
      defaultValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.RelationType = 0;
      defaultValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      return defaultValidAssociateCustomerAssetServiceModel;
    }

    public static DissociateCustomerAssetEvent GetDefaultValidDissociateCustomerAssetServiceRequest()
    {
      DissociateCustomerAssetEvent defaultValidDissociateCustomerAssetServiceModel = new DissociateCustomerAssetEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid();
      defaultValidDissociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid();
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      return defaultValidDissociateCustomerAssetServiceModel;
    }

    public static InvalidAssociateCustomerAssetEvent GetDefaultInValidAssociateCustomerAssetServiceRequest()
    {
      InvalidAssociateCustomerAssetEvent defaultInValidAssociateCustomerAssetServiceModel = new InvalidAssociateCustomerAssetEvent();
      defaultInValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidAssociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid().ToString(); ;
      defaultInValidAssociateCustomerAssetServiceModel.RelationType = "Customer";
      defaultInValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString(); 
      return defaultInValidAssociateCustomerAssetServiceModel;
    }

    public static InvalidDissociateCustomerAssetEvent GetDefaultInValidDissociateCustomerAssetServiceRequest()
    {
      InvalidDissociateCustomerAssetEvent defaultValidDissociateCustomerAssetServiceModel = new InvalidDissociateCustomerAssetEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString(); ;
      defaultValidDissociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid().ToString(); ;
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString(); ;
      return defaultValidDissociateCustomerAssetServiceModel;
    }
  }
}
