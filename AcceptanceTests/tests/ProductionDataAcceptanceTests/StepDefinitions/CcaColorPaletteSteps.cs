using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class CcaColorPaletteSteps
    {
        Getter<CCAColorPaletteResult> paletteRequester;

        [When(@"I request CCA color palette for machine (.*) in project (.*)")]
        public void WhenIRequestCCAColorPaletteForMachineInProject(long machineId, long projectId)
        {
            string uri = string.Format("{0}/api/v1/ccacolors?projectId={1}&assetId={2}", RaptorClientConfig.ProdSvcBaseUri, projectId, machineId);
            paletteRequester = new Getter<CCAColorPaletteResult>(uri);
            paletteRequester.DoValidRequest();
        }
                
        [Then(@"the following color is returned")]
        public void ThenTheFollowingColorIsReturned(string paletteJson)
        {
            CCAColorPaletteResult expected = JsonConvert.DeserializeObject<CCAColorPaletteResult>(paletteJson);
            Assert.AreEqual(expected, paletteRequester.CurrentResponse);
        }
    }
}
