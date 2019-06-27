﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionTile.feature")]
  public class CompactionTileSteps : FeatureGetRequestBase<JObject>
  { }
}
