using System;
using TechTalk.SpecFlow;
using AutomationCore.Shared.Library;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.KafkaWrapper;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using System.Threading.Tasks;
using VSS.KafkaWrapper.Models;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.GeofenceService
{
  [Binding]
  public class GeofenceServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(GeofenceServiceSteps));
    private static GeofenceServiceSupport geofenceServiceSupport = new GeofenceServiceSupport(Log);

    [Given(@"GeofenceService Is Ready To Verify '(.*)'")]
    public void GivenGeofenceServiceIsReadyToVerify(string TestDescription)
    {
      {
        //log the scenario info
        TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
        //TestName = TestDescription;
        LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
      }
    }

    [Given(@"GeofenceServiceCreate Request Is Setup With Default Values")]
    public void GivenGeofenceServiceCreateRequestIsSetupWithDefaultValues()
    {
      geofenceServiceSupport.CreateGeofenceModel = GetDefaultValidGeofenceServiceCreateRequest();
    }

    [When(@"I Post Valid GeofenceServiceCreate Request")]
    public void WhenIPostValidGeofenceServiceCreateRequest()
    {
      geofenceServiceSupport.SetupCreateGeofenceKafkaConsumer(geofenceServiceSupport.CreateGeofenceModel.UserUID,
        geofenceServiceSupport.CreateGeofenceModel.GeofenceUID, geofenceServiceSupport.CreateGeofenceModel.ActionUTC);

      geofenceServiceSupport.PostValidCreateRequestToService();
    }

    [When(@"I Set GeofenceServiceCreate Description To '(.*)'")]
    public void WhenISetGeofenceServiceCreateDescriptionTo(string description)
    {
      geofenceServiceSupport.CreateGeofenceModel.Description = InputGenerator.GetValue(description);
    }

    [When(@"I Set GeofenceServiceCreate FillColor To '(.*)'")]
    public void WhenISetGeofenceServiceCreateFillColorTo(string fillColor)
    {
      geofenceServiceSupport.CreateGeofenceModel.FillColor = String.IsNullOrEmpty(InputGenerator.GetValue(fillColor)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(fillColor));
    }

    [When(@"I Set GeofenceServiceCreate IsTransparent To '(.*)'")]
    public void WhenISetGeofenceServiceCreateIsTransparentTo(string isTransparent)
    {
      geofenceServiceSupport.CreateGeofenceModel.IsTransparent = String.IsNullOrEmpty(InputGenerator.GetValue(isTransparent)) ? null : (bool?)bool.Parse(InputGenerator.GetValue(isTransparent));
    }

    [When(@"I Set GeofenceServiceUpdate GeofenceName To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateGeofenceNameTo(string geofenceName)
    {
      geofenceServiceSupport.UpdateGeofenceModel.GeofenceName = InputGenerator.GetValue(geofenceName);
    }

    [When(@"I Set GeofenceServiceUpdate GeofenceType To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateGeofenceTypeTo(string geofenceType)
    {
      geofenceServiceSupport.UpdateGeofenceModel.GeofenceType = String.IsNullOrEmpty(InputGenerator.GetValue(geofenceType)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(geofenceType));
    }

    [When(@"I Set GeofenceServiceUpdate Description To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateDescriptionTo(string description)
    {

      geofenceServiceSupport.UpdateGeofenceModel.Description = InputGenerator.GetValue(description);
    }

    [When(@"I Set GeofenceServiceUpdate GeometryWKT To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateGeometryWKTTo(string geometryWkt)
    {

      geofenceServiceSupport.UpdateGeofenceModel.GeometryWKT = InputGenerator.GetValue(geometryWkt);
    }

    [When(@"I Set GeofenceServiceUpdate FillColor To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateFillColorTo(string fillColor)
    {
      geofenceServiceSupport.UpdateGeofenceModel.FillColor = String.IsNullOrEmpty(InputGenerator.GetValue(fillColor)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(fillColor));
    }

    [When(@"I Set GeofenceServiceUpdate IsTransparent To '(.*)'")]
    public void WhenISetGeofenceServiceUpdateIsTransparentTo(string isTransparent)
    {
      geofenceServiceSupport.UpdateGeofenceModel.IsTransparent = String.IsNullOrEmpty(InputGenerator.GetValue(isTransparent)) ? null : (bool?)bool.Parse(InputGenerator.GetValue(isTransparent));
    }

    [Then(@"The Processed GeofenceServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedGeofenceServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => geofenceServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      CreateGeofenceModel kafkaresponse = geofenceServiceSupport._checkForGeoFenceCreateHandler.geofenceEvent;
      Assert.IsTrue(geofenceServiceSupport._checkForGeoFenceCreateHandler.HasFound()); //Asserts that the CreateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      geofenceServiceSupport.VerifyGeofenceServiceCreateResponse(kafkaresponse);
    }

    [Given(@"GeofenceServiceUpdate Request Is Setup With Default Values")]
    public void GivenGeofenceServiceUpdateRequestIsSetupWithDefaultValues()
    {
      geofenceServiceSupport.UpdateGeofenceModel = GetDefaultValidGeofenceServiceUpdateRequest();
    }

    [When(@"I Post Invalid GeofenceServiceCreate Request")]
    public void WhenIPostInvalidGeofenceServiceCreateRequest()
    {
      string contentType = "application/json";
      geofenceServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Post Invalid GeofenceServiceUpdate Request")]
    public void WhenIPostInvalidGeofenceServiceUpdateRequest()
    {
      string contentType = "application/json";
      geofenceServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid GeofenceServiceCreate CustomerUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateCustomerUIDTo(string customerUid)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid GeofenceServiceCreate GeofenceName To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateGeofenceNameTo(string GeofenceName)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.GeofenceName = InputGenerator.GetValue(GeofenceName);
    }

    [When(@"I Set Invalid GeofenceServiceCreate GeofenceType To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateGeofenceTypeTo(string geofenceType)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.GeofenceType = InputGenerator.GetValue(geofenceType);
    }

    [When(@"I Set Invalid GeofenceServiceCreate GeometryWKT To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateGeometryWKTTo(string geometryWkt)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.GeometryWKT = InputGenerator.GetValue(geometryWkt);
    }

    [When(@"I Set Invalid GeofenceServiceCreate FillColor To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateFillColorTo(string fillColor)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.FillColor = InputGenerator.GetValue(fillColor);
    }

    [When(@"I Set Invalid GeofenceServiceCreate IsTransparent To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateIsTransparentTo(string isTransparent)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.IsTransparent = InputGenerator.GetValue(isTransparent);
    }

    [When(@"I Set Invalid GeofenceServiceCreate GeofenceUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateGeofenceUIDTo(string geofenceUid)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.GeofenceUID = InputGenerator.GetValue(geofenceUid);
    }

    [When(@"I Set Invalid GeofenceServiceCreate UserUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateUserUIDTo(string userUid)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid GeofenceServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceCreateActionUTCTo(string actionUtc)
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Set Invalid GeofenceServiceUpdate FillColor To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceUpdateFillColorTo(string fillColor)
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel.FillColor = InputGenerator.GetValue(fillColor);
    }

    [When(@"I Set Invalid GeofenceServiceUpdate IsTransparent To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceUpdateIsTransparentTo(string isTransparent)
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel.IsTransparent = InputGenerator.GetValue(isTransparent);
    }

    [When(@"I Set Invalid GeofenceServiceUpdate GeofenceUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceUpdateGeofenceUIDTo(string geofenceUid)
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel.GeofenceUID = InputGenerator.GetValue(geofenceUid);
    }

    [When(@"I Set Invalid GeofenceServiceUpdate UserUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceUpdateUserUIDTo(string userUid)
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid GeofenceServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceUpdateActionUTCTo(string actionUtc)
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"GeofenceServiceCreate Response With '(.*)' Should Be Returned")]
    public void ThenGeofenceServiceCreateResponseWithShouldBeReturned(string errorMessage)
    {
      geofenceServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Post Valid GeofenceServiceUpdate Request")]
    public void WhenIPostValidGeofenceServiceUpdateRequest()
    {
      geofenceServiceSupport.SetupUpdateGeofenceKafkaConsumer(geofenceServiceSupport.UpdateGeofenceModel.UserUID,
              geofenceServiceSupport.UpdateGeofenceModel.GeofenceUID, geofenceServiceSupport.UpdateGeofenceModel.ActionUTC);

      geofenceServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The Processed GeofenceServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedGeofenceServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => geofenceServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      UpdateGeofenceModel kafkaresponse = geofenceServiceSupport._checkForGeoFenceUpdateHandler.geofenceEvent;
      Assert.IsTrue(geofenceServiceSupport._checkForGeoFenceUpdateHandler.HasFound()); //Asserts that the UpdateGeofenceEvent has published into the GeofenceKafkaTopic by validating the presence of the particular useruid,geofenceuid and actionutc
      geofenceServiceSupport.VerifyGeofenceServiceUpdateResponse(kafkaresponse);
    }

    [Given(@"GeofenceServiceDelete Request Is Setup With Default Values")]
    public void GivenGeofenceServiceDeleteRequestIsSetupWithDefaultValues()
    {
      geofenceServiceSupport.DeleteGeofenceModel = GetDefaultValidGeofenceServiceDeleteRequest();
    }

    [When(@"I Post Valid GeofenceServiceDelete Request")]
    public void WhenIPostValidGeofenceServiceDeleteRequest()
    {
      geofenceServiceSupport.SetupDeleteGeofenceKafkaConsumer(geofenceServiceSupport.DeleteGeofenceModel.UserUID,
              geofenceServiceSupport.DeleteGeofenceModel.GeofenceUID, geofenceServiceSupport.DeleteGeofenceModel.ActionUTC);

      geofenceServiceSupport.PostValidDeleteRequestToService(geofenceServiceSupport.DeleteGeofenceModel.UserUID,
              geofenceServiceSupport.DeleteGeofenceModel.GeofenceUID, geofenceServiceSupport.DeleteGeofenceModel.ActionUTC);
    }

    [Then(@"The Processed GeofenceServiceDelete Message must be available in Kafka topic")]
    public void ThenTheProcessedGeofenceServiceDeleteMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => geofenceServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      DeleteGeofenceModel kafkaresponse = geofenceServiceSupport._checkForGeoFenceDeleteHandler.geofenceEvent;
      Assert.IsTrue(geofenceServiceSupport._checkForGeoFenceDeleteHandler.HasFound()); //Asserts that the DeleteAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      geofenceServiceSupport.VerifyGeofenceServiceDeleteResponse(kafkaresponse);
    }

    [Then(@"GeofenceServiceUpdate Response With '(.*)' Should Be Returned")]
    public void ThenGeofenceServiceUpdateResponseWithShouldBeReturned(string errorMessage)
    {
      geofenceServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid GeofenceServiceDelete GeofenceUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceDeleteGeofenceUIDTo(string geofenceUid)
    {
      geofenceServiceSupport.InvalidDeleteGeofenceModel.GeofenceUID = InputGenerator.GetValue(geofenceUid);
    }

    [When(@"I Post Invalid GeofenceServiceDelete Request")]
    public void WhenIPostInvalidGeofenceServiceDeleteRequest()
    {
      string contentType = "application/json";
      geofenceServiceSupport.PostInValidDeleteRequestToService(geofenceServiceSupport.InvalidDeleteGeofenceModel.GeofenceUID, geofenceServiceSupport.InvalidDeleteGeofenceModel.UserUID,
        geofenceServiceSupport.InvalidDeleteGeofenceModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"GeofenceServiceDelete Response With '(.*)' Should Be Returned")]
    public void ThenGeofenceServiceDeleteResponseWithShouldBeReturned(string errorMessage)
    {
      geofenceServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid GeofenceServiceDelete UserUID To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceDeleteUserUIDTo(string userUid)
    {
      geofenceServiceSupport.InvalidDeleteGeofenceModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid GeofenceServiceDelete ActionUTC To '(.*)'")]
    public void WhenISetInvalidGeofenceServiceDeleteActionUTCTo(string actionUtc)
    {
      geofenceServiceSupport.InvalidDeleteGeofenceModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }


    [Given(@"GeofenceServiceCreate Request Is Setup With Invalid Default Values")]
    public void GivenGeofenceServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      geofenceServiceSupport.InvalidCreateGeofenceModel = GetDefaultInValidGeofenceServiceCreateRequest();
    }

    [Given(@"GeofenceServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenGeofenceServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      geofenceServiceSupport.InvalidUpdateGeofenceModel = GetDefaultInValidGeofenceServiceUpdateRequest();
    }

    [Given(@"GeofenceServiceDelete Request Is Setup With Invalid Default Values")]
    public void GivenGeofenceServiceDeleteRequestIsSetupWithInvalidDefaultValues()
    {
      geofenceServiceSupport.InvalidDeleteGeofenceModel = GetDefaultInValidGeofenceServiceDeleteRequest();
    }


    public static CreateGeofenceEvent GetDefaultValidGeofenceServiceCreateRequest()
    {
      CreateGeofenceEvent defaultValidGeofenceServiceCreateModel = new CreateGeofenceEvent();
      defaultValidGeofenceServiceCreateModel.CustomerUID = Guid.NewGuid();
      defaultValidGeofenceServiceCreateModel.GeofenceName = "AutoTestAPIGeofenceName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceCreateModel.Description = "AutoTestAPIDescription" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceCreateModel.GeofenceType = geofenceServiceSupport.RandomNumber();
      defaultValidGeofenceServiceCreateModel.GeometryWKT = "AutoTestAPIGeometryWKT" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceCreateModel.FillColor = geofenceServiceSupport.RandomNumber();
      defaultValidGeofenceServiceCreateModel.IsTransparent = true;
      defaultValidGeofenceServiceCreateModel.GeofenceUID = Guid.NewGuid();
      defaultValidGeofenceServiceCreateModel.UserUID = Guid.NewGuid();
      defaultValidGeofenceServiceCreateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGeofenceServiceCreateModel;
    }

    public static UpdateGeofenceEvent GetDefaultValidGeofenceServiceUpdateRequest()
    {
      UpdateGeofenceEvent defaultValidGeofenceServiceUpdateModel = new UpdateGeofenceEvent();
      defaultValidGeofenceServiceUpdateModel.GeofenceName = "AutoTestAPIGeofenceName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceUpdateModel.Description = "AutoTestAPIDescription" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceUpdateModel.GeofenceType = geofenceServiceSupport.RandomNumber();
      defaultValidGeofenceServiceUpdateModel.GeometryWKT = "AutoTestAPIGeometryWKT" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGeofenceServiceUpdateModel.FillColor = geofenceServiceSupport.RandomNumber();
      defaultValidGeofenceServiceUpdateModel.IsTransparent = true;
      defaultValidGeofenceServiceUpdateModel.GeofenceUID = Guid.NewGuid();
      defaultValidGeofenceServiceUpdateModel.UserUID = Guid.NewGuid();
      defaultValidGeofenceServiceUpdateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGeofenceServiceUpdateModel;
    }

    public static DeleteGeofenceEvent GetDefaultValidGeofenceServiceDeleteRequest()
    {
      DeleteGeofenceEvent defaultValidGeofenceServiceDeleteModel = new DeleteGeofenceEvent();
      defaultValidGeofenceServiceDeleteModel.GeofenceUID = Guid.NewGuid();
      defaultValidGeofenceServiceDeleteModel.UserUID = Guid.NewGuid();
      defaultValidGeofenceServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGeofenceServiceDeleteModel;
    }

    public static InvalidCreateGeofenceEvent GetDefaultInValidGeofenceServiceCreateRequest()
    {
      InvalidCreateGeofenceEvent defaultInValidGeofenceServiceCreateModel = new InvalidCreateGeofenceEvent();
      defaultInValidGeofenceServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceCreateModel.GeofenceName = "AutoTestAPIGeofenceName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceCreateModel.Description = "AutoTestAPIDescription" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceCreateModel.GeofenceType = "AutoTestAPIGeofenceType" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceCreateModel.GeometryWKT = "AutoTestAPIGeometryWKT" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceCreateModel.FillColor = geofenceServiceSupport.RandomNumber().ToString();
      defaultInValidGeofenceServiceCreateModel.IsTransparent = true.ToString();
      defaultInValidGeofenceServiceCreateModel.GeofenceUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceCreateModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGeofenceServiceCreateModel;
    }

    public static InvalidUpdateGeofenceEvent GetDefaultInValidGeofenceServiceUpdateRequest()
    {
      InvalidUpdateGeofenceEvent defaultInValidGeofenceServiceUpdateModel = new InvalidUpdateGeofenceEvent();
      defaultInValidGeofenceServiceUpdateModel.GeofenceName = "AutoTestAPIGeofenceName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceUpdateModel.Description = "AutoTestAPIDescription" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceUpdateModel.GeofenceType = "AutoTestAPIGeofenceType" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceUpdateModel.GeometryWKT = "AutoTestAPIGeometryWKT" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGeofenceServiceUpdateModel.FillColor = geofenceServiceSupport.RandomNumber().ToString();
      defaultInValidGeofenceServiceUpdateModel.IsTransparent = true.ToString();
      defaultInValidGeofenceServiceUpdateModel.GeofenceUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceUpdateModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGeofenceServiceUpdateModel;
    }

    public static InvalidDeleteGeofenceEvent GetDefaultInValidGeofenceServiceDeleteRequest()
    {
      InvalidDeleteGeofenceEvent defaultInValidGeofenceServiceDeleteModel = new InvalidDeleteGeofenceEvent();
      defaultInValidGeofenceServiceDeleteModel.GeofenceUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceDeleteModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGeofenceServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGeofenceServiceDeleteModel;
    }
  }
}
