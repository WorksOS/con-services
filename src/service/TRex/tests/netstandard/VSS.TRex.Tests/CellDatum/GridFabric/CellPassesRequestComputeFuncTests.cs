using System;
using FluentAssertions;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellPassesRequestComputeFuncTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_CellPassesRequestComputeFunc_ApplicationService_Creation()
    {
      var func = new CellPassesRequestComputeFunc_ApplicationService();

      func.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellPassesRequestComputeFunc_ClusterCompute_Creation()
    {
      var func = new CellPassesRequestComputeFunc_ClusterCompute();

      func.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellPassesRequestComputeFunc_ClusterCompute_Creation_FailInvokeWithNullArgument()
    {
      var func = new CellPassesRequestComputeFunc_ClusterCompute
      { 
        Argument = null
      };

      Action act = () => func.Invoke();
      act.Should().Throw<ArgumentException>().WithMessage("Argument for ComputeFunc must be provided");
    }
  }
}
