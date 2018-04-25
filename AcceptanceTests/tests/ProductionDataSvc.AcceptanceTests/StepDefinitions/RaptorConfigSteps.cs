using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "RaptorConfig")]
    public class RaptorConfigSteps
    {
        private Getter<ConfigResult> configGetter;

        [Given(@"the Raptor Config service URI ""(.*)""")]
        public void GivenTheRaptorConfigServiceURI(string configUri)
        {
            configUri = RaptorClientConfig.ReportSvcBaseUri + configUri;
            configGetter = new Getter<ConfigResult>(configUri);
        }

        [When(@"I try to get config for Raptor")]
        public void WhenITryToGetConfigForRaptor()
        {
            configGetter.DoValidRequest();
        }

        [Then(@"the response should contain code (.*) and message ""(.*)""")]
        public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(configGetter.CurrentResponse.Code == code && configGetter.CurrentResponse.Message == message,
                string.Format("Expected Code {0} and Message {1}, but received {2} and {3} instead.",
                code, message, configGetter.CurrentResponse.Code, configGetter.CurrentResponse.Message));
        }

        [Then(@"the config should contain correct tags")]
        public void ThenTheConfigShouldContainCorrectTags()
        {
            XmlDocument xConfig = JsonConvert.DeserializeXmlNode(configGetter.CurrentResponse.Configuration);

            bool hasConfigTag = xConfig.GetElementsByTagName("Config").Count > 0;
            bool hasMonitoringTag = xConfig.GetElementsByTagName("Monitoring").Count > 0;
            bool hasASNodesTag = xConfig.GetElementsByTagName("ASNodes").Count > 0;
            bool hasPSNodesTag = xConfig.GetElementsByTagName("PSNodes").Count > 0;
            bool hasIONodesTAg = xConfig.GetElementsByTagName("IONodes").Count > 0;
            bool hasCoordServicesTag = xConfig.GetElementsByTagName("CoordServices").Count > 0;

            Assert.IsTrue(hasConfigTag && hasMonitoringTag &&
                hasASNodesTag && hasPSNodesTag &&
                hasIONodesTAg && hasCoordServicesTag);
        }
    }
}
