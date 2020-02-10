using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSP.MasterData.Asset.Data.Helpers;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.Productivity
{
    [Binding]
    public class AddProductivitySteps
    {
        #region variables
        public string TestName;
        private static Log4Net Log = new Log4Net(typeof(AddProductivitySteps));
        private static AddProductivitySupport testSupport = new AddProductivitySupport(Log);

        #endregion


        [BeforeScenario("DeleteExistingRecords")]
        public static void DeleteAssetWeeklyConfiguration()
        {

            testSupport.UpdateDB();

        }

        [Given(@"'(.*)' is ready for verification")]
        public void GivenIsReadyForVerification(string testDescription)
        {
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"AddProductivity is setup with default valid values")]
        public void GivenAddProductivityIsSetupWithDefaultValidValues()
        {
            testSupport.SetDefaultValidValuesForProductivity();
        }

        [Given(@"I modify  in '(.*)' value to be  '(.*)'")]
        public void GivenIModifyInValueToBe(string AssetTargetName, string AssetTargetValue)
        {
            testSupport.ModifyDefaultProductivityTargetValues(AssetTargetName, AssetTargetValue);
        }
        [When(@"I Put Invalid Productivity details for asset")]
        public void WhenIPutInvalidProductivityDetailsForAsset()
        {
            testSupport.InvalidPutRequest();
        }


        [When(@"I Put Valid Productivity details for asset")]
        public void WhenIPutValidProductivityDetailsForAsset()
        {
            testSupport.ValidPutRequest();
        }
     

        [Then(@"Valid Error Code (\d+) should be shown")]
        public void ThenValidErrorCodeShouldBeShown(int errorCode)
        {
            testSupport.ErrorCodeValidation(errorCode);
        }


        [When(@"When I try to retrieve Productivity details")]

        public void WhenITryToRetrieveProductivityDetails()
        {
            testSupport.RetrieveProductivityDetails();
        }




        [Then(@"Valid details should be displayed")]
        public void ThenValidDetailsShouldBeDisplayed()
        {
            testSupport.VerifyProductivityDetails();
        }



    }
}
