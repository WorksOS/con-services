using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Surfaces
{
  public class ToFromBinary_TINSurfaceResponse
  {
    [Fact]
    public void Test_TINSurfaceRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestNonBinarizableClass<TINSurfaceRequestResponse>();
    }

    [Fact]
    public void Test_TINSurfaceRequestResponse()
    {
      var response = new TINSurfaceRequestResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        TIN = new TrimbleTINModel()
      };

      SimpleBinarizableInstanceTester.TestNonBinarizableClass<TINSurfaceRequestResponse>();
    }
  }
}
