using System;
using System.Collections.Generic;
using System.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetLifts.feature")]
  public class GetLiftsSteps : FeatureGetRequestBase
  {
    [Then(@"the following lift details should be returned")]
    public void ThenTheFollowingLiftDetailsShouldBeReturned(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new LayerIdsExecutionResult();
      var expectedLayers = new List<LayerIdDetails>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedLayers.Add(new LayerIdDetails
        {
          AssetId = long.Parse(row.Cells.ElementAt(0).Value),
          DesignId = long.Parse(row.Cells.ElementAt(1).Value),
          LayerId = long.Parse(row.Cells.ElementAt(2).Value),
          StartDate = DateTime.Parse(row.Cells.ElementAt(3).Value),
          EndDate = DateTime.Parse(row.Cells.ElementAt(4).Value)
        });
      }

      expectedResult.LayerIdDetailsArray = expectedLayers.ToArray();

      AssertObjectsAreEqual<LayerIdsExecutionResult>(expectedResult);
    }
  }
}
