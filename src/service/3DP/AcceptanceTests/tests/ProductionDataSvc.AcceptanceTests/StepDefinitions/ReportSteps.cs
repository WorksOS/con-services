using System.Linq;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("Report.feature")]
  public class ReportSteps : FeatureGetRequestBase
  {
    [Then(@"the complex response object should match ""(.*)"" from the repository")]
    public void ValidateResponse(string resultName)
    {
      var actualResult = JsonConvert.DeserializeObject<GridReport>(GetResponseHandler.CurrentResponse["reportData"].ToString());
      var expectedResult = JsonConvert.DeserializeObject<GridReport>(GetResponseHandler.ResponseRepo[resultName]["reportData"].ToString());

      // Sort the rows 
      var actualrows = actualResult.Rows.OrderBy(x => x.Northing).ThenBy(x => x.Easting).ToList();
      var expectedrows = expectedResult.Rows.OrderBy(x => x.Northing).ThenBy(x => x.Easting).ToList();
      var rowCount = actualrows.Count;
      
      Assert.True(rowCount == expectedrows.Count, "Row count not the same as expected");

      var actualrowList = actualrows.ToList();
      var expectedrowList = expectedrows.ToList();

      for (var rowIdx = 0; rowIdx < rowCount; rowIdx++)
      {
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].Easting, actualrowList[rowIdx].Easting, "Easting", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].Northing, actualrowList[rowIdx].Northing, "Northing", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].Elevation, actualrowList[rowIdx].Elevation, "Elevation", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].CMV, actualrowList[rowIdx].CMV, "CMV", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].CutFill, actualrowList[rowIdx].CutFill, "CutFill", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].MDP, actualrowList[rowIdx].MDP, "MDP", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].PassCount, actualrowList[rowIdx].PassCount, "PassCount", rowIdx);
        ObjectComparer.CompareDouble(expectedrowList[rowIdx].Temperature, actualrowList[rowIdx].Temperature, "Temperature", rowIdx);
      }
    }
  }
}
