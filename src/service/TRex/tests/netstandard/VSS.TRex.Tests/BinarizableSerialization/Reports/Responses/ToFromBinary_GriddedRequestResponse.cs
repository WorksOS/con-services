using System.Collections.Generic;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.Gridded;
using Xunit;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Responses
{
  public class ToFromBinary_GriddedReportRequestResponse : BaseTests
  {
    [Fact(Skip = "Stopped working with Tester changes")]
    public void Test_GriddedReportRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<GriddedReportRequestResponse>("Empty GriddedReportResponse not same after round trip serialisation");
    }

    [Fact(Skip = "Stopped working with Tester changes")]
    public void Test_GriddedReportRequestResponse_WithContent()
    {
      var rows = new List<GriddedReportDataRow>
      {
        new GriddedReportDataRow()
        {
          Northing = 1,
          Easting = 2,
          Elevation = 3,
          CutFill = 4,
          Cmv = 5,
          Mdp = 6,
          PassCount = 7,
          Temperature = 8
        },
        new GriddedReportDataRow()
        {
          Northing = 10,
          Easting = 11,
          Elevation = 12,
          CutFill = 13,
          Cmv = 14,
          Mdp = 15,
          PassCount = 16,
          Temperature = 17
        }
      };
      var rowList = new List<GriddedReportDataRow>();
      rowList.AddRange(rows);

      var response = new GriddedReportRequestResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        ReturnCode = ReportReturnCode.NoError,
        GriddedReportDataRowList = rowList
      };

      SimpleBinarizableInstanceTester.TestClass<GriddedReportRequestResponse>("Empty GriddedReportResponse not same after round trip serialisation");
    }
  }
}
