using System.Collections.Generic;
using VSS.Productivity3D.Models.Models.Reports;
using Xunit;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Responses
{
  public class ToFromBinary_StationOffsetReportRequestResponse_ClusterCompute : BaseTests
  {
    [Fact]
    public void Test_StationOffsetReportRequestResponse_ClusterCompute_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<StationOffsetReportRequestResponse_ClusterCompute>("Empty StationOffsetReportRequestResponse_ClusterCompute not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestResponse_ClusterCompute_WithContent()
    {
      var rows = new List<StationOffsetRow>
      {
        new StationOffsetRow()
        {
          Station = 200,
          Offset = -1,
          Northing = 1,
          Easting = 2,
          Elevation = 3,
          CutFill = 4,
          Cmv = 5,
          Mdp = 6,
          PassCount = 7,
          Temperature = 8
        },
        new StationOffsetRow()
        {
          Station = 200,
          Offset = 0,
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
      var rowList = new List<StationOffsetRow>();
      rowList.AddRange(rows);

      var response = new StationOffsetReportRequestResponse_ClusterCompute()
      {
        ReturnCode = ReportReturnCode.NoError,
        ResultStatus = RequestErrorStatus.OK,
        StationOffsetRows = rowList
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Empty StationOffsetReportRequestResponse_ClusterCompute not same after round trip serialisation");
    }
  }
}
