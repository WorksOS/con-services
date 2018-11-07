using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("GetProjectMachines.feature")]
  public class GetProjectMachinesSteps : FeatureGetRequestBase
  {
    [Then(@"the following machines should be returned:")]
    public void ThenTheFollowingMachinesShouldBeReturned(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new GetMachinesResult();
      var expectedMachineList = new List<MachineStatus>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedMachineList.Add(new MachineStatus
        {
          lastKnownDesignName = row.Cells.ElementAt(0).Value,
          lastKnownLayerId = ushort.Parse(row.Cells.ElementAt(1).Value),
          lastKnownTimeStamp = DateTime.Parse(row.Cells.ElementAt(2).Value),
          lastKnownLatitude = Math.Round(double.Parse(row.Cells.ElementAt(3).Value), 12),
          lastKnownLongitude = Math.Round(double.Parse(row.Cells.ElementAt(4).Value), 12),
          lastKnownX = Math.Round(double.Parse(row.Cells.ElementAt(5).Value), 12),
          lastKnownY = Math.Round(double.Parse(row.Cells.ElementAt(6).Value), 12),
          AssetId = long.Parse(row.Cells.ElementAt(7).Value),
          MachineName = row.Cells.ElementAt(8).Value,
          IsJohnDoe = bool.Parse(row.Cells.ElementAt(9).Value)
        });
      }

      var actualResult = JsonConvert.DeserializeObject<GetMachinesResult>(GetResponseHandler.CurrentResponse.ToString());
      expectedResult.MachineStatuses = expectedMachineList.ToArray();

      ObjectComparer.RoundAllArrayElementsProperties(actualResult.MachineStatuses, roundingPrecision: 8);
      ObjectComparer.RoundAllArrayElementsProperties(expectedResult.MachineStatuses, roundingPrecision: 8);

      ObjectComparer.AssertAreEqual(actualResultObj: actualResult, expectedResultObj: expectedResult);
    }
  }
}
