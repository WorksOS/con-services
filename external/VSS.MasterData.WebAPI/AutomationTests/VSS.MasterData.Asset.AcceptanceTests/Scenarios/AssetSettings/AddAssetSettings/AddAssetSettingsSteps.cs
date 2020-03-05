using AutomationCore.Shared.Library;
using TechTalk.SpecFlow;


namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.AddAssetSettings
{
    [Binding]
    public class AddAssetSettingsSteps
    {
        #region variables
        public string TestName;
        private static Log4Net Log = new Log4Net(typeof( AddAssetSettingsSteps));
        private static AddAssetSettingsSupport addAssetSettingSupport = new AddAssetSettingsSupport(Log);

        #endregion

        [Given(@"AddAssetSettings is setup with default valid values")]
        public void GivenAddAssetSettingsIsSetupWithDefaultValidValues()
        {
            addAssetSettingSupport.SetDefaultValidValues();
        }

        [When(@"I Put Valid asset settings")]
        public void WhenIPutValidAssetSettings()
        {
            addAssetSettingSupport.CreateValidAssetSettings();
        }

        [When(@"When I try to retrieve asset settings")]
        public void WhenWhenITryToRetrieveAssetSettings()
        {
            addAssetSettingSupport.RetrieveAssetSettings();
        }

        [Then(@"I try to retrieve the asset targets")]
        public void ThenITryToRetrieveTheAssetTargets()
        {
            addAssetSettingSupport.RetrieveAssetSettings();
        }
        [Given(@"I Put Valid asset settings with startdate as '(.*)' and EndDate as '(.*)'")]
        public void GivenIPutValidAssetSettingsWithStartdateAsAndEndDateAs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Same details should be displayed")]
        public void ThenSameDetailsShouldBeDisplayed()
        {
            addAssetSettingSupport.VerifyAssetSettings();
        }

    }
}
