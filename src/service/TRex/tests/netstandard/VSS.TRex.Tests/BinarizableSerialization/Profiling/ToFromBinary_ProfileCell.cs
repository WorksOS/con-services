using VSS.TRex.Profiling;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileCell
  {
    [Fact]
    public void Test_ProfileCell_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<ProfileCell>("Empty ProfileCell not same after round trip serialisation");
    }
  }
}
