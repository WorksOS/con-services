using System.Collections.Generic;
using System.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CoordinateConversion.feature")]
  public class CoordinateConversionSteps : FeaturePostRequestBase<CoordinateConversionRequest, CoordinateConversionResult>
  {
    [And(@"the coordinate conversion type ""(.*)""")]
    public void GivenTheCoordinateConversionType(string conversionType)
    {
      if (conversionType == "LatLonToNorthEast")
        PostRequestHandler.CurrentRequest.conversionType = TwoDCoordinateConversionType.LatLonToNorthEast;
      if (conversionType == "NorthEastToLatLon")
        PostRequestHandler.CurrentRequest.conversionType = TwoDCoordinateConversionType.NorthEastToLatLon;
    }

    [And(@"these coordinates")]
    public void GivenTheseCoordinates(Gherkin.Ast.DataTable dataTable)
    {
      var coordinates = new List<TwoDConversionCoordinate>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        coordinates.Add(new TwoDConversionCoordinate
        {
          x = double.Parse(row.Cells.ElementAt(0).Value),
          y = double.Parse(row.Cells.ElementAt(1).Value)
        });
      }

      PostRequestHandler.CurrentRequest.conversionCoordinates = coordinates.ToArray();
    }

    [Then(@"the result should be")]
    public void ThenTheResultShouldBeThese(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new CoordinateConversionResult();
      var expectedCoordinates = new List<TwoDConversionCoordinate>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedCoordinates.Add(new TwoDConversionCoordinate
        {
          x = double.Parse(row.Cells.ElementAt(0).Value),
          y = double.Parse(row.Cells.ElementAt(1).Value)
        });
      }

      expectedResult.conversionCoordinates = expectedCoordinates.ToArray();

      Assert.Equal(expectedResult, PostRequestHandler.CurrentResponse);
    }
  }
}
