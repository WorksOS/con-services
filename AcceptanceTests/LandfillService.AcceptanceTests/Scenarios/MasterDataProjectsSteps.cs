using LandfillService.AcceptanceTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataProjectsSteps
    {
        private readonly StepSupport stepSupport = new StepSupport();

        [Given(@"I inject the following master data events")]
        public void GivenIInjectTheFollowingMasterDataEvents(Table table)
        {
            var randomNumber = stepSupport.GetRandomNumber();
            foreach (TableRow row in table.Rows)
            {
                var message = MessageFactory.Instance.CreateMessage(row, randomNumber, MessageType.CreateProjectEvent);
                message.Send();
                Assert.IsTrue(LandFillMySqlDb.WaitForProjectToBeCreated(row["ProjectName"] + randomNumber), "Failed to created a project in landfill mySql db");
            }

        }

        [When(@"I request the project details from landfill web api")]
        public void WhenIRequestTheProjectDetailsFromLandfillWebApi()
        {
        //            [Given(@"Get a list of all projects")]
        //public void GivenGetAListOfAllProjects()
        //{
        //    var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects"), Method = HttpMethod.Get };
        //    request.Headers.Add("SessionID", sessionId);
        //    response = httpClient.SendAsync(request).Result;
        //   // var projects = await response.Content.ReadAsAsync<Project[]>();
        //}
        }

        [Then(@"I the project details result from the Web Api should be")]
        public void ThenITheProjectDetailsResultFromTheWebApiShouldBe(Table table)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
