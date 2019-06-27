﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionTemperature.feature")]
  public class CompactionTemperatureSteps : FeatureGetRequestBase<JObject>
  { }
}
