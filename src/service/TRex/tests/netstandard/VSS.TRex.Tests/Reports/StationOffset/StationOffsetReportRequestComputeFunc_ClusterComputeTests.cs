using FluentAssertions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class StationOffsetReportRequestComputeFunc_ClusterComputeTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var func = new StationOffsetReportRequestComputeFunc_ClusterCompute();
      func.Should().NotBeNull();
    }

    [Fact]
    public void StationOffsetReportRequestComputeFunc_ClusterCompute_InvokeFailureWIthException()
    {
      var func = new StationOffsetReportRequestComputeFunc_ClusterCompute();

      var result = func.Invoke(null);
      result.Should().NotBeNull();
      result.ReturnCode.Should().Be(ReportReturnCode.UnknownError);
      result.ResultStatus.Should().Be(RequestErrorStatus.Unknown);
    }
  }
}
