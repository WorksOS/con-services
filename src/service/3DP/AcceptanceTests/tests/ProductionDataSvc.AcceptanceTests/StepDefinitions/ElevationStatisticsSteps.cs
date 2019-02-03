using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ElevationStatistics.feature")]
  public class ElevationStatisticsSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
