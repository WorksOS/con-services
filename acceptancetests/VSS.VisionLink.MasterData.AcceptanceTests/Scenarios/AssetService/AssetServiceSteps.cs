using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using AutomationCore.API.Framework.Common.Features.BSS;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using VSS.KafkaWrapper;
using VSS.KafkaWrapper.Models;
using System.Net;


namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.AssetService
{
  [Binding]
  public class AssetServiceSteps
  {
    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(AssetServiceSteps));
    private static AssetServiceSupport assetServiceSupport = new AssetServiceSupport(Log);

    [Given(@"AssetService Is Ready To Verify '(.*)'")]
    public void GivenAssetServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"AssetServiceCreate Request Is Setup With Default Values")]
    public void GivenAssetServiceCreateRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
    }

    [When(@"I Post Valid AssetServiceCreate Request")]
    public void WhenIPostValidAssetServiceCreateRequest()
    {
      assetServiceSupport.SetupCreateAssetKafkaConsumer(assetServiceSupport.CreateAssetModel.AssetUID, assetServiceSupport.CreateAssetModel.ActionUTC);
      assetServiceSupport.PostValidCreateRequestToService();
    }

    [Then(@"The Processed AssetServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => assetServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      CreateAssetModel kafkaresponse = assetServiceSupport._checkForAssetCreateHandler.assetEvent;
      Assert.IsTrue(assetServiceSupport._checkForAssetCreateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      assetServiceSupport.VerifyAssetServiceCreateResponse(kafkaresponse);
    }

    [Given(@"AssetServiceUpdate Request Is Setup With Default Values")]
    public void GivenAssetServiceUpdateRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.UpdateAssetModel = GetDefaultValidAssetServiceUpdateRequest();
    }

    [When(@"I Post Valid AssetServiceUpdate Request")]
    public void WhenIPostValidAssetServiceUpdateRequest()
    {
      assetServiceSupport.SetupUpdateAssetKafkaConsumer(assetServiceSupport.UpdateAssetModel.AssetUID, assetServiceSupport.UpdateAssetModel.ActionUTC);
      assetServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The Processed AssetServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => assetServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      UpdateAssetModel kafkaresponse = assetServiceSupport._checkForAssetUpdateHandler.assetEvent;
      Assert.IsTrue(assetServiceSupport._checkForAssetUpdateHandler.HasFound());//Asserts that the UpdateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      assetServiceSupport.VerifyAssetServiceUpdateResponse(kafkaresponse);
    }


    [Given(@"AssetServiceDelete Request Is Setup With Default Values")]
    public void GivenAssetServiceDeleteRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.DeleteAssetModel = GetDefaultValidAssetServiceDeleteRequest();

    }

    [Given(@"AssetServiceCreate Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidCreateAssetModel = GetDefaultInValidAssetServiceCreateRequest();
    }

    [Given(@"AssetServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidUpdateAssetModel = GetDefaultInValidAssetServiceUpdateRequest();
    }

    [Given(@"AssetServiceDelete Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceDeleteRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidDeleteAssetModel = GetDefaultInValidAssetServiceDeleteRequest();
    }


    [When(@"I Post Valid AssetServiceDelete Request")]
    public void WhenIPostValidAssetServiceDeleteRequest()
    {
      assetServiceSupport.SetupDeleteAssetKafkaConsumer(assetServiceSupport.DeleteAssetModel.AssetUID, assetServiceSupport.DeleteAssetModel.ActionUTC);
      assetServiceSupport.PostValidDeleteRequestToService(assetServiceSupport.DeleteAssetModel.AssetUID, assetServiceSupport.DeleteAssetModel.ActionUTC);
    }

    [Then(@"The Processed AssetServiceDelete Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceDeleteMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => assetServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 10));
      DeleteAssetModel kafkaresponse = assetServiceSupport._checkForAssetDeleteHandler.assetEvent;
      Assert.IsTrue(assetServiceSupport._checkForAssetDeleteHandler.HasFound()); //Asserts that the DeleteAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      assetServiceSupport.VerifyAssetServiceDeleteResponse(kafkaresponse);
    }

    [When(@"I Set AssetServiceCreate AssetName To '(.*)'")]
    public void WhenISetAssetServiceCreateAssetNameTo(string assetName)
    {
      assetServiceSupport.CreateAssetModel.AssetName = InputGenerator.GetValue(assetName);
    }

    [When(@"I Set AssetServiceCreate AssetType To '(.*)'")]
    public void WhenISetAssetServiceCreateAssetTypeTo(string assetType)
    {
      assetServiceSupport.CreateAssetModel.AssetType = InputGenerator.GetValue(assetType);
    }

    [When(@"I Set AssetServiceCreate Model To '(.*)'")]
    public void WhenISetAssetServiceCreateModelTo(string model)
    {
      assetServiceSupport.CreateAssetModel.Model = InputGenerator.GetValue(model);
    }

    [When(@"I Set AssetServiceCreate EquipmentVIN To '(.*)'")]
    public void WhenISetAssetServiceCreateEquipmentVINTo(string equipmentVin)
    {
      assetServiceSupport.CreateAssetModel.EquipmentVIN = InputGenerator.GetValue(equipmentVin);
    }

    [When(@"I Set AssetServiceCreate IconKey To '(.*)'")]
    public void WhenISetAssetServiceCreateIconKeyTo(string iconKey)
    {
      assetServiceSupport.CreateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(iconKey)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(iconKey));
    }

    [When(@"I Set AssetServiceCreate ModelYear To '(.*)'")]
    public void WhenISetAssetServiceCreateModelYearTo(string modelYear)
    {
      assetServiceSupport.CreateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(modelYear)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(modelYear));
    }

    [When(@"I Set AssetServiceCreate MakeCode To '(.*)'")]
    public void WhenISetAssetServiceCreateMakeCodeTo(string makeCode)
    {
      assetServiceSupport.CreateAssetModel.MakeCode = InputGenerator.GetValue(makeCode);

    }

    [When(@"I Set Invalid AssetServiceCreate AssetName To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetNameTo(string assetName)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetName = InputGenerator.GetValue(assetName);
    }

    [When(@"I Set Invalid AssetServiceCreate AssetType To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetTypeTo(string assetType)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetType = InputGenerator.GetValue(assetType);
    }

    [When(@"I Set Invalid AssetServiceCreate Model To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateModelTo(string model)
    {
      assetServiceSupport.InValidCreateAssetModel.Model = InputGenerator.GetValue(model);
    }

    [When(@"I Set Invalid AssetServiceCreate EquipmentVIN To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateEquipmentVINTo(string equipmentVin)
    {
      assetServiceSupport.InValidCreateAssetModel.EquipmentVIN = InputGenerator.GetValue(equipmentVin);
    }

    [When(@"I Set Invalid AssetServiceCreate IconKey To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateIconKeyTo(string iconKey)
    {
      assetServiceSupport.InValidCreateAssetModel.IconKey = InputGenerator.GetValue(iconKey);
    }

    [When(@"I Set Invalid AssetServiceCreate ModelYear To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateModelYearTo(string modelYear)
    {
      assetServiceSupport.InValidCreateAssetModel.ModelYear = InputGenerator.GetValue(modelYear);
    }

    [When(@"I Set Invalid AssetServiceCreate MakeCode To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateMakeCodeTo(string makeCode)
    {
      assetServiceSupport.InValidCreateAssetModel.MakeCode = InputGenerator.GetValue(makeCode);
    }

    [When(@"I Set Invalid AssetServiceUpdate AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidUpdateAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Post Valid AssetServiceCreate Request With The Below Values")]
    public void WhenIPostValidAssetServiceCreateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.CreateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.CreateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.CreateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.CreateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.CreateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.CreateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.CreateAssetModel.ActionUTC = DateTime.UtcNow;
      assetServiceSupport.SetupCreateAssetKafkaConsumer(assetServiceSupport.CreateAssetModel.AssetUID, assetServiceSupport.CreateAssetModel.ActionUTC);
      Thread.Sleep(new TimeSpan(0, 0, 10));
      assetServiceSupport.PostValidCreateRequestToService();
    }


    [When(@"I Post Valid AssetServiceUpdate Request With The Below Values")]
    public void WhenIPostValidAssetServiceUpdateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.UpdateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.UpdateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.UpdateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.UpdateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.UpdateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.UpdateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.UpdateAssetModel.ActionUTC = DateTime.UtcNow;
      assetServiceSupport.SetupUpdateAssetKafkaConsumer(assetServiceSupport.UpdateAssetModel.AssetUID, assetServiceSupport.UpdateAssetModel.ActionUTC);
      Thread.Sleep(new TimeSpan(0, 0, 10));
      assetServiceSupport.PostValidUpdateRequestToService();
    }

    [When(@"I Post Invalid AssetServiceUpdate Request With The Below Values")]
    public void WhenIPostInvalidAssetServiceUpdateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.InValidUpdateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.InValidUpdateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.InValidUpdateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.InValidUpdateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.InValidUpdateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.InValidUpdateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.InValidUpdateAssetModel.ActionUTC = DateTime.UtcNow.ToString();
      string contentType = "application/json";
      assetServiceSupport.PostInValidUpdateRequestToService(contentType,HttpStatusCode.BadRequest);
    }


    [When(@"I Set Invalid AssetServiceCreate AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid AssetServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidCreateAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Set Invalid AssetServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidUpdateAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Set Invalid AssetServiceDelete ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceDeleteActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidDeleteAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Post Invalid AssetServiceCreate Request")]
    public void WhenIPostInvalidAssetServiceCreateRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"AssetService Response With '(.*)' Should Be Returned")]
    public void ThenAssetServiceResponseWithShouldBeReturned(string errorMessage)
    {
      assetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid AssetServiceCreate SerialNumber To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateSerialNumberTo(string serialNumber)
    {
      assetServiceSupport.InValidCreateAssetModel.SerialNumber = InputGenerator.GetValue(serialNumber);
    }

    [When(@"I Set Invalid AssetServiceUpdate ModelYear To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateModelYearTo(string modelYear)
    {
      assetServiceSupport.InValidUpdateAssetModel.ModelYear = InputGenerator.GetValue(modelYear);
    }

    [When(@"I Set Invalid AssetServiceUpdate IconKey To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateIconKeyTo(string iconKey)
    {
      assetServiceSupport.InValidUpdateAssetModel.IconKey = InputGenerator.GetValue(iconKey);
    }

    [When(@"I Post AssetServiceCreate Request With Invalid ContentType '(.*)'")]
    public void WhenIPostAssetServiceCreateRequestWithInvalidContentType(string contentType)
    {
      assetServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.UnsupportedMediaType);
    }

    [When(@"I Post Invalid AssetServiceUpdate Request")]
    public void WhenIPostInvalidAssetServiceUpdateRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Post Invalid AssetServiceUpdate Request With Invalid ContentType '(.*)'")]
    public void WhenIPostInvalidAssetServiceUpdateRequestWithInvalidContentType(string contentType)
    {
      assetServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.UnsupportedMediaType);
    }

    [When(@"I Set Invalid AssetServiceDelete AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceDeleteAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidDeleteAssetModel.AssetUID = assetUid;
    }

    [When(@"I Post Invalid AssetServiceDelete Request")]
    public void WhenIPostInvalidDeleteAssetRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidDeleteRequestToService(assetServiceSupport.InValidDeleteAssetModel.AssetUID, assetServiceSupport.InValidDeleteAssetModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
    }

    public static CreateAssetEvent GetDefaultValidAssetServiceCreateRequest()
    {
      CreateAssetEvent defaultValidAssetServiceCreateModel = new CreateAssetEvent();
      defaultValidAssetServiceCreateModel.AssetUID = Guid.NewGuid();
      defaultValidAssetServiceCreateModel.AssetName = "AutoTestAPICreateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.SerialNumber = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.MakeCode = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.Model = "AutoTestAPICreateAssetModel" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.AssetType = "WHEEL LOADERS";
      defaultValidAssetServiceCreateModel.IconKey = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceCreateModel.EquipmentVIN = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.ModelYear = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceCreateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidAssetServiceCreateModel;
    }

    public static UpdateAssetEvent GetDefaultValidAssetServiceUpdateRequest()
    {
      UpdateAssetEvent defaultValidAssetServiceUpdateModel = new UpdateAssetEvent();
      defaultValidAssetServiceUpdateModel.AssetUID = Guid.NewGuid();
      defaultValidAssetServiceUpdateModel.AssetName = "AutoTestAPIUpdateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceUpdateModel.Model = "A60";
      defaultValidAssetServiceUpdateModel.AssetType = "GENSET";
      defaultValidAssetServiceUpdateModel.IconKey = assetServiceSupport.RandomNumber(); ;
      defaultValidAssetServiceUpdateModel.EquipmentVIN = "AutoTestAPIUpdateEquipmentVIN" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceUpdateModel.ModelYear = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceUpdateModel.ActionUTC = DateTime.UtcNow;

      return defaultValidAssetServiceUpdateModel;
    }

    public static DeleteAssetEvent GetDefaultValidAssetServiceDeleteRequest()
    {
      DeleteAssetEvent defaultValidAssetServiceDeleteModel = new DeleteAssetEvent();
      defaultValidAssetServiceDeleteModel.AssetUID = Guid.NewGuid();
      defaultValidAssetServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      return defaultValidAssetServiceDeleteModel;
    }

    public static InValidCreateAssetEvent GetDefaultInValidAssetServiceCreateRequest()
    {
      InValidCreateAssetEvent defaultInValidAssetServiceCreateModel = new InValidCreateAssetEvent();
      defaultInValidAssetServiceCreateModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceCreateModel.AssetName = "AutoTestAPICreateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.SerialNumber = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.MakeCode = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.Model = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.AssetType = "WHEEL LOADERS";
      defaultInValidAssetServiceCreateModel.IconKey = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceCreateModel.EquipmentVIN = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.ModelYear = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidAssetServiceCreateModel;
    }

    public static InValidUpdateAssetEvent GetDefaultInValidAssetServiceUpdateRequest()
    {
      InValidUpdateAssetEvent defaultInValidAssetServiceUpdateModel = new InValidUpdateAssetEvent();
      defaultInValidAssetServiceUpdateModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceUpdateModel.AssetName = "AutoTestAPIUpdateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceUpdateModel.Model = "A60";
      defaultInValidAssetServiceUpdateModel.AssetType = "GENSET";
      defaultInValidAssetServiceUpdateModel.IconKey = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceUpdateModel.EquipmentVIN = "AutoTestAPIUpdateEquipmentVIN" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceUpdateModel.ModelYear = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();

      return defaultInValidAssetServiceUpdateModel;
    }

    public static InValidDeleteAssetEvent GetDefaultInValidAssetServiceDeleteRequest()
    {
      InValidDeleteAssetEvent defaultInValidAssetServiceDeleteModel = new InValidDeleteAssetEvent();
      defaultInValidAssetServiceDeleteModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidAssetServiceDeleteModel;
    }


  }
}