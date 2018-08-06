using VSS.TRex.Exports.Surfaces.GridDecimator;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class CachedSubGridMapTests
    {
      [Fact]
      public void CachedSubGridMap_Creation()
      {
        CachedSubGridMap map = new CachedSubGridMap();

        Assert.Null(map.SubGrid);
        Assert.True(map.TriangleScanInvocationNumber == 0);
      }
    }
}
