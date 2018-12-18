using System.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetProjectExtents.feature")]
  public class GetProjectExtentsSteps : FeaturePostRequestBase<ProjectExtentRequest, ProjectExtentsResult>
  {
    [And(@"I decide to exclude surveyed surface (.*)")]
    public void GivenIDecideToExcludeAnySurveyedSurface(int surveyedSurfaceId)
    {
       PostRequestHandler.CurrentRequest.excludedSurveyedSurfaceIds = new long[] { surveyedSurfaceId };
    }

    [Then(@"the following objects should be returned:")]
    public void ThenTheFollowingObjectsShouldBeReturned(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new ProjectExtentsResult();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedResult.ProjectExtents = new BoundingBox3DGrid
        {
          maxX = double.Parse(row.Cells.ElementAt(0).Value),
          maxY = double.Parse(row.Cells.ElementAt(1).Value),
          maxZ = double.Parse(row.Cells.ElementAt(2).Value),
          minX = double.Parse(row.Cells.ElementAt(3).Value),
          minY = double.Parse(row.Cells.ElementAt(4).Value),
          minZ = double.Parse(row.Cells.ElementAt(5).Value)
        };
      }

      AssertObjectsAreEqual(expectedResult);
    }
  }
}
