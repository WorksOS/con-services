using System;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Responses
{
  public class ToFromBinary_GriddedRequestResponse : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_GriddedReportResult_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<GriddedReportRequestResponse>("Empty GriddedReportResponse not same after round trip serialisation");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_GriddedReportResponse()
    {
      throw new NotImplementedException();
      // todojeannie
      //var response = new GriddedReportResult(ReportType.Gridded);
      //var griddedDataRow = new GriddedDataRow()
      //{
      //  Northing=1.0,
      //  Easting = 2.0,
      //  Elevation = 3.0,
      //  CutFill = 4.0,
      //  Cmv = 5,
      //  Mdp = 6,
      //  PassCount = 7,
      //  Temperature = 8
      //};
      //response.GriddedData.Rows.Add(griddedDataRow);

      //SimpleBinarizableInstanceTester.TestClass(response, "Custom GriddedReportResult not same after round trip serialisation");
    }
  }
}
