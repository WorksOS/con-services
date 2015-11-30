using System;
using LandfillService.AcceptanceTests.Helpers;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataProjectsSteps
    {
        private readonly StepSupport stepSupport = new StepSupport();
        
        [Given(@"I inject the following projects master data events")]
        public void GivenIInjectTheFollowingProjectsMasterDataEvents(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                IMessage message = MessageFactory.Instance.CreateMessage(row, stepSupport.GetRandomNumber(),MessageType.CreateProjectEvent);
                message.Send();

                //if (message.GetMessageType() == MessageType.CreateAssetEvent) 
                //    Helpers.WaitForMySqlToBeUpdatedUsingCounter(testSupporter.AssetUid, "Asset", 1);
            }
        }

        [When(@"I request the project details from landfill web api")]
        public void WhenIRequestTheProjectDetailsFromLandfillWebApi()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I the project details result from the Web Api should be")]
        public void ThenITheProjectDetailsResultFromTheWebApiShouldBe(Table table)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
