using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.VolumePerCycle.RetrieveVolumePerCycle
{
    [Binding]
    public class SaveVolumePerCycleSteps
    {
        private static Log4Net Log = new Log4Net(typeof(SaveVolumePerCycleSteps));
        public List<string> ListAssetUIDs = new List<string>();
        public RetrieveVolumePerCycleSupport retrieveVolumePerCycleSupport = new RetrieveVolumePerCycleSupport();

        [Given(@"SaveVolumePerCycle is ready to verify '(.*)'")]
        public void GivenSaveVolumePerCycleIsReadyToVerify(string description)
        {
            string testName = ScenarioContext.Current.ScenarioInfo.Title.ToString();
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario - " + testName);
        }
        
        [Given(@"I create Assets using create asset  request")]
        public void GivenICreateAssetsUsingCreateAssetRequest()
        {
            ListAssetUIDs.Add(retrieveVolumePerCycleSupport.CreateAsset().ToString());
            retrieveVolumePerCycleSupport.AssociateAssetDevice(ListAssetUIDs);
            retrieveVolumePerCycleSupport.AssociateAssetCustomer(ListAssetUIDs);
        }
        
        [Given(@"SaveVolumePerCycle request is setup with default valid values")]
        public void GivenSaveVolumePerCycleRequestIsSetupWithDefaultValidValues()
        {
            retrieveVolumePerCycleSupport.SetVolumePerCycleAPIDefaultValues(ListAssetUIDs);
        }

        [When(@"I Set '(.*)' to volumepercycle Request")]
        public void WhenISetToVolumepercycleRequest(List<String> ListAssetUIDs)
        {
            //retrieveVolumePerCycleSupport.CreateAsset(ListAssetUIDs);
        }

        [When(@"I PUT valid VolumePerCycle request")]
        public void WhenIPUTValidVolumePerCycleRequest()
        {
            retrieveVolumePerCycleSupport.PostVolumePerCycleRequest();
        }

        [Then(@"saved volume value retrived should match with DB `")]
        public void ThenSavedVolumeValueRetrivedShouldMatchWithDB()
        {
            retrieveVolumePerCycleSupport.ValidateDB(ListAssetUIDs);
        }

    }
}
