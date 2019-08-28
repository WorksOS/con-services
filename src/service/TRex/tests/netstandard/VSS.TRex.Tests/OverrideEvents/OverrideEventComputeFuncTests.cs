using System;
using FluentAssertions;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.OverrideEvents
{
  public class OverrideEventComputeFuncTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_OverrideEventComputeFunc_Creation()
    {
      var func = new OverrideEventComputeFunc();

      Assert.NotNull(func);
    }

    [Fact]
    public void Test_OverrideEventComputeFunc_Creation_FailInvokeWithNullArgument()
    {
      var func = new OverrideEventComputeFunc();
      Action act = () => func.Invoke(null);
      act.Should().Throw<ArgumentException>().WithMessage("Argument for ComputeFunc must be provided");
    }
  }
}
