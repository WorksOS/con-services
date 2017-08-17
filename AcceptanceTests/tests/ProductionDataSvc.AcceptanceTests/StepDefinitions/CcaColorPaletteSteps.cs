using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding]
    public class CcaColorPaletteSteps
    {
        Getter<CCAColorPaletteResult> paletteRequester;

        [When(@"I request CCA color palette for machine (.*) in project (.*)")]
        public void WhenIRequestCCAColorPaletteForMachineInProject(long machineId, long projectId)
        {
            var uri = $"{RaptorClientConfig.ProdSvcBaseUri}/api/v1/ccacolors?projectId={projectId}&assetId={machineId}";
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
