using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CellPassAttributeFilter
  {
    [Fact]
    public void Test_CellPassAttributeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellPassAttributeFilter>();
    }
  }
}
