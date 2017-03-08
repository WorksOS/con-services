using System;
using TechTalk.SpecFlow;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "GetMachineDesigns")]
    public class GetMachineDesignsSteps
    {
        private Getter<GetMachineDesignResult> machineDesignRequester;

        [Given(@"the Machine Design service URI ""(.*)""")]
        public void GivenTheMachineDesignServiceURI(string uri)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            machineDesignRequester = new Getter<GetMachineDesignResult>(uri);
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
    }
}
