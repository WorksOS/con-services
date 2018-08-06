using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class InUseSubGridMapTests
    {
      [Fact]
      public void InUseSubGridMapTests_Creation()
      {
        InUseSubGridMap map = new InUseSubGridMap();

        Assert.True(map.TriangleScanInvocationNumber == 0);
        Assert.True(map.InUseMap == null);
      }
    }
}
