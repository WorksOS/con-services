using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ExportReport.feature")]
  public class ExportReportSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
