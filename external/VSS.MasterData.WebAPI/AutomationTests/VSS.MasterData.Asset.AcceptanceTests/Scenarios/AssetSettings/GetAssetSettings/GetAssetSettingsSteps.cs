using AutomationCore.Shared.Library;
using TechTalk.SpecFlow;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.GetAssetSettings
{
    [Binding]
    public class GetAssetSettingsSteps
    {
        #region variables
        public string TestName;
        private static Log4Net Log = new Log4Net(typeof(GetAssetSettingsSteps));
        private static GetAssetSettingsSupport testAssetSettingSupport = new GetAssetSettingsSupport(Log);

        #endregion

        
        [Given(@"'(.*)' is ready to verify")]
        public void GivenIsReadyToVerify(string testDescription)
        {
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"GetAssetSettings is setup with default valid values")]
        public void GivenGetAssetSettingsIsSetupWithDefaultValidValues()
        {
            testAssetSettingSupport.SetDefaultValidValues();
        }
        [Given(@"I add Asset")]
        public void GivenIAddAsset()
        {
            testAssetSettingSupport.CreateAssets();
            testAssetSettingSupport.AssociateAssetDevice();
            testAssetSettingSupport.AssociateAssetCustomer();
        }

        [When(@"I try to get asset Details")]
        public void WhenITryToGetAssetDetails()
        {
            testAssetSettingSupport.GetAssetDetails();
        }
        [Then(@"Valid Asset Details response should be returned")]
        public void ThenValidAssetDetailsResponseShouldBeReturned()
        {
            testAssetSettingSupport.VerifyValidResponse();
        }
        [Given(@"I set FilterName as '(.*)' and  FilterValue as '(.*)'")]
        public void GivenISetFilterNameAsAndFilterValueAs(string filterName, string filterValue)
        {
            testAssetSettingSupport.SetFilterNameFilterValue(filterName,filterValue);
        }

        [Then(@"I delete Asset")]
        public void ThenIDeleteAsset()
        {
            testAssetSettingSupport.DeleteAsset();
           
        }

        [Then(@"No Asset Details response should be returned")]
        public void ThenNoAssetDetailsResponseShouldBeReturned()
        {
            testAssetSettingSupport.VerifyInvalidResponse();
        }

        [Given(@"I set sortcolumn to '(.*)' and SortingType as '(.*)'")]
        public void GivenISetSortcolumnToAndSortingTypeAs(string sortColumn, string sortType)
        {
            testAssetSettingSupport.SetSortColumnAndType(sortColumn,sortType);
        }

        [Then(@"Valid Asset Details response should be returned based on sortcolumn '(.*)'")]
        public void ThenValidAssetDetailsResponseShouldBeReturnedBasedOnSortcolumn(string sortcolumn)
        {
            testAssetSettingSupport.VerifyBasedOnSortColumn(sortcolumn);
        }

    }
}
