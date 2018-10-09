using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CellSpatialFilterModel
  {
    [Fact]
    public void Test_CellSpatialFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellSpatialFilterModel>();
    }
  }
}
