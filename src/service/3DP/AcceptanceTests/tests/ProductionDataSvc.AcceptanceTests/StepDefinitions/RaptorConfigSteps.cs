using System.Xml;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("RaptorConfig.feature")]
  public class RaptorConfigSteps : FeatureGetRequestBase
  {
    [And(@"the config should contain correct tags")]
    public void ThenTheConfigShouldContainCorrectTags()
    {
      var e = (string)GetResponseHandler.CurrentResponse["configuration"];

      var xConfig = new XmlDocument();
      xConfig.LoadXml(e);

      var hasConfigTag = xConfig.GetElementsByTagName("Config").Count > 0;
      var hasMonitoringTag = xConfig.GetElementsByTagName("Monitoring").Count > 0;
      var hasASNodesTag = xConfig.GetElementsByTagName("ASNodes").Count > 0;
      var hasPSNodesTag = xConfig.GetElementsByTagName("PSNodes").Count > 0;
      var hasIONodesTAg = xConfig.GetElementsByTagName("IONodes").Count > 0;
      var hasCoordServicesTag = xConfig.GetElementsByTagName("CoordServices").Count > 0;

      Assert.True(hasConfigTag && hasMonitoringTag &
                  hasASNodesTag && hasPSNodesTag &&
                  hasIONodesTAg && hasCoordServicesTag);
    }
  }
}
