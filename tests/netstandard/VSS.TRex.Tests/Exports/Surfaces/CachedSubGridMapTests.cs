using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
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
