using System;
using FluentAssertions;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellDatumRequestComputeFuncTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_CellDatumRequestComputeFunc_ApplicationService_Creation()
    {
      var func = new CellDatumRequestComputeFunc_ApplicationService();

      Assert.NotNull(func);
    }

    [Fact]
    public void Test_CellDatumRequestComputeFunc_ClusterCompute_Creation()
    {
      var func = new CellDatumRequestComputeFunc_ClusterCompute();

      Assert.NotNull(func);
    }

    [Fact]
    public void Test_CellDatumRequestComputeFunc_ClusterCompute_Creation_FailInvokeWithNullArgument()
    {
      var func = new CellDatumRequestComputeFunc_ClusterCompute
      {
        Argument = null
      };

      Action act = () => func.Invoke();
      act.Should().Throw<ArgumentException>().WithMessage("Argument for ComputeFunc must be provided");
    }
  }
}
