using System.Linq;
using FluentAssertions;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Surfaces
{
  public class ToFromBinary_TINSurfaceResult
  {
    [Fact]
    public void Test_TINSurfaceRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TINSurfaceResult>();
    }

    [Fact]
    public void Test_TINSurfaceRequestResponse_ErrorResult()
    {
      var result = new TINSurfaceResult
      {
        ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout,
        data = null
      };

      SimpleBinarizableInstanceTester.TestClass(result);
    }

    [Fact]
    public void Test_TINSurfaceRequestResponse_SuccessResult()
    {
      var result = new TINSurfaceResult
      {
        ResultStatus = RequestErrorStatus.OK,
        data = Enumerable.Range(0, 100).Select(x => (byte)x).ToArray()
    };

      var roundTripResult = SimpleBinarizableInstanceTester.TestClass(result);

      roundTripResult.member.Should().BeEquivalentTo(result);
    }
  }
}
