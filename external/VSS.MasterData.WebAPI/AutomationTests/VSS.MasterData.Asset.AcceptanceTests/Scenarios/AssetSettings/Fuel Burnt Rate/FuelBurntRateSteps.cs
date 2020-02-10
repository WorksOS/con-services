using AutomationCore.Shared.Library;
using TechTalk.SpecFlow;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.Fuel_Burnt_Rate
{
    [Binding]
   public class FuelBurntRateSteps
    {
        private static Log4Net Log = new Log4Net(typeof(FuelBurntRateSteps));
        FuelBurntRateSupport fuelBurntSupport = new FuelBurntRateSupport(Log);

        [Given(@"FuelBurntRate is ready to verify '(.*)'")]
        public void GivenFuelBurntRateIsReadyToVerify(string testDescription)
        {
            string testName = string.Empty;
            testName = (ScenarioContext.Current.ScenarioInfo).Title.ToString() + "_" + testDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario " + testName);
        }

        [Given(@"I fetch all the assets for a customer")]
        public void GivenIFetchAllTheAssetsForACustomer()
        {
            fuelBurntSupport.FetchAssets();
        }

        [Given(@"fuel burnt rate is set with working burnt rate as '(.*)' and Idle Burnt Rate as'(.*)'")]
        public void GivenFuelBurntRateIsSetWithWorkingBurntRateAsAndIdleBurntRateAs(string workBurntRate, string idleBurntRate)
        {
            fuelBurntSupport.SetDefaultValidValues( workBurntRate,idleBurntRate);
        }

        [When(@"I POST valid FuelBurntRate request")]
        public void WhenIPOSTValidFuelBurntRateRequest()
        {
            fuelBurntSupport.PostFuelBurntRate(true);
        }
        [When(@"I Get the Fuel burnt rate")]
        public void WhenIGetTheFuelBurntRate()
        {
            fuelBurntSupport.GetFuelBurntRate();
        }

        [Then(@"both the values should match")]
        public void ThenBothTheValuesShouldMatch()
        {
            fuelBurntSupport.VerifyValidValues();
        }

        [When(@"I POST invalid FuelBurntRate request")]
        public void WhenIPOSTInvalidFuelBurntRateRequest()
        {
            fuelBurntSupport.PostFuelBurntRate(false);
        }

        [Then(@"Valid Error Code should be received")]
        public void ThenValidErrorCodeShouldBeReceived()
        {
            fuelBurntSupport.VerifyInvalidValues();
        }


    }
}
