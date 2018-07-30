using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
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
