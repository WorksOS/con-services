using System;
using TechTalk.SpecFlow;
using System.Net;
using System.Collections.Generic;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "GetMachineDesigns")]
    public class GetMachineDesignsSteps
    {
        private Getter<GetMachineDesignResult> machineDesignRequester;

      [Given(@"the Get Machine Design service URI ""(.*)"" and the result file ""(.*)""")]
      public void GivenTheGetMachineDesignServiceURIAndTheResultFile(string uri, string resultFileName)
      {
        uri = RaptorClientConfig.ProdSvcBaseUri + uri;
        machineDesignRequester = new Getter<GetMachineDesignResult>(uri, resultFileName);
      }

       [Given(@"a project Id (.*)")]
        public void GivenAProjectId(int projectId)
        {
            machineDesignRequester.Uri = String.Format(machineDesignRequester.Uri, projectId);
        }

        [When(@"I request machine designs")]
        public void WhenIRequestMachineDesigns()
        {
            machineDesignRequester.DoValidRequest();
        }

        [When(@"I request machine designs expecting Bad Request")]
        public void WhenIRequestMachineDesignsExpectingBadRequest()
        {
            machineDesignRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
        }

        [Then(@"the following machine designs should be returned")]
        public void ThenTheFollowingMachineDesignsShouldBeReturned(Table designs)
        {
            GetMachineDesignResult expectedResult = new GetMachineDesignResult();

            // Get expected machine designs from feature file
            List<DesignNames> expectedDesigns = new List<DesignNames>();
            foreach (var design in designs.Rows)
            {
                expectedDesigns.Add(new DesignNames()
                { 
                    designId = Convert.ToInt64(design["designId"]),
                    designName = design["designName"]
                });
            }

            expectedResult.designs = expectedDesigns;

            Assert.AreEqual(expectedResult, machineDesignRequester.CurrentResponse);
        }

        [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(machineDesignRequester.CurrentResponse.Code == code && 
                machineDesignRequester.CurrentResponse.Message == message);
        }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      machineDesignRequester.Uri = String.Format(machineDesignRequester.Uri, projectUid);
    }

    [Given(@"startUTC ""(.*)""")]
    public void GivenStartUTC(string startUTC)
    {
      machineDesignRequester.QueryString.Add("startUtc", startUTC);
    }

    [Given(@"endUTC ""(.*)""")]
    public void GivenEndUTC(string endUTC)
    {
      machineDesignRequester.QueryString.Add("endUtc", endUTC);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(machineDesignRequester.ResponseRepo[resultName], machineDesignRequester.CurrentResponse);
    }

  }
}
