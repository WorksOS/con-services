using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.MileageTargetAPI;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.MileageTargetAPI
{
    [Binding]
    public class MileageTargetAPISteps
    {
        private static Log4Net Log = new Log4Net(typeof(MileageTargetAPISteps));
        public MileageTargetAPISupport mileageTargetAPISupport = new MileageTargetAPISupport();
        public List<string> ListAssetUIDs = new List<string>();

        [Given(@"TargetAPI is ready to verify '(.*)'")]
        public void GivenTargetAPIIsReadyToVerify(string description)
        {
            string testName = ScenarioContext.Current.ScenarioInfo.Title.ToString();
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario - " + testName);
        }

        [Given(@"I create Asset(.*) using create asset request")]
        public void GivenICreateAssetUsingCreateAssetRequest(int numberOfAssets)
        {
            ListAssetUIDs.Add(mileageTargetAPISupport.CreateAsset(numberOfAssets).ToString());
            mileageTargetAPISupport.AssociateAssetDevice(ListAssetUIDs);
            mileageTargetAPISupport.AssociateAssetCustomer(ListAssetUIDs);
        }

        [Given(@"I create Asset using create asset request")]
        public void GivenICreateAssetUsingCreateAssetRequest()
        {
            ListAssetUIDs.Add(mileageTargetAPISupport.CreateAsset().ToString());
            mileageTargetAPISupport.AssociateAssetDevice(ListAssetUIDs);
            mileageTargetAPISupport.AssociateAssetCustomer(ListAssetUIDs);

        }
        [When(@"I Set '(.*)' to mileage Request")]
        public void WhenISetToMileageRequest(List<string>ListAssetUIDs)
        {
            mileageTargetAPISupport.SetAssetUIDs(ListAssetUIDs);
        }

        [Given(@"TargetAPI request is setup with default valid values")]
        public void GivenTargetAPIRequestIsSetupWithDefaultValidValues()
        {
            mileageTargetAPISupport.SetMileageTargetAPIDefaultValues(ListAssetUIDs);
        }

        [When(@"I POST valid TargetAPI request")]
        public void WhenIPOSTValidTargetAPIRequest()
        {
            mileageTargetAPISupport.PostMileageTargetRequest();
        }

        [Then(@"estimated mileage value retrived should match with DB")]
        public void ThenEstimatedMileageValueRetrivedShouldMatchWithDB()
        {
            mileageTargetAPISupport.ValidateDB(ListAssetUIDs);
        }
    }
}


