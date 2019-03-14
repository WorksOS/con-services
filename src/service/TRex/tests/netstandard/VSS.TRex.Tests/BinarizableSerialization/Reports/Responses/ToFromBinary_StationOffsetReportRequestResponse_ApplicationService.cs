using System.Collections.Generic;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Responses
{
  public class ToFromBinary_StationOffsetReportRequestResponse_ApplicationService
  {
    [Fact]
    public void Test_StationOffsetReportRequestResponse_ApplicationService_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<StationOffsetReportRequestResponse_ApplicationService>("Empty StationOffsetReportRequestResponse_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestResponse_ApplicationService_Empty()
    {
      var response = new StationOffsetReportRequestResponse_ApplicationService() { ReturnCode = ReportReturnCode.NoData };

      SimpleBinarizableInstanceTester.TestClass(response, "Empty StationOffsetReportRequestResponse_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestResponse_ApplicationService_WithMinimalContent()
    {
      var response = new StationOffsetReportRequestResponse_ApplicationService()
      {
        ResultStatus = RequestErrorStatus.OK,
        ReturnCode = ReportReturnCode.NoData,
        ReportType = ReportType.StationOffset,
        StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>()
        {
          new StationOffsetReportDataRow_ApplicationService() {}
        }};

      SimpleBinarizableInstanceTester.TestClass(response, "Empty StationOffsetReportRequestResponse_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestResponse_ApplicationService_WithContent()
    {
      double station = 1;
      var offsets = new List<StationOffsetRow>()
        { new StationOffsetRow() {Northing = 1,Easting = 2,Elevation = 4,Cmv = 5,CutFill = 6,Mdp = 7,PassCount = 8,Temperature = 9}}
        ;

      var response = new StationOffsetReportRequestResponse_ApplicationService()
      {
        ResultStatus = RequestErrorStatus.OK,
        ReturnCode = ReportReturnCode.NoData,
        ReportType = ReportType.StationOffset,
        StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>()
        {
          new StationOffsetReportDataRow_ApplicationService(station, offsets) {}
        }
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Empty StationOffsetReportRequestResponse_ApplicationService not same after round trip serialisation");
    }
  }
}
