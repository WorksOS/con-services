using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;


namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.VolumePerCycle
{
    [Binding]
    public class VolumePerCycleSteps
    {
        private static Log4Net Log = new Log4Net(typeof(VolumePerCycleSteps));
        public List<string> ListAssetUIDs = new List<string>();
        public VolumePerCycleSupport volumePerCycleSupport = new VolumePerCycleSupport();

        [Given(@"VolumePerCycle is ready to verify '(.*)'")]
        public void GivenVolumePerCycleIsReadyToVerify(string description)
        {
            string testName = ScenarioContext.Current.ScenarioInfo.Title.ToString();
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario - " + testName);
        }
        
        [Given(@"I create Assets using create asset request")]
        public void GivenICreateAssetsUsingCreateAssetRequest()
        {
            ListAssetUIDs.Add(volumePerCycleSupport.CreateAsset().ToString());
            volumePerCycleSupport.AssociateAssetDevice(ListAssetUIDs);
            volumePerCycleSupport.AssociateAssetCustomer(ListAssetUIDs);
        }
        
        [Given(@"VolumePerCycle request is setup with default valid values")]
        public void GivenVolumePerCycleRequestIsSetupWithDefaultValidValues()
        {
            volumePerCycleSupport.SetVolumePerCycleAPIDefaultValues(ListAssetUIDs);
        }
        
        [When(@"I POST valid VolumePerCycle request")]
        public void WhenIPOSTValidVolumePerCycleRequest()
        {
            volumePerCycleSupport.PostVolumePerCycleRequest();
        }

        [Then(@"Posted volume value retrived should match with DB")]
        public void ThenPostedVolumeValueRetrivedShouldMatchWithDB()
        {
            volumePerCycleSupport.ValidateDB(ListAssetUIDs);
        }
    }
}
