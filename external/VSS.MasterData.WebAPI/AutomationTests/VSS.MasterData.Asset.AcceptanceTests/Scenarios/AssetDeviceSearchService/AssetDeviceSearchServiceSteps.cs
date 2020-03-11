using AutomationCore.Shared.Library;
using System;
using TechTalk.SpecFlow;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSearchService
{
    [Binding]
    public class AssetDeviceSearchServiceSteps
    {
        #region Variables
        public string TestName;

        private static Log4Net Log = new Log4Net(typeof(AssetDeviceSearchServiceSteps));
        public static AssetDeviceSearchServiceSupport assetDeviceSearchServiceSupport = new AssetDeviceSearchServiceSupport(Log);
        public static CreateDeviceModel defaultValidDeviceServiceCreateModel = new CreateDeviceModel();
        public static DeviceAssetAssociationModel defaultValidDeviceAssetAssociationModel = new DeviceAssetAssociationModel();

        #endregion

        [Given(@"AssetDeviceSearchService Is Ready To Verify '(.*)'")]
        public void GivenAssetDeviceSearchServiceIsReadyToVerify(string testDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [When(@"I Get the AssetDeviceSearchService For '(.*)'")]
        public void WhenIGetTheAssetDeviceSearchServiceFor(string searchString)
        {
            assetDeviceSearchServiceSupport.GetAssetDeviceList(searchString, null, null);
        }

        [When(@"I Get the AssetDeviceSearchService")]
        public void WhenIGetTheAssetDeviceSearchService()
        {
            assetDeviceSearchServiceSupport.GetAssetDeviceList(null, null, null);
        }

        [When(@"I Get the AssetDeviceSearchService With '(.*)', '(.*)' And '(.*)'")]
        public void WhenIGetTheAssetDeviceSearchServiceWith(string searchString, string pageNo, string pageSize)
        {
            if (searchString == "EMPTY") searchString = null;
            if (pageNo == "EMPTY") pageNo = null;
            if (pageSize == "EMPTY") pageSize = null;
            assetDeviceSearchServiceSupport.GetAssetDeviceList(searchString, pageNo, pageSize);
        }

        [When(@"I Get the AssetDeviceSearchService With '(.*)' SearchString And '(.*)' PageSize")]
        public void WhenIGetTheAssetDeviceSearchServiceWithSearchStringAndPageSize(string searchString, string pageSize)
        {
            if (searchString == "EMPTY") searchString = null;
            if (pageSize == "EMPTY") pageSize = null;
            assetDeviceSearchServiceSupport.GetAssetDeviceList(searchString, null, pageSize);
        }

        [Then(@"AssetDeviceSearchService Response With No AssetDevices Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseWithNoAssetDevicesShouldBeReturned()
        {
            assetDeviceSearchServiceSupport.VerifyResponseZeroAssetDeviceList();
        }

        [Then(@"AssetDeviceSearchService Response With Created '(.*)' Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseWithCreatedShouldBeReturned(string verifyParameter)
        {
            assetDeviceSearchServiceSupport.VerifyResponse(verifyParameter);
        }

        [Then(@"AssetDeviceSearchService Response Sorted By AssetSN Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseSortedbyAssetSNShouldBeReturned()
        {
            assetDeviceSearchServiceSupport.VerifyResponseSortedByAssetSN();
        }

        [Then(@"AssetDeviceSearchService Response With Valid '(.*)' Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseWithValidShouldBeReturned(string verifyParameter)
        {
            assetDeviceSearchServiceSupport.VerifyResponse(verifyParameter);
        }

        [Then(@"AssetDeviceSearchService Response With Wrong PageSize Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseWithWrongPageSizeShouldBeReturned()
        {
            assetDeviceSearchServiceSupport.VerifyResponse("InvalidPageSize");
        }

        [Then(@"AssetDeviceSearchService Response With ErrorMessage Should Be Returned")]
        public void ThenAssetDeviceSearchServiceResponseWithErrorMessageShouldBeReturned()
        {
            assetDeviceSearchServiceSupport.VerifyErrorResponse();
        }


        [Given(@"DeviceServiceCreate Request Is Setup With Default Values")]
        public void GivenDeviceServiceCreateRequestIsSetupWithDefaultValues()
        {
            assetDeviceSearchServiceSupport.CreateDeviceModel = GetDefaultValidDeviceServiceCreateRequest();
        }

        [When(@"I Post Valid DeviceServiceCreate Request")]
        public void WhenIPostValidDeviceServiceCreateRequest()
        {
            assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
        }

        [Given(@"AssetDeviceAssociation Request Is Setup With Default Values")]
        public void GivenAssetDeviceAssociationRequestIsSetupWithDefaultValues()
        {
            AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
            AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
            assetDeviceSearchServiceSupport.CreateDeviceModel = GetDefaultValidDeviceServiceCreateRequest();
            assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
            assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = GetDefaultValidDeviceAssetAssociationServiceRequest();   
        }

        [When(@"I Post Valid DeviceAssetAssociation Request")]
        public void WhenIPostValidDeviceAssetAssociationRequest()
        {
            assetDeviceSearchServiceSupport.PostValidDeviceAssetAssociationRequestToService();
        }

        public static CreateDeviceModel GetDefaultValidDeviceServiceCreateRequest()
        {
            defaultValidDeviceServiceCreateModel.DeviceUID = Guid.NewGuid();
            defaultValidDeviceServiceCreateModel.DeviceSerialNumber = "AutoTestAPICreateDeviceSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidDeviceServiceCreateModel.DeviceType = "PLE631";
            defaultValidDeviceServiceCreateModel.DeviceState = "Provisioned"; //"Subscribed";
            defaultValidDeviceServiceCreateModel.DeregisteredUTC = null;
            defaultValidDeviceServiceCreateModel.ModuleType = null;
            defaultValidDeviceServiceCreateModel.MainboardSoftwareVersion = null;
            defaultValidDeviceServiceCreateModel.RadioFirmwarePartNumber = null;
            defaultValidDeviceServiceCreateModel.GatewayFirmwarePartNumber = null;
            defaultValidDeviceServiceCreateModel.DataLinkType = null;
            defaultValidDeviceServiceCreateModel.ActionUTC = DateTime.UtcNow;
            defaultValidDeviceServiceCreateModel.ReceivedUTC = null;

            return defaultValidDeviceServiceCreateModel;
        }

        public static DeviceAssetAssociationModel GetDefaultValidDeviceAssetAssociationServiceRequest()
        {
            defaultValidDeviceAssetAssociationModel.DeviceUID = assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
            defaultValidDeviceAssetAssociationModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            defaultValidDeviceAssetAssociationModel.ActionUTC = DateTime.UtcNow;
            defaultValidDeviceAssetAssociationModel.ReceivedUTC = null;

            return defaultValidDeviceAssetAssociationModel;
        }

    }
}