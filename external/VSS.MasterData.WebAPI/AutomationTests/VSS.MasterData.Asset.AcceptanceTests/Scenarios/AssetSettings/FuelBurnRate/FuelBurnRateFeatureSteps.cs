using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.FuelBurnRate
{
    [Binding]
    public class FuelBurnRateFeatureSteps
    {
        private static Log4Net Log = new Log4Net(typeof(FuelBurnRateFeatureSteps));
        public FuelBurnRateSupport fuelBurnRateSupport = new FuelBurnRateSupport();
        public List<string> ListAssetUIDs = new List<string>();

        [Given(@"FuelBurnRateAPI is ready to verify '(.*)'")]
        public void GivenFuelBurnRateAPIIsReadyToVerify(string description)
        {
            string testName = ScenarioContext.Current.ScenarioInfo.Title.ToString();
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario - " + testName);
        }
        
        [Given(@"FuelBurnRateAPI request is setup with default valid values")]
        public void GivenFuelBurnRateAPIRequestIsSetupWithDefaultValidValues()
        {
            fuelBurnRateSupport.SetFuelBurnAPIDefaultValues(ListAssetUIDs);
        }

        [Given(@"I create Asset using create asset requests")]
        public void GivenICreateAssetUsingCreateAssetRequests()
        {
            ListAssetUIDs = fuelBurnRateSupport.CreateAsset();
            fuelBurnRateSupport.AssociateAssetDevice(ListAssetUIDs);
            fuelBurnRateSupport.AssociateAssetCustomer(ListAssetUIDs);
        }

        [When(@"I Put valid FuelBurnRateAPI request")]
        public void WhenIPutValidFuelBurnRateAPIRequest()
        {
            fuelBurnRateSupport.PostMileageTargetRequest();
        }

        [Then(@"Same FuelBurnRate details should be displayed")]
        public void ThenSameFuelBurnRateDetailsShouldBeDisplayed()
        {
            fuelBurnRateSupport.VerifyAssetSettings();
        }

    }
}
