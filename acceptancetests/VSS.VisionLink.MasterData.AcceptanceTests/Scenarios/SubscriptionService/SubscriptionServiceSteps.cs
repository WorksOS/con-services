using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.SubscriptionService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.SubscriptionService
{
  [Binding]
  public class SubscriptionServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(SubscriptionServiceSteps));
    private static SubscriptionServiceSupport subscriptionServiceSupport = new SubscriptionServiceSupport(Log);

    [Given(@"SubscriptionService Is Ready To Verify '(.*)'")]
    public void GivenSubscriptionServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"SubscriptionServiceCreate Request Is Setup With Default Values")]
    public void GivenSubscriptionServiceCreateRequestIsSetupWithDefaultValues()
    {
      subscriptionServiceSupport.CreateSubscriptionModel = GetDefaultValidSubscriptionServiceCreateRequest();
    }

    [Given(@"SubscriptionServiceUpdate Request Is Setup With Default Values")]
    public void GivenSubscriptionServiceUpdateRequestIsSetupWithDefaultValues()
    {
      subscriptionServiceSupport.UpdateSubscriptionModel = GetDefaultValidSubscriptionServiceUpdateRequest();
    }

    [Given(@"SubscriptionServiceCreate Request Is Setup With Invalid Default Values")]
    public void GivenSubscriptionServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel = GetDefaultInValidSubscriptionServiceCreateRequest();
    }

    [Given(@"SubscriptionServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenSubscriptionServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel = GetDefaultInValidSubscriptionServiceUpdateRequest();
    }

    [When(@"I Post Valid SubscriptionServiceCreate Request")]
    public void WhenIPostValidSubscriptionServiceCreateRequest()
    {
      subscriptionServiceSupport.SetupCreateSubscriptionKafkaConsumer(subscriptionServiceSupport.CreateSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.CreateSubscriptionModel.CustomerUID, subscriptionServiceSupport.CreateSubscriptionModel.ActionUTC);

      subscriptionServiceSupport.PostValidCreateRequestToService();
    }

    [When(@"I Post Valid SubscriptionServiceUpdate Request")]
    public void WhenIPostValidSubscriptionServiceUpdateRequest()
    {
      subscriptionServiceSupport.SetupUpdateSubscriptionKafkaConsumer(subscriptionServiceSupport.UpdateSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.UpdateSubscriptionModel.CustomerUID, subscriptionServiceSupport.UpdateSubscriptionModel.ActionUTC);

      subscriptionServiceSupport.PostValidUpdateRequestToService();
    }

    [When(@"I Set SubscriptionServiceCreate AssetUID To '(.*)'")]
    public void WhenISetSubscriptionServiceCreateAssetUIDTo(string assetUid)
    {
      subscriptionServiceSupport.CreateSubscriptionModel.AssetUID = String.IsNullOrEmpty(InputGenerator.GetValue(assetUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(assetUid));
    }

    [When(@"I Set SubscriptionServiceCreate SubscriptionTypeID To '(.*)'")]
    public void WhenISetSubscriptionServiceCreateSubscriptionTypeIDTo(string subscriptionTypeid)
    {
      subscriptionServiceSupport.CreateSubscriptionModel.SubscriptionTypeID = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), InputGenerator.GetValue(subscriptionTypeid)); 
    }

    [When(@"I Set SubscriptionServiceUpdate AssetUID To '(.*)'")]
    public void WhenISetSubscriptionServiceUpdateAssetUIDTo(string assetUid)
    {
      subscriptionServiceSupport.UpdateSubscriptionModel.AssetUID = String.IsNullOrEmpty(InputGenerator.GetValue(assetUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(assetUid));
    }

    [When(@"I Set SubscriptionServiceUpdate SubscriptionTypeID To '(.*)'")]
    public void WhenISetSubscriptionServiceUpdateSubscriptionTypeIDTo(string subscriptionTypeid)
    {
      subscriptionServiceSupport.UpdateSubscriptionModel.SubscriptionTypeID = String.IsNullOrEmpty(InputGenerator.GetValue(subscriptionTypeid)) ? (SubscriptionType?)null : (SubscriptionType?)Enum.Parse(typeof(SubscriptionType?), InputGenerator.GetValue(subscriptionTypeid)); 
    }

    [When(@"I Set SubscriptionServiceUpdate StartDate  To '(.*)'")]
    public void WhenISetSubscriptionServiceUpdateStartDateTo(string startDate)
    {
      subscriptionServiceSupport.UpdateSubscriptionModel.StartDate = String.IsNullOrEmpty(InputGenerator.GetValue(startDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(startDate.ToString()));
    }

    [When(@"I Set SubscriptionServiceUpdate EndDate To '(.*)'")]
    public void WhenISetSubscriptionServiceUpdateEndDateTo(string endDate)
    {
      subscriptionServiceSupport.UpdateSubscriptionModel.StartDate = String.IsNullOrEmpty(InputGenerator.GetValue(endDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(endDate.ToString()));
    }

    [When(@"I Set Invalid SubscriptionServiceCreate SubscriptionUID  To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateSubscriptionUIDTo(string subscriptionUid)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.SubscriptionUID = InputGenerator.GetValue(subscriptionUid);
    }

    [When(@"I Post Invalid SubscriptionServiceCreate Request")]
    public void WhenIPostInvalidSubscriptionServiceCreateRequest()
    {
      string contentType = "application/json";
      subscriptionServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate CustomerUID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateCustomerUIDTo(string customerUid)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate AssetUID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateAssetUIDTo(string assetUid)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate SubscriptionTypeID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateSubscriptionTypeIDTo(string subscriptionTypeid)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.SubscriptionTypeID = InputGenerator.GetValue(subscriptionTypeid);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate StartDate To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateStartDateTo(string startDate)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.StartDate = InputGenerator.GetValue(startDate);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate EndDate To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateEndDateTo(string endDate)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.EndDate = InputGenerator.GetValue(endDate);
    }

    [When(@"I Set Invalid SubscriptionServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceCreateActionUTCTo(string actionUtc)
    {
      subscriptionServiceSupport.InvalidCreateSubscriptionModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate SubscriptionUID  To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateSubscriptionUIDTo(string subscriptionUid)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.SubscriptionUID = InputGenerator.GetValue(subscriptionUid);
    }

    [When(@"I Post Invalid SubscriptionServiceUpdate Request")]
    public void WhenIPostInvalidSubscriptionServiceUpdateRequest()
    {
      string contentType = "application/json";
      subscriptionServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate CustomerUID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateCustomerUIDTo(string customerUid)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate AssetUID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateAssetUIDTo(string assetUid)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate SubscriptionTypeID To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateSubscriptionTypeIDTo(string subscriptionTypeid)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.SubscriptionTypeID = InputGenerator.GetValue(subscriptionTypeid);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate StartDate To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateStartDateTo(string startDate)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.StartDate = InputGenerator.GetValue(startDate);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate EndDate To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateEndDateTo(string endDate)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.EndDate = InputGenerator.GetValue(endDate);
    }

    [When(@"I Set Invalid SubscriptionServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidSubscriptionServiceUpdateActionUTCTo(string actionUtc)
    {
      subscriptionServiceSupport.InvalidUpdateSubscriptionModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"The Processed SubscriptionServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedSubscriptionServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      CreateSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForSubscriptionCreateHandler.subscriptionEvent;
      Assert.IsTrue(subscriptionServiceSupport._checkForSubscriptionCreateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      subscriptionServiceSupport.VerifySubscriptionServiceCreateResponse(kafkaresponse);
    }

    [Then(@"The Processed SubscriptionServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedSubscriptionServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      UpdateSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForSubscriptionUpdateHandler.subscriptionEvent;
      Assert.IsTrue(subscriptionServiceSupport._checkForSubscriptionUpdateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      subscriptionServiceSupport.VerifySubscriptionServiceUpdateResponse(kafkaresponse);
    }

    [Then(@"SubscriptionServiceCreate Response With '(.*)' Should Be Returned")]
    public void ThenSubscriptionServiceCreateResponseWithShouldBeReturned(string errorMessage)
    {
      subscriptionServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"SubscriptionServiceUpdate Response With '(.*)' Should Be Returned")]
    public void ThenSubscriptionServiceUpdateResponseWithShouldBeReturned(string errorMessage)
    {
      subscriptionServiceSupport.VerifyErrorResponse(errorMessage);
    }

    public static CreateSubscriptionEvent GetDefaultValidSubscriptionServiceCreateRequest()
    {
      CreateSubscriptionEvent defaultValidSubscriptionServiceCreateModel = new CreateSubscriptionEvent();
      defaultValidSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid();
      defaultValidSubscriptionServiceCreateModel.CustomerUID = Guid.NewGuid();
      defaultValidSubscriptionServiceCreateModel.SubscriptionTypeID = SubscriptionType.Essentials;
      defaultValidSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow;
      defaultValidSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10); 
      defaultValidSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidSubscriptionServiceCreateModel;
    }

    public static UpdateSubscriptionEvent GetDefaultValidSubscriptionServiceUpdateRequest()
    {
      UpdateSubscriptionEvent defaultValidSubscriptionServiceUpdateModel = new UpdateSubscriptionEvent();
      defaultValidSubscriptionServiceUpdateModel.SubscriptionUID = Guid.NewGuid();
      defaultValidSubscriptionServiceUpdateModel.CustomerUID = Guid.NewGuid();
      defaultValidSubscriptionServiceUpdateModel.SubscriptionTypeID = SubscriptionType.CATMAINT;
      defaultValidSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow;
      defaultValidSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow;
      defaultValidSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddYears(10);
      return defaultValidSubscriptionServiceUpdateModel;
    }

    public static InvalidCreateSubscriptionEvent GetDefaultInValidSubscriptionServiceCreateRequest()
    {
      InvalidCreateSubscriptionEvent defaultInValidSubscriptionServiceCreateModel = new InvalidCreateSubscriptionEvent();
      defaultInValidSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid().ToString();
      defaultInValidSubscriptionServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidSubscriptionServiceCreateModel.SubscriptionTypeID = "StandardHealth";
      defaultInValidSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow.ToString();
      defaultInValidSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
      defaultInValidSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidSubscriptionServiceCreateModel;
    }

    public static InvalidUpdateSubscriptionEvent GetDefaultInValidSubscriptionServiceUpdateRequest()
    {
      InvalidUpdateSubscriptionEvent defaultInValidSubscriptionServiceUpdateModel = new InvalidUpdateSubscriptionEvent();
      defaultInValidSubscriptionServiceUpdateModel.SubscriptionUID = Guid.NewGuid().ToString();
      defaultInValidSubscriptionServiceUpdateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidSubscriptionServiceUpdateModel.SubscriptionTypeID = "ManualMaintenanceLog";
      defaultInValidSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow.ToString();
      defaultInValidSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
      defaultInValidSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidSubscriptionServiceUpdateModel;
    }
  }
}
