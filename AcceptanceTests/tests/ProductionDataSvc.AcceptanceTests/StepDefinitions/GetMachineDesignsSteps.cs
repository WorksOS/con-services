using System.Collections.Generic;
using System.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetMachineDesigns.feature")]
  public class GetMachineDesignsSteps : FeatureGetRequestBase
  {
    [Then(@"the following machine designs should be returned:")]
    public void ThenTheFollowingMachineDesignsShouldBeReturned(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new GetMachineDesignResult();
      var expectedDesigns = new List<DesignName>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedDesigns.Add(new DesignName
        {
          designId = long.Parse(row.Cells.ElementAt(0).Value),
          designName = row.Cells.ElementAt(1).Value
        });
      }

      expectedResult.designs = expectedDesigns;

      AssertObjectsAreEqual(expectedResult);
    }
  }
}
