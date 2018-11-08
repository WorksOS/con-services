using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CombinedFilter
  {
    [Fact]
    public void ToFromBinary_CombinedFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CombinedFilter>("Empty CombinedFilter not same after round trip serialisation");
    }
  }
}

