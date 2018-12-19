using System;
using System.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ProjectStatistics.feature")]
  public class ProjectStatisticsSteps : FeaturePostRequestBase<StatisticsParameters, ProjectStatistics>
  {
    [And(@"a Project Statistics project id (.*)")]
    public void GivenAProjectStatisticsProjectId(long pId)
    {
      PostRequestHandler.CurrentRequest = new StatisticsParameters { projectId = pId, excludedSurveyedSurfaceIds = new long[] { } };
    }

    [And(@"I decide to exclude surveyed surfaces ""(.*)""")]
    public void GivenIDecideToExcludeSurveyedSurfaces(string surveyedSurfaceIds)
    {
      var ids = Array.ConvertAll(surveyedSurfaceIds.Split(','), long.Parse);

      PostRequestHandler.CurrentRequest.excludedSurveyedSurfaceIds = ids;
    }

    [And(@"I decide to include surveyed surfaces ""(.*)""")]
    public void GivenIDecideToIncludeSurveyedSurfaces(string surveyedSurfaceIds)
    {
      var ids = Array.ConvertAll(surveyedSurfaceIds.Split(','), long.Parse);

      PostRequestHandler.CurrentRequest.excludedSurveyedSurfaceIds = ids;
    }

    [Then(@"I should get the following project statistics:")]
    public void ThenIShouldGetTheFollowingProjectStatistics(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new ProjectStatistics();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedResult.startTime = DateTime.Parse(row.Cells.ElementAt(0).Value);
        expectedResult.endTime = DateTime.Parse(row.Cells.ElementAt(1).Value);
        expectedResult.cellSize = double.Parse(row.Cells.ElementAt(2).Value);
        expectedResult.indexOriginOffset = int.Parse(row.Cells.ElementAt(3).Value);
        expectedResult.extents = new BoundingBox3DGrid
        {
          maxX = double.Parse(row.Cells.ElementAt(4).Value),
          maxY = double.Parse(row.Cells.ElementAt(5).Value),
          maxZ = double.Parse(row.Cells.ElementAt(6).Value),
          minX = double.Parse(row.Cells.ElementAt(7).Value),
          minY = double.Parse(row.Cells.ElementAt(8).Value),
          minZ = double.Parse(row.Cells.ElementAt(9).Value)
        };
      }

      ObjectComparer.RoundAllDoubleProperties(expectedResult.extents, roundingPrecision: 2);
      ObjectComparer.RoundAllDoubleProperties(PostRequestHandler.CurrentResponse.extents, roundingPrecision: 2);

      ObjectComparer.AssertAreEqual(actualResultObj: PostRequestHandler.CurrentResponse, expectedResultObj: expectedResult);
    }
  }
}
