using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CellPassAttributeFilterModel
  {
    [Fact]
    public void Test_CellPassAttributeFilterModel_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellPassAttributeFilterModel>();
    }
  }
}
