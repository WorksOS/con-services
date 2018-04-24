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
    [Binding, Scope(Feature = "GetProjectMachines")]
    public class GetProjectMachinesSteps
    {
        private Getter<GetMachinesResult> machineDetailRequester;

        [Given(@"the Machine service URI ""(.*)""")]
        public void GivenTheMachineServiceURI(string uri)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            machineDetailRequester = new Getter<GetMachinesResult>(uri);
        }

        [Given(@"a project id (.*)")]
        public void GivenAProjectId(int projectId)
        {
            machineDetailRequester.Uri = String.Format(machineDetailRequester.Uri, projectId);
        }

        [Given(@"a machine id (.*)")]
        public void GivenAMachineId(Decimal machineId)
        {
            //resourceUri = resourceUri + "/" + mId;
            machineDetailRequester.Uri = machineDetailRequester.Uri + "/" + machineId;
        }

        [When(@"I try to get machine details")]
        public void WhenITryToGetMachineDetails()
        {
            machineDetailRequester.DoValidRequest();
        }

        [Then(@"the following machines should be returned")]
        public void ThenTheFollowingMachinesShouldBeReturned(Table machines)
        {
            GetMachinesResult expectedResult = new GetMachinesResult();

            // Get expected machine details from feature file
            List<MachineStatus> expectedMachineList = new List<MachineStatus>();
            foreach(var machine in machines.Rows)
            {
                expectedMachineList.Add(new MachineStatus() 
                {
                    lastKnownDesignName = machine["lastKnownDesignName"],
                    lastKnownLayerId = Convert.ToUInt16(machine["lastKnownLayerId"]),
                    lastKnownTimeStamp = Convert.ToDateTime(machine["lastKnownTimeStamp"]),
                    lastKnownLatitude = Convert.ToDouble(machine["lastKnownLatitude"]),
                    lastKnownLongitude = Convert.ToDouble(machine["lastKnownLongitude"]),
                    lastKnownX = Convert.ToDouble(machine["lastKnownX"]),
                    lastKnownY = Convert.ToDouble(machine["lastKnownY"]),
                    assetID = Convert.ToInt64(machine["assetID"]),
                    machineName = machine["machineName"],
                    isJohnDoe = Convert.ToBoolean(machine["isJohnDoe"])
                });
            }
            MachineStatus[] expectedMachineArray = expectedMachineList.ToArray();

            expectedResult.MachineStatuses = expectedMachineArray;

            Assert.AreEqual<GetMachinesResult>(expectedResult, machineDetailRequester.CurrentResponse);
        }

        [When(@"I try to get machine details expecting badrequest")]
        public void WhenITryToGetMachineDetailsExpectingBadrequest()
        {
            machineDetailRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
        }

        [Then(@"the response should have code (.*) and message ""(.*)""")]
        public void ThenTheResponseShouldHaveCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(machineDetailRequester.CurrentResponse.Code == code && 
                machineDetailRequester.CurrentResponse.Message == message);
        }
    }
}
