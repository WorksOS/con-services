using System;
using LandfillService.AcceptanceTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataProjectsSteps
    {
        private readonly StepSupport stepSupport = new StepSupport();

        [Given(@"I inject the following CreateProject master data events")]
        public void GivenIInjectTheFollowingProjectsMasterDataEvents(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                IMessage message = MessageFactory.Instance.CreateMessage(row, stepSupport.GetRandomNumber(),MessageType.CreateProjectEvent);
                message.Send();
                Assert.IsTrue(LandFillMySqlDb.WaitForProjectToBeCreated(row["ProjectName"] + stepSupport.GetRandomNumber()), "Failed to created a project in landfill mySql db");
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
