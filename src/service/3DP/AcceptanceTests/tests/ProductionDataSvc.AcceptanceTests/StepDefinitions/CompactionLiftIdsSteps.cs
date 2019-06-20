﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionLiftIds.feature")]
  public class CompactionLiftIdsSteps : FeatureGetRequestBase<JObject>
  { }
}
