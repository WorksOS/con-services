using System.Linq;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Helpers;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetProjectExtents.feature")]
  public class GetProjectExtentsSteps : FeaturePostRequestBase<ProjectExtentRequest, ProjectExtentsResult>
  {
    [And(@"require surveyed surface larger than production data")]
    public void RequireSurveyedSurfaceLargerThanProductionData()
    {
      BeforeAndAfter.CreateSurveyedSurfaceLargerThanProductionData();
    }
    
    [And(@"I decide to exclude any surveyed surface")]
    public void GivenIDecideToExcludeAnySurveyedSurface()
    {
       PostRequestHandler.CurrentRequest.excludedSurveyedSurfaceIds = new long[] { 111 };
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
    //  var actualResult = JsonConvert.DeserializeObject<GetMachinesResult>(PostRequestHandler.CurrentResponse.ToString());
    ////  expectedResult.ProjectExtents = expectedMachineList.ToArray();

    //  ObjectComparer.RoundAllArrayElementsProperties(actualResult.MachineStatuses, roundingPrecision: 12);

    //  ObjectComparer.AssertAreEqual(
    //    actualResultObj: actualResult,
    //    expectedResultJson: JsonConvert.SerializeObject(expectedResult),
    //    ignoreCase: true);
    }
  }
}
